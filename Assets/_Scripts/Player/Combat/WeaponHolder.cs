using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.InputSystem;

/**
<summary>
WeaponHolder is responsible for displaying the weapon sprite and pointing it in the aim direction.
</summary>
*/
public class WeaponHolder : NetworkBehaviour
{
    [SerializeField]
    private MovementSystem _movementSystem;

    [SerializeField]
    private PlayerHealth _playerHealth;

    [SerializeField]
    private InputSystem _inputSystem;

    [SerializeField]
    private PlayerInputValues _input;

    [SerializeField]
    private CombatSystem _combatSystem;

    [SerializeField]
    private GameObject _defaultWeapon;

    public Weapon CurrentWeapon;

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

        _inputSystem = _inputSystem ?? GetComponent<InputSystem>();
        _movementSystem = _movementSystem ?? GetComponent<MovementSystem>();

        _input = _inputSystem.InputValues;

        _movementSystem.OnChangeToCombatMode += OnChangeToCombatMode;
        _movementSystem.OnChangeToParkourMode += OnChangeToParkourMode;

        _playerHealth.OnDeath.AddListener(OnDeath);

        // Set defaults
        SetWeaponShow(false);
        SetFlipY(false);
        CurrentWeapon.IsEquipped = true;
    }

    private void OnChangeToCombatMode(bool inCombat)
    {
        SetWeaponShow(true);
    }

    private void OnChangeToParkourMode(bool inCombat)
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

    public void EquipWeapon(GameObject weapon, Vector3 dropPosition)
    {
        // Get current weapon's visual properties
        var droppedWeaponPos = CurrentWeapon.transform.localPosition;
        var droppedWeaponRot = CurrentWeapon.transform.localRotation;
        var droppedWeaponIsShown = CurrentWeapon.IsShown;
        var droppedWeaponIsFlippedY = CurrentWeapon.IsFlippedY;
        // droppedWeaponPos = new Vector3();

        // Drop the current weapon where the new weapon to switch to is
        // Need to pass the weapon holder game object to the server because the weapon holder game object is the one that has the weapon component
        if (CurrentWeapon.gameObject != _defaultWeapon)
        {
            DropWeaponServer(CurrentWeapon.gameObject, dropPosition);
        }
        else
        {
            // If the current weapon is the default weapon, destroy it
            Destroy(CurrentWeapon.gameObject);
        }

        // Equip the new weapon with the old weapon's visual properties
        EquipWeaponServer(weapon, droppedWeaponPos, droppedWeaponRot, droppedWeaponIsShown, droppedWeaponIsFlippedY);
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

        // Equip the default weapon
        EquipWeaponServer(_defaultWeapon, Vector3.zero, Quaternion.identity, false, false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropWeaponServer(GameObject weaponGameObj, Vector3 dropPosition)
    {
        DropWeaponObserver(weaponGameObj, dropPosition);
    }

    [ObserversRpc]
    private void DropWeaponObserver(GameObject weaponGameObj, Vector3 dropPosition)
    {
        weaponGameObj.GetComponent<Weapon>().Drop(dropPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void EquipWeaponServer(GameObject weaponGameObj, Vector3 equipPosition, Quaternion equipRotation, bool showWeapon, bool flipY)
    {
        EquipWeaponObserver(weaponGameObj, equipPosition, equipRotation, showWeapon, flipY);
    }

    [ObserversRpc]
    private void EquipWeaponObserver(GameObject weapon, Vector3 equipPosition, Quaternion equipRotation, bool showWeapon, bool flipY)
    {
        CurrentWeapon = weapon.GetComponent<Weapon>();

        CurrentWeapon.Equip(transform, equipPosition, equipRotation);

        CurrentWeapon.Show(WeaponShow);
        CurrentWeapon.FlipY(FlipY);
    }
}
