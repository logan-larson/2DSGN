using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

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

    private Vector3 _aimDirection = Vector3.zero;

    [SyncVar(OnChange = nameof(OnChangeFlipY))]
    public bool FlipY = false;

    private void OnChangeFlipY(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            //CurrentWeapon.FlipY(true);
        }
        else
        {
            //CurrentWeapon.FlipY(false);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _inputSystem = _inputSystem ?? GetComponent<InputSystem>();

        _input = _inputSystem.InputValues;

        // Set defaults
        SetFlipY(false);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Set defaults
        SetFlipY(false);
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
    public void SetFlipY(bool flipY)
    {
        FlipY = flipY;
    }

}
