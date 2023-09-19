using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthNut : NetworkBehaviour
{
    public SpriteRenderer HealthNutSprite;

    [SyncVar]
    public int HealthNutID = 0;

    [SyncVar (OnChange = nameof(ToggleIsAvailable))]
    public bool IsAvailable = true;

    private void ToggleIsAvailable(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            HealthNutSprite.enabled = true;
        }
        else
        {
            HealthNutSprite.enabled = false;
        }
    }

    public void ShowHighlight()
    {
        HealthNutSprite.color = Color.blue;
    }

    public void HideHighlight()
    {
        HealthNutSprite.color = Color.white;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        HealthNutID = UnityEngine.Random.Range(0, 1000);
    }

    public void Pickup()
    {
        if (!base.IsServer) return;

        IsAvailable = false;

        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(5);

        IsAvailable = true;
    }
}
