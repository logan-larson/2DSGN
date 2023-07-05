using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class WeaponPickup : NetworkBehaviour
{
    public SpriteRenderer WeaponSprite;
    public string Name;

    public void ShowHighlight()
    {
        WeaponSprite.color = Color.blue;
    }

    public void HideHighlight()
    {
        WeaponSprite.color = Color.white;
    }
}