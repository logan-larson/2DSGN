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
        _modeManager = _modeManager ?? GetComponentInParent<ModeManager>();

        _input = _inputSystem.InputValues;

        _modeManager.OnChangeToParkour.AddListener(OnChangeToParkourMode);
        _modeManager.OnChangeToCombat.AddListener(OnChangeToCombatMode);

        _playerHealth.OnDeath.AddListener(OnDeath);

        // Set defaults
        SetWeaponShow(false);
        SetFlipY(false);

        Debug.Log($"WeaponHolder OwnerID: {base.OwnerId}");

        // CurrentWeapon.IsEquipped = true;
        // CurrentWeapon.SetEquipped(true);
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

    public void EquipWeapon(GameObject weapon, Vector3 dropPosition)
    {
        if (!base.IsOwner) return;

        // Get current weapon's properties for the new weapon to switch to
        var prevWeaponPos = CurrentWeapon.transform.localPosition;
        var prevWeaponRot = CurrentWeapon.transform.localRotation;
        var prevWeaponIsShown = CurrentWeapon.IsShown;
        var prevWeaponIsFlippedY = CurrentWeapon.IsFlippedY;

        // Drop the current weapon where the new weapon to switch to is
        // Need to pass the weapon holder game object to the server because the weapon holder game object is the one that has the weapon component


        // TESTING drop with default weapon
        /*
        if (CurrentWeapon.gameObject != _defaultWeapon)
        {
        */
            DropWeaponServer(CurrentWeapon.gameObject, dropPosition);
        /*
        }
        else
        {
            // If the current weapon is the default weapon, destroy it
            CurrentWeapon.gameObject.GetComponent<NetworkObject>().Despawn();
        }
        */

        // Equip the new weapon with the old weapon's visual properties
        EquipWeaponServer(weapon, prevWeaponPos, prevWeaponRot, prevWeaponIsShown, prevWeaponIsFlippedY);

        CurrentWeapon = weapon.GetComponent<Weapon>();

        CurrentWeapon.Equip(transform, prevWeaponPos, prevWeaponRot);

        CurrentWeapon.Show(prevWeaponIsShown);
        CurrentWeapon.FlipY(prevWeaponIsFlippedY);
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

    [ServerRpc(RequireOwnership = true)]
    private void DropWeaponServer(GameObject weaponGameObj, Vector3 dropPosition)
    {
        weaponGameObj.GetComponent<NetworkObject>().RemoveOwnership();
        weaponGameObj.GetComponent<Weapon>().Drop(dropPosition);
    }


    [ServerRpc(RequireOwnership = true)]
    private void EquipWeaponServer(GameObject weaponGameObj, Vector3 equipPosition, Quaternion equipRotation, bool showWeapon, bool flipY, NetworkConnection newOwner = null)
    {
        if (newOwner != null)
        {
            weaponGameObj.GetComponent<NetworkObject>().GiveOwnership(newOwner);
        }
    }
}
