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

        if (Input.GetKeyDown(KeyCode.P))
        {
            ForceRespawnServerRpc(gameObject.GetInstanceID());
        }
    }

    [ServerRpc]
    private void ForceRespawnServerRpc(int id)
    {
        PlayerManager.Instance.DamagePlayer(id, 100, id, "Revolver");
    }

    private void Respawn()
    {
        transform.SetPositionAndRotation(new Vector3(0, 100f, 0), Quaternion.Euler(new Vector3(0f, 0f, 0f)));

        // Delay respawn
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        // No delay for now, until I can make the player invulnerable
        yield return new WaitForSeconds(3f);

        OnRespawn.Invoke();

        Vector3 spawnPosition = PlayerManager.Instance.GetSpawnPosition();

        PlayerManager.Instance.RespawnPlayer(gameObject.GetInstanceID());

        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;
    }
}
