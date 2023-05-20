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

    public int CurrentOwnerId = -1;

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

    // TESTING - owner check
    private void Update()
    {
        if (CurrentOwnerId != base.OwnerId)
        {
            CurrentOwnerId = base.OwnerId;
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

    [Server]
    public void Equip(Transform weaponHolder, Vector3 position, Quaternion rotation)
    {

        if (!base.IsOwner) return;

        transform.SetParent(weaponHolder);

        transform.localPosition = position;
        transform.localRotation = rotation;

        HideHighlight();
        IsEquipped = true;
    }

    [Server]
    public void Drop(Vector3 dropPosition)
    {
        Debug.Log($"Dropping {WeaponInfo.Name}");

        if (!base.IsOwner) return;

        Debug.Log("Passed owner check");

        IsEquipped = false;


        var weaponPickups = GameObject.FindWithTag("WeaponPickups");
        transform.SetParent(weaponPickups.transform);

        transform.position = dropPosition;

        IsEquipped = false;
        Show(true);
        HideHighlight();
    }

    [ServerRpc]
    private void SetEquippedServer(bool equipped)
    {
        IsEquipped = equipped;
    }
}