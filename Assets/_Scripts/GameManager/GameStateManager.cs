using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEngine.Events;

public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public Dictionary<int, Player> Players = new Dictionary<int, Player>();


    public UnityEvent OnGameEnd = new UnityEvent();
    public UnityEvent OnInitiateCountdown = new UnityEvent();
    public UnityEvent OnGameStart = new UnityEvent();
    public UnityEvent OnPlayerKilled = new UnityEvent();


    [SerializeField]
    private int _countdownTime = 5;

    [SerializeField]
    private int _killsToWin = 3;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayerKilled(int playerID, int attackerID)
    {
        if (!base.IsServer) return;

        Players[attackerID].Kills++;
        Players[playerID].Deaths++;

        CheckGameEnd(attackerID);
    }

    private void CheckGameEnd(int playerID)
    {
        if (Players[playerID].Kills >= _killsToWin)
        {
            OnGameEnd.Invoke();
        }   
    }

    public void SetUsername(int playerID, string username)
    {
        if (!base.IsServer) return;

        Players[playerID].Username = username;
    }

    public class Player
    {
        public NetworkConnection Connection { get; set; }
        public string Username { get; set; } = "user123";
        public int Kills { get; set; } = 0;
        public int Deaths { get; set; } = 0;
        public bool IsReady { get; set; } = false;
    }
}
