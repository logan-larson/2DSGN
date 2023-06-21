using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    public WeaponInfo WeaponInfo;
    public SpriteRenderer WeaponSprite;
    public SpriteRenderer MuzzleFlashSprite;
    public TrailRenderer BulletTrailRenderer;

    public float CurrentBloom = 0f;

    [SyncVar]
    public bool IsEquipped = false;
    [SyncVar]
    public bool IsShown = false;
    [SyncVar]
    public bool IsFlippedY = false;
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

        // Determine if weapon is equipped.
        if (transform.parent.GetComponent<WeaponHolder>() != null)
        {
            SetEquippedServer(true);
        }
        else
        {
            SetEquippedServer(false);
        }

        SetShownServer(true);
        SetFlippedYServer(false);
        SetMuzzleFlashShown(false);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        MuzzleFlashSprite.enabled = false;

        // Determine if weapon is equipped.
        if (transform.parent.GetComponent<WeaponHolder>() != null)
        {
            IsEquipped = true;
        }
        else
        {
            IsEquipped = false;
        }

        IsShown = true;
        IsFlippedY = false;
        IsMuzzleFlashShown = false;
    }

    public void Show(bool show)
    {
        WeaponSprite.enabled = show;
        IsShown = show;
    }

    public void FlipY(bool flipY)
    {
        WeaponSprite.flipY = flipY;
        MuzzleFlashSprite.flipY = flipY;
        IsFlippedY = flipY;
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

    public void ShowHighlight()
    {
        WeaponSprite.color = Color.blue;
    }

    public void HideHighlight()
    {
        WeaponSprite.color = Color.white;
    }

    // [Server]
    public void Equip(Transform weaponHolder, Vector3 position, Quaternion rotation)
    {
        transform.localPosition = position;
        transform.localRotation = rotation;

        HideHighlight();
    }

    public void Drop(Vector3 dropPosition)
    {
        transform.position = dropPosition;

        IsEquipped = false;
        Show(true);
        HideHighlight();
    }

    [Server]
    public void SetEquipped(bool equipped)
    {
        IsEquipped = equipped;
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
    private void SetEquippedServer(bool equipped)
    {
        IsEquipped = equipped;
    }

    [ServerRpc]
    private void SetMuzzleFlashShown(bool isMuzzleFlashShown)
    {
        IsMuzzleFlashShown = isMuzzleFlashShown;
    }
}