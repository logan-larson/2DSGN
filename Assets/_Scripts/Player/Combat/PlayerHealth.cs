using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;

/**
<summary>
PlayerHealth is responsible for syncing the health of the player.
</summary>
*/
public class PlayerHealth : NetworkBehaviour
{
    [SyncVar]
    public int Health = 100;

    public UnityEvent<bool> OnDeath;

    [SerializeField]
    private GameObject _damageIndicatorPrefab;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        OnDeath = OnDeath ?? new UnityEvent<bool>();
    }

    [Server]
    public void TakeDamage(int damage)
    {

        Health = PlayerManager.Instance.Players[gameObject.GetInstanceID()].Health;

        // Spawn damage indicator
        GameObject damageIndicator = Instantiate(_damageIndicatorPrefab, transform.position, Quaternion.identity);
        damageIndicator.GetComponentInChildren<DamageIndicator>().SetDamageValue(damage);

        if (base.IsHost) return;

        // Spawn damage indicator on clients
        SpawnDamageIndicatorObserversRpc(transform.position, damage);
    }

    [ObserversRpc]
    private void SpawnDamageIndicatorObserversRpc(Vector3 position, int damage)
    {
        GameObject damageIndicator = Instantiate(_damageIndicatorPrefab, position, Quaternion.identity);
        damageIndicator.GetComponentInChildren<DamageIndicator>().SetDamageValue(damage);
    }

    [Server]
    public void ResetHealth()
    {
        Health = 100;
    }

}
