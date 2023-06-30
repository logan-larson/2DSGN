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
    public GameObject WeaponPickup;

    public float CurrentBloom = 0f;

    [SyncVar (OnChange = nameof(SetSpriteShown))]
    public bool IsShown = false;

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

        //SetShownServer(true);
        SetFlippedYServer(false);
        SetMuzzleFlashShown(false);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        MuzzleFlashSprite.enabled = false;

        /*
        IsShown = true;
        IsFlippedY = false;
        IsMuzzleFlashShown = false;
        */
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