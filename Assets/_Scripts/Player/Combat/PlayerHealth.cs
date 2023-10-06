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

    public UnityEvent<bool, Vector3> OnDeath;

    [SerializeField]
    private GameObject _damageIndicatorPrefab;

    [SerializeField]
    private GameObject _deathIndicatorPrefab;

    private void Start() { }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        OnDeath = OnDeath ?? new UnityEvent<bool, Vector3>();

        OnDeath.AddListener(SpawnDeathIndicator);
    }

    private void SpawnDeathIndicator(bool isSuicide, Vector3 deathPosition)
    {
        Instantiate(_deathIndicatorPrefab, deathPosition, Quaternion.identity);

        SpawnDeathIndicatorObserversRpc(deathPosition);
    }

    [ObserversRpc]
    private void SpawnDeathIndicatorObserversRpc(Vector3 position)
    {
        if (base.IsHost) return;

        Instantiate(_deathIndicatorPrefab, position, Quaternion.identity);
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int damage)
    {
        PlayerManager.Instance.DamagePlayer(gameObject.GetInstanceID(), damage, gameObject.GetInstanceID(), "Revolver");
    }

    [Server]
    public void TakeDamage(int damage)
    {
        Health = PlayerManager.Instance.Players[gameObject.GetInstanceID()].Health;

        // Spawn damage indicator
        GameObject damageIndicator = Instantiate(_damageIndicatorPrefab, transform.position, Quaternion.identity);
        damageIndicator.GetComponentInChildren<DamageIndicator>().SetDamageValue(damage);

        // Spawn damage indicator on clients
        SpawnDamageIndicatorObserversRpc(transform.position, damage);
    }

    [ObserversRpc]
    private void SpawnDamageIndicatorObserversRpc(Vector3 position, int damage)
    {
        if (base.IsHost) return;

        GameObject damageIndicator = Instantiate(_damageIndicatorPrefab, position, Quaternion.identity);
        damageIndicator.GetComponentInChildren<DamageIndicator>().SetDamageValue(damage);
    }

    [Server]
    public void ResetHealth()
    {
        var healthDifference = 100 - Health;
        
        PlayerManager.Instance.Players[gameObject.GetInstanceID()].Health = 100;
        Health = 100;

        GameObject healIndicator = Instantiate(_damageIndicatorPrefab, transform.position, Quaternion.identity);
        var healIndicatorText = healIndicator.GetComponentInChildren<DamageIndicator>();
        healIndicatorText.SetDamageValue(healthDifference);
        healIndicatorText.SetColor(Color.green);

        SpawnHealIndicatorObserversRpc(transform.position, healthDifference);
    }

    [ObserversRpc]
    private void SpawnHealIndicatorObserversRpc(Vector3 position, int healthDifference)
    {
        if (base.IsHost) return;

        GameObject healIndicator = Instantiate(_damageIndicatorPrefab, position, Quaternion.identity);
        var healIndicatorText = healIndicator.GetComponentInChildren<DamageIndicator>();
        healIndicatorText.SetDamageValue(healthDifference);
        healIndicatorText.SetColor(Color.green);
    }

}
