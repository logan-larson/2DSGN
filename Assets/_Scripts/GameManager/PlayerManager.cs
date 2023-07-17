using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FishNet.Object;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public UnityEvent<string, string, string> OnPlayerKilled;

    private void Awake()
    {
        Instance = this;

        OnPlayerKilled = OnPlayerKilled ?? new UnityEvent<string, string, string>();
    }
    
    public Dictionary<int, Player> Players = new Dictionary<int, Player>();

    public void DamagePlayer(int playerID, int damage, int attackerID, string weaponName)
    {
        if (!base.IsServer) return;

        Players[playerID].Health -= damage;

        Players[playerID].PlayerHealth.TakeDamage(damage);

        if (Players[playerID].Health <= 0 && !Players[playerID].IsDead)
        {
            Players[playerID].IsDead = true;

            PlayerKilled(playerID, attackerID, weaponName);
        }    
    }

    private void PlayerKilled(int playerID, int attackerID, string weaponName)
    {

        //Debug.Log($"Player {Players[playerID].Username} killed by {Players[attackerID].Username}");
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

    public class Player
    {
        public string Username { get; set; }
        public int Health { get; set; }
        public bool IsDead { get; set; }
        public PlayerHealth PlayerHealth { get; set; }
    }
}
