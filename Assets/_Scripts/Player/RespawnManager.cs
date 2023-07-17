using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FishNet.Object;

public class RespawnManager : NetworkBehaviour
{

    public UnityEvent OnRespawn;

    [SerializeField]
    private PlayerHealth _playerHealth;

    [SerializeField]
    private List<Vector3> _spawnPositions = new List<Vector3>();

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        OnRespawn = OnRespawn ?? new UnityEvent();

        _playerHealth.OnDeath.AddListener(Respawn);
    }

    private void Update()
    {
        if (!base.IsOwner) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerManager.Instance.DamagePlayer(gameObject.GetInstanceID(), 100, -1, "Revolver");
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayerManager.Instance.DamagePlayer(gameObject.GetInstanceID(), 7, -1, "Revolver");
        }
    }

    private void Respawn()
    {
        // Delay respawn
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        // No delay for now, until I can make the player invulnerable
        yield return new WaitForSeconds(0f);

        OnRespawn.Invoke();

        PlayerManager.Instance.RespawnPlayer(gameObject.GetInstanceID());

        Vector3 spawnPosition = _spawnPositions[Random.Range(0, _spawnPositions.Count)];

        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;
    }
}
