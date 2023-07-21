using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FishNet.Object;
using FishNet.Connection;
using System.Linq;

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
    
    public Dictionary<int, Player> Players = new Dictionary<int, Player>();

    public void DamagePlayer(int playerID, int damage, int attackerID, string weaponName, NetworkConnection playerConn = null)
    {
        if (!base.IsServer) return;

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

        var attackerUsername = attackerID == -1 ? "Suicide" : Players[attackerID].Username;

        OnPlayerKilled.Invoke(Players[playerID].Username, attackerUsername, weaponName);

        Players[playerID].PlayerHealth.OnDeath.Invoke();
    }

    public void RespawnPlayer(int playerID)
    {
        if (!base.IsServer) return;

        Players[playerID].Health = 100;

        Players[playerID].IsDead = false;

        Players[playerID].PlayerHealth.ResetHealth();
    }   

    public void SetUsername(int playerID, string username)
    {
        if (!base.IsServer) return;

        Players[playerID].Username = username;
    }

    public Vector3 GetSpawnPosition()
    {
        Transform[] playerPositions = Players.Values.Where(p => !p.IsDead).Select(p => p.GameObject.transform).ToArray();

        // Find spawn point furthest away from all players
        Vector3 furthestSpawnPoint = Vector3.zero;
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
        public string Username { get; set; }
        public int Health { get; set; }
        public bool IsDead { get; set; }
        public PlayerHealth PlayerHealth { get; set; }
        public GameObject GameObject { get; set; }
        public NetworkConnection Connection { get; set; }
    }
}
