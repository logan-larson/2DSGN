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
    private InputSystem _inputSystem;

    [SerializeField]
    private PlayerInputValues _input;

    [SerializeField]
    private CombatSystem _combatSystem;

    public Weapon CurrentWeapon;

    private Vector3 _aimDirection = Vector3.zero;

    [SyncVar(OnChange = nameof(OnChangeWeaponShow))]
    public bool WeaponShow = false;

    private void OnChangeWeaponShow(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            // _spriteRenderer.enabled = true;
            CurrentWeapon.ShowWeapon();
        }
        else
        {
            // _spriteRenderer.enabled = false;
            CurrentWeapon.HideWeapon();
        }
    }

    [SyncVar(OnChange = nameof(OnChangeFlipY))]
    public bool FlipY = false;

    private void OnChangeFlipY(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            // _spriteRenderer.flipY = true;
            CurrentWeapon.FlipY(true);
        }
        else
        {
            // _spriteRenderer.flipY = false;
            CurrentWeapon.FlipY(false);
        }
    }

    private void Start() {
        CurrentWeapon = CurrentWeapon ?? GetComponentInChildren<Weapon>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            _inputSystem = _inputSystem ?? GetComponent<InputSystem>();
            _input = _inputSystem.InputValues;
        }
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

    public void EquipWeapon(Weapon weapon, Vector3 dropPosition)
    {
        var droppedWeaponPos = CurrentWeapon.transform.localPosition;
        var droppedWeaponRot = CurrentWeapon.transform.localRotation;

        if (CurrentWeapon != null)
        {
            CurrentWeapon.Drop(dropPosition);
        }

        CurrentWeapon = weapon;

        CurrentWeapon.Equip(transform, droppedWeaponPos, droppedWeaponRot);
    }
}
