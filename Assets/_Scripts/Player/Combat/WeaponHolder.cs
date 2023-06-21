using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using UnityEngine.InputSystem;

/**
<summary>
WeaponHolder is responsible for displaying the weapon sprite and pointing it in the aim direction.
</summary>
*/
public class WeaponHolder : NetworkBehaviour
{
    [SerializeField]
    private PlayerHealth _playerHealth;

    [SerializeField]
    private RespawnManager _respawnManager;

    [SerializeField]
    private InputSystem _inputSystem;

    [SerializeField]
    private PlayerInputValues _input;

    [SerializeField]
    private CombatSystem _combatSystem;

    [SerializeField]
    private ModeManager _modeManager;

    [SerializeField]
    private GameObject _defaultWeapon;

    public Weapon CurrentWeapon;

    [SerializeField]
    private GameObject _currentWeaponGO;

    [SerializeField]
    private List<GameObject> _weapons;

    private Vector3 _aimDirection = Vector3.zero;

    [SyncVar(OnChange = nameof(OnChangeWeaponShow))]
    public bool WeaponShow = false;

    private void OnChangeWeaponShow(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            CurrentWeapon.Show(true);
        }
        else
        {
            CurrentWeapon.Show(false);
        }
    }

    [SyncVar(OnChange = nameof(OnChangeFlipY))]
    public bool FlipY = false;

    private void OnChangeFlipY(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            CurrentWeapon.FlipY(true);
        }
        else
        {
            CurrentWeapon.FlipY(false);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        CurrentWeapon = CurrentWeapon ?? gameObject.GetComponent<Weapon>();
        _currentWeaponGO = CurrentWeapon.gameObject;
        var weapons = gameObject.GetComponentsInChildren<Weapon>(true);

        foreach (var w in weapons)
        {
            _weapons.Add(w.gameObject);
        }

        _inputSystem = _inputSystem ?? GetComponent<InputSystem>();
        _modeManager = _modeManager ?? GetComponentInParent<ModeManager>();

        _input = _inputSystem.InputValues;

        _modeManager.OnChangeToParkour.AddListener(OnChangeToParkourMode);
        _modeManager.OnChangeToCombat.AddListener(OnChangeToCombatMode);

        _playerHealth.OnDeath.AddListener(OnDeath);
        _respawnManager.OnRespawn.AddListener(OnRespawn);

        // Set defaults
        SetWeaponShow(false);
        SetFlipY(false);
    }

    private void OnChangeToCombatMode()
    {
        SetWeaponShow(true);
    }

    private void OnChangeToParkourMode()
    {
        SetWeaponShow(false);
    }

    public void Update()
    {
        if (!base.IsOwner) return;

        _aimDirection = _combatSystem.AimDirection;

        transform.rotation = Quaternion.LookRotation(Vector3.forward, _aimDirection) * Quaternion.Euler(0f, 0f, 90f);

        // If past vertical, flip sprite.
        float angleDifference = Mathf.DeltaAngle(transform.parent.rotation.eulerAngles.z, transform.rotation.eulerAngles.z);
        if (angleDifference > 90f || angleDifference < -90f)
        {
            SetFlipY(true);
        }
        else
        {
            SetFlipY(false);
        }
    }

    [ServerRpc]
    public void SetWeaponShow(bool show)
    {
        WeaponShow = show;
    }

    [ServerRpc]
    public void SetFlipY(bool flipY)
    {
        FlipY = flipY;
    }

    public void SwapWeapons(GameObject weapon)
    {
        if (!base.IsOwner) return;

        if (CurrentWeapon == null) return;

        // TODO: Implement this new system!
        // 1. Check if the weapon is already equipped, if so, return.
        // 2. Disable the current weapon.
        // 3. Enable the new weapon.
        // 4. Destroy the weapon pickup.
        // 5. Spawn the old weapon pickup in place of the new one.

        // Get current weapon's properties for the new weapon to switch to
        var prevWeaponPos = CurrentWeapon.transform.localPosition;
        var prevWeaponRot = CurrentWeapon.transform.localRotation;
        var prevWeaponIsShown = CurrentWeapon.IsShown;
        var prevWeaponIsFlippedY = CurrentWeapon.IsFlippedY;

        var dropPosition = weapon.transform.position;

        // Drop the current weapon where the new weapon to switch to is
        if (CurrentWeapon.gameObject != _defaultWeapon)
        {
            DropWeaponServer(CurrentWeapon.gameObject, dropPosition);
        }
        else
        {
            // If the current weapon is the default weapon, destroy it
            DestroyWeaponServer(CurrentWeapon.gameObject);
        }

        // Equip the new weapon with the old weapon's visual properties
        EquipWeaponServer(weapon, prevWeaponPos, prevWeaponRot, prevWeaponIsShown, prevWeaponIsFlippedY, base.Owner);

        CurrentWeapon = weapon.GetComponent<Weapon>();
    }

    private void OnDeath()
    {
        // Drop the current weapon, if it isn't the default weapon
        if (CurrentWeapon.gameObject != _defaultWeapon)
        {
            DropWeaponServer(CurrentWeapon.gameObject, transform.position);
        }
        else
        {
            // If the current weapon is the default weapon, destroy it
            Destroy(CurrentWeapon.gameObject);
        }

        CurrentWeapon = null;
    }

    private void OnRespawn()
    {
        // Equip the default weapon
        EquipDefaultWeapon();
    }

    private void EquipDefaultWeapon()
    {
        if (!base.IsOwner) return;

        SpawnDefaultWeapon(base.Owner);
    }

    [ServerRpc(RequireOwnership = true)]
    private void SpawnDefaultWeapon(NetworkConnection conn)
    {
        var weapon = Instantiate(_defaultWeapon, transform.position, Quaternion.identity);

        base.Spawn(weapon, conn);

        weapon.transform.SetParent(transform);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        SetCurrentWeapon(base.Owner, weapon);
    }

    [TargetRpc]
    private void SetCurrentWeapon(NetworkConnection conn, GameObject weapon)
    {
        weapon.transform.SetParent(transform);

        CurrentWeapon = weapon.GetComponent<Weapon>();
    }

    [ServerRpc(RequireOwnership = true)]
    private void DestroyWeaponServer(GameObject weaponGameObj)
    {
        weaponGameObj.GetComponent<NetworkObject>().Despawn();
    }

    [ServerRpc(RequireOwnership = true)]
    private void DropWeaponServer(GameObject weaponGameObj, Vector3 dropPosition)
    {
        weaponGameObj.GetComponent<NetworkObject>().RemoveOwnership();

        var weapon = weaponGameObj.GetComponent<Weapon>();

        var weaponPickups = GameObject.FindWithTag("WeaponPickups");
        weapon.transform.SetParent(weaponPickups.transform);

        weapon.transform.position = dropPosition;
        weapon.transform.rotation = Quaternion.identity;

        weapon.SetEquipped(false);
    
        weapon.Show(true);

        DropWeaponObservers(weaponGameObj, dropPosition);


        // weapon.Drop(dropPosition);
    }

    [ObserversRpc]
    private void DropWeaponObservers(GameObject weaponGameObj, Vector3 dropPosition)
    {
        var weapon = weaponGameObj.GetComponent<Weapon>();

        weapon.Show(true);

        // var weaponPickups = GameObject.FindWithTag("WeaponPickups");
        // weapon.transform.SetParent(weaponPickups.transform);

        // weapon.Drop(dropPosition);
    }

    [ServerRpc(RequireOwnership = true)]
    private void EquipWeaponServer(GameObject weaponGameObj, Vector3 equipPosition, Quaternion equipRotation, bool showWeapon, bool flipY, NetworkConnection newOwner)
    {
        weaponGameObj.GetComponent<NetworkObject>().GiveOwnership(newOwner);

        weaponGameObj.transform.SetParent(transform);

        var weapon = weaponGameObj.GetComponent<Weapon>();

        weapon.Show(showWeapon);
        weapon.FlipY(flipY);

        weapon.transform.position = equipPosition;
        weapon.transform.rotation = equipRotation;

        weapon.SetEquipped(true);

        // weapon.Equip(transform, equipPosition, equipRotation);

        EquipWeaponObservers(weaponGameObj, equipPosition, equipRotation, showWeapon, flipY);
    }

    [ObserversRpc]
    private void EquipWeaponObservers(GameObject weaponGameObj, Vector3 equipPosition, Quaternion equipRotation, bool showWeapon, bool flipY)
    {
        weaponGameObj.transform.SetParent(transform);

        var weapon = weaponGameObj.GetComponent<Weapon>();

        weapon.Show(showWeapon);
        weapon.FlipY(flipY);


        // weapon.Equip(transform, equipPosition, equipRotation);
    }
}
