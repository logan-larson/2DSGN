using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.UI;
using FishNet.Transporting;

public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public Dictionary<int, Player> Players = new Dictionary<int, Player>();


    /// <summary>
    /// Called at the end of each game.
    /// </summary>
    public UnityEvent OnGameEnd = new UnityEvent();
    /// <summary>
    /// Called when all players are ready.
    /// </summary>
    public UnityEvent OnInitiateCountdown = new UnityEvent();
    /// <summary>
    /// Called at the beginning of each game.
    /// </summary>
    public UnityEvent OnGameStart = new UnityEvent();

    /// <summary>
    /// Called once at the beginning of the session. When the lobby is loaded.
    /// </summary>
    public UnityEvent OnLobbyStart = new UnityEvent();

    /// <summary>
    /// Invoked whenever a player is killed.
    /// </summary>
    public UnityEvent OnPlayerKilled = new UnityEvent();

    /// <summary>
    /// Invoked whenever the scoreboard is updated.
    /// </summary>
    public UnityEvent OnScoreboardUpdate = new UnityEvent();

    [SerializeField]
    private int _countdownTime = 5;

    [SerializeField]
    private int _killsToWin = 5;

    public GameState CurrentGameState { get; private set; } = GameState.Lobby;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        OnLobbyStart.Invoke();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

        // Check if this is the Playground scene, if so, start the game
        /*
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Playground")
            StartGame(); 
        */
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        OnScoreboardUpdate.Invoke();
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            if (Players.Count() == 0) return;

            var player = Players.Select(x => (x.Key, x.Value)).First(x => x.Value.Connection == conn);

            Players.Remove(player.Key);

            // If the player was the last one, end the game and go back to the lobby
            // Otherwise, update the scoreboard
            if (Players.Count() == 0)
            {
                CurrentGameState = GameState.Lobby;
                OnLobbyStart.Invoke();
            }
            else
            {
                OnScoreboardUpdate.Invoke();
            }
        }
    }

    public void PlayerJoined(int playerID, Player player)
    {
        if (!base.IsServer) return;

        Players.Add(playerID, player);

        OnScoreboardUpdate.Invoke();
    }

    public void PlayerKilled(int playerID, int attackerID)
    {
        if (!base.IsServer) return;

        if (playerID != attackerID)
            Players[attackerID].Kills++;

        Players[playerID].Deaths++;
            
        OnScoreboardUpdate.Invoke();

        CheckGameEnd(attackerID);
    }

    private void CheckGameEnd(int playerID)
    {
        if (Players[playerID].Kills >= _killsToWin)
        {
            Players.Select(x => x.Value).ToList().ForEach(x =>
            {
                x.IsReady = false;
                SetPlayerReadyTargetRpc(x.Connection, x, false);
            });

            OnScoreboardUpdate.Invoke();

            OnGameEnd.Invoke();

            CurrentGameState = GameState.Lobby;
        }   
    }

    public void SetUsername(int playerID, string username)
    {
        if (!base.IsServer) return;

        Players[playerID].Username = username;

        OnScoreboardUpdate.Invoke();
    }

    public void SetPlayerReady(NetworkConnection conn, bool isReady)
    {

        if (CurrentGameState != GameState.Lobby) return;

        var player = Players.Values.First(x => x.Connection == conn);

        player.IsReady = isReady;

        SetPlayerReadyTargetRpc(conn, player, isReady);

        OnScoreboardUpdate.Invoke();

        if (Players.Values.All(x => x.IsReady))
        {
            InitiateCountdown();
        }
    }

    private void InitiateCountdown()
    {
        CurrentGameState = GameState.Countdown;

        OnInitiateCountdown.Invoke();

        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        var countdownTime = _countdownTime;

        while (countdownTime > 0)
        {
            yield return new WaitForSeconds(1);

            countdownTime--;
        }

        StartGame();
    }

    private void StartGame()
    {
        CurrentGameState = GameState.Game;

        OnGameStart.Invoke();

        Players.Values.ToList().ForEach(x =>
        {
            x.Kills = 0;
            x.Deaths = 0;
        });

        OnScoreboardUpdate.Invoke();
    }

    [TargetRpc]
    private void SetPlayerReadyTargetRpc(NetworkConnection conn, Player player, bool isReady)
    {
        player.GameObject.GetComponent<LobbyManager>().SetReady(isReady);
    }

    public class Player
    {
        public NetworkConnection Connection { get; set; }
        public GameObject GameObject { get; set; }
        public string Username { get; set; }
        public int Kills { get; set; } = 0;
        public int Deaths { get; set; } = 0;
        public bool IsReady { get; set; } = false;

        public Player()
        {
            System.Random random = new System.Random();
            Username = "user" + random.Next(100, 1000);
        }
    }

    public enum GameState
    {
        Lobby,
        Countdown,
        Game
    }
}
