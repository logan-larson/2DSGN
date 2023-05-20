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

    public bool IsShown = false;
    public bool IsFlippedY = false;
    public float CurrentBloom = 0f;

    [SyncVar]
    public bool IsEquipped = false;

    public override void OnStartClient()
    {
        base.OnStartClient();

        MuzzleFlashSprite.enabled = false;

        // Determine if weapon is equipped.
        if (transform.parent.GetComponent<WeaponHolder>() != null)
        {
            SetEquippedServer(true);
        }

        if (IsEquipped)
        {
            Show(true);
            FlipY(IsFlippedY);
        }
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

    public void Equip(Transform weaponHolder, Vector3 position, Quaternion rotation)
    {

        if (!base.IsOwner) return;

        transform.SetParent(weaponHolder);
        transform.localPosition = position;
        transform.localRotation = rotation;

        HideHighlight();
        SetEquippedServer(true);
    }

    public void Drop(Vector3 dropPosition)
    {
        // if (!base.IsOwner) return;

        transform.position = dropPosition;

        var weaponPickups = GameObject.FindWithTag("WeaponPickups");
        transform.SetParent(weaponPickups.transform);

        SetEquippedServer(false);
        Show(true);
        HideHighlight();
    }

    public void SetEquipped(bool equipped)
    {
        if (!base.IsOwner) {
            Debug.Log("Not owner");
            return;
        }

        SetEquippedServer(equipped);
    }

    [ServerRpc]
    private void SetEquippedServer(bool equipped)
    {
        IsEquipped = equipped;
    }
}