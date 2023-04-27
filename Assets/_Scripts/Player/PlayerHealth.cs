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

    public UnityEvent OnDeath;

    [SerializeField]
    private TMP_Text _healthText;

    private RespawnManager _respawnManager;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        OnDeath = OnDeath ?? new UnityEvent();

        _respawnManager = GetComponent<RespawnManager>();

        _respawnManager.OnRespawn.AddListener(ResetHealth);
    }

    private void Update()
    {
        _healthText.text = $"Health: {Health.ToString()}";

        if (Health <= 0)
        {
            ResetHealth();
            OnDeath.Invoke();
        }
    }

    public void TakeDamage(int damage)
    {
        if (!base.IsOwner) return;

        Health -= damage;
    }

    private void ResetHealth()
    {
        Health = 100;
    }

}
