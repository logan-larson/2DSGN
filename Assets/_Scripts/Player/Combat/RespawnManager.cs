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
    private MovementSystem _movementSystem;

    [SerializeField]
    private List<Vector3> _spawnPositions = new List<Vector3>();

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        ForceRespawnServerRpc();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        OnRespawn = OnRespawn ?? new UnityEvent();

        _playerHealth.OnDeath.AddListener(Respawn);

        _movementSystem = _movementSystem ?? GetComponent<MovementSystem>();

        GameStateManager.Instance.OnInitiateCountdown.AddListener(() =>
        {
            PlayerManager.Instance.DamagePlayer(gameObject.GetInstanceID(), 100, -1, "Revolver");
        });
    }

    public void ForceRespawn()
    {
        if (!base.IsOwner) return;

        ForceSuicideServerRpc();
    }

    [ServerRpc]
    private void ForceRespawnServerRpc()
    {
        Respawn(true);
    }

    [ServerRpc]
    private void ForceSuicideServerRpc()
    {
        PlayerManager.Instance.DamagePlayer(gameObject.GetInstanceID(), 100, gameObject.GetInstanceID(), "Revolver");
    }

    private void Respawn(bool isGameStart)
    {
        transform.SetPositionAndRotation(new Vector3(0, 78.5f, 0), Quaternion.identity);

        _movementSystem.SetIsRespawning(true);

        // Delay respawn
        StartCoroutine(RespawnCoroutine(isGameStart));
    }

    private IEnumerator RespawnCoroutine(bool isGameStart)
    {
        var delay = isGameStart ? 0f : 3f;

        yield return new WaitForSeconds(delay);

        OnRespawn.Invoke();

        Vector3 spawnPosition = PlayerManager.Instance.GetSpawnPosition();

        PlayerManager.Instance.RespawnPlayer(gameObject.GetInstanceID());

        _movementSystem.SetIsRespawning(false);

        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;
    }
}
