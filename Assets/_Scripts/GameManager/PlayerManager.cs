using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FishNet.Object;
using FishNet.Connection;
using System.Linq;
using FishNet.Transporting;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public UnityEvent<string, string, string> OnPlayerKilled;

    [SerializeField]
    private List<Vector3> _spawnPositions = new List<Vector3>();

    private void Awake()
    {
        Instance = this;

        OnPlayerKilled = OnPlayerKilled ?? new UnityEvent<string, string, string>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            if (Players.Count() == 0) return;

            var player = Players.Select(x => (x.Key, x.Value)).First(x => x.Value.Connection == conn);

            Players.Remove(player.Key);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var position in _spawnPositions)
        {
            Gizmos.DrawSphere(position, 1f);
        }
    }

    public Dictionary<int, Player> Players = new Dictionary<int, Player>();

    public void DamagePlayer(int playerID, int damage, int attackerID, string weaponName, NetworkConnection playerConn = null)
    {
        if (!base.IsServer) return;

        if (attackerID == -1)
        {
            Players[playerID].PlayerHealth.OnDeath.Invoke(true);
            return;
        }

        var player = Players[playerID];
        var attacker = Players[attackerID];

        if (player.IsDead) return;

        player.Health -= damage;

        player.PlayerHealth.TakeDamage(damage);


        if (player.Health <= 0)
        {
            player.IsDead = true;

            PlayerKilled(playerID, attackerID, weaponName);

            if (playerConn != null)
            {
                player.GameObject.GetComponent<CameraManager>().SetPlayerFollowTargetRpc(player.Connection, attacker.GameObject);
            }
        }    
    }

    private void PlayerKilled(int playerID, int attackerID, string weaponName)
    {
        var attackerUsername = attackerID == playerID ? "Suicide" : Players[attackerID].Username;

        GameStateManager.Instance.PlayerKilled(playerID, attackerID);

        GameStateManager.Instance.OnPlayerKilled.Invoke();
        OnPlayerKilled.Invoke(Players[playerID].Username, attackerUsername, weaponName);

        Players[playerID].PlayerHealth.OnDeath.Invoke(false);
    }

    public void RespawnPlayer(int playerID)
    {
        if (!base.IsServer) return;

        var player = Players[playerID];

        player.Health = 100;

        player.IsDead = false;

        player.PlayerHealth.ResetHealth();
    }   

    public void SetUsername(int playerID, string username)
    {
        if (!base.IsServer) return;

        Players[playerID].Username = username;
    }

    public Vector3 GetSpawnPosition()
    {
        Transform[] playerPositions = Players.Values.Where(p => !p.IsDead).Select(p => p.GameObject.transform).ToArray();

        Debug.Log(playerPositions.Length);

        // Find spawn point furthest away from all players
        Vector3 furthestSpawnPoint = _spawnPositions[0];
        float maxDistance = 0f;

        foreach (Vector3 spawnPoint in _spawnPositions)
        {
            // Calculate the distance from each player to the spawn point
            float distanceSum = 0f;

            foreach (Transform playerPos in playerPositions)
            {
                distanceSum += Vector3.Distance(spawnPoint, playerPos.position);
            }

            // Check if the distance is greater than the previous max distance
            if (distanceSum > maxDistance)
            {
                maxDistance = distanceSum;
                furthestSpawnPoint = spawnPoint;
            }
        }

        // furthestSpawnPoint will contain the spawn point furthest from all players
        return furthestSpawnPoint;
    }

    public class Player
    {
        public PlayerHealth PlayerHealth { get; set; }
        public GameObject GameObject { get; set; }
        public NetworkConnection Connection { get; set; }
        public string Username { get; set; } = "user123";
        public int Health { get; set; } = 100;
        public bool IsDead { get; set; } = false;
    }
}
