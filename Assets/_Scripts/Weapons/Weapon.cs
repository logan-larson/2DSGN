using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    public WeaponInfo WeaponInfo;
    public SpriteRenderer WeaponSprite;
    public SpriteRenderer MuzzleFlashSprite;
    public TrailRenderer BulletTrailRenderer;
    public GameObject WeaponPickup;

    [SerializeField]
    private CombatSystem _combatSystem;

    [SerializeField]
    private WeaponEquipManager _weaponEquipManager;

    public float CurrentBloom = 0f;

    [SyncVar (OnChange = nameof(SetSpriteShown))]
    public bool IsShown = true;

    private void SetSpriteShown(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            WeaponSprite.enabled = true;
            MuzzleFlashSprite.enabled = false;
        }
        else
        {
            WeaponSprite.enabled = false;
            MuzzleFlashSprite.enabled = false;
        }
    }
    
    [SyncVar (OnChange = nameof(FlipY))]
    public bool IsFlippedY = false;

    private void FlipY(bool oldValue, bool newValue, bool isServer)
    {
        WeaponSprite.flipY = newValue;
        MuzzleFlashSprite.flipY = newValue;
    }

    [SyncVar (OnChange = nameof(ToggleSprite))]
    public bool IsMuzzleFlashShown = false;

    private void ToggleSprite(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            MuzzleFlashSprite.enabled = true;
        }
        else
        {
            MuzzleFlashSprite.enabled = false;
        }
    }   

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _combatSystem = _combatSystem ?? transform.parent.GetComponentInParent<CombatSystem>();
        _weaponEquipManager = _weaponEquipManager ?? transform.parent.GetComponentInParent<WeaponEquipManager>();

        SetShownServer(false);
        SetFlippedYServer(false);
        SetMuzzleFlashShown(false);

        WeaponSprite.enabled = false;
        MuzzleFlashSprite.enabled = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public void Update()
    {
        if (!base.IsOwner) return;

        if (_weaponEquipManager.CurrentWeapon.WeaponInfo.Name != WeaponInfo.Name) return;

        transform.rotation = Quaternion.LookRotation(Vector3.forward, _combatSystem.AimDirection) * Quaternion.Euler(0f, 0f, 90f);

        // If past vertical, flip sprite.
        float angleDifference = Mathf.DeltaAngle(transform.parent.rotation.eulerAngles.z, transform.rotation.eulerAngles.z);

        bool isFlippedY = angleDifference > 90f || angleDifference < -90f;

        if (isFlippedY == IsFlippedY) return; 

        if (base.IsServer)
        {
            IsFlippedY = isFlippedY;
        }
        else
        {
            SetFlippedYServer(isFlippedY);
        }
    }


    public void ShowMuzzleFlash()
    {
        MuzzleFlashSprite.enabled = true;
        StartCoroutine(DisableMuzzleFlash());
    }

    private IEnumerator DisableMuzzleFlash()
    {
        yield return new WaitForSeconds(0.05f);
        MuzzleFlashSprite.enabled = false;
    }

    public void Show()
    {
        Debug.Log($"Showing weapon {WeaponInfo.Name} on client: {base.OwnerId}");

        WeaponSprite.enabled = true;
        MuzzleFlashSprite.enabled = false;
    }   

    public void Hide()
    {
        Debug.Log($"Hiding weapon {WeaponInfo.Name} on client: {base.OwnerId}");

        WeaponSprite.enabled = false;
        MuzzleFlashSprite.enabled = false;
    }   

    [ServerRpc]
    private void SetShownServer(bool isShown)
    {
        IsShown = isShown;
    }

    [ServerRpc]
    private void SetFlippedYServer(bool isFlippedY)
    {
        IsFlippedY = isFlippedY;
    }

    [ServerRpc]
    private void SetMuzzleFlashShown(bool isMuzzleFlashShown)
    {
        IsMuzzleFlashShown = isMuzzleFlashShown;
    }
}