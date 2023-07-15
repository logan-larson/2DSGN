using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class WeaponPickup : NetworkBehaviour
{
    public SpriteRenderer WeaponSprite;
    public string Name;

    [SyncVar]
    public int WeaponID = 0;

    [SyncVar (OnChange = nameof(ToggleIsPickedUp))]
    public bool IsPickedUp = false;

    private void ToggleIsPickedUp(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            WeaponSprite.enabled = false;
        }
        else
        {
            WeaponSprite.enabled = true;
        }
    }   

    public void ShowHighlight()
    {
        WeaponSprite.color = Color.blue;
    }

    public void HideHighlight()
    {
        WeaponSprite.color = Color.white;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // TODO: Change this implementation to a more sophisticated approach
        WeaponID = UnityEngine.Random.Range(0, 1000);
    }

    public void Pickup()
    {
        if (!base.IsServer) return;

        IsPickedUp = true;
    }

    public void Drop()
    {
        if (!base.IsServer) return;

        IsPickedUp = false;
    }
}