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

    public override void OnStartClient()
    {
        base.OnStartClient();

        MuzzleFlashSprite.enabled = false;

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
        IsEquipped = true;
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
}