using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.UI;

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


    public UnityEvent OnPlayerKilled = new UnityEvent();

    public UnityEvent OnLeaderboardActive = new UnityEvent();

    [SerializeField]
    private GameObject _lobbyLeaderboard;

    [SerializeField]
    private GameObject _playersList;

    [SerializeField]
    private GameObject _playersListItem;

    [SerializeField]
    private int _countdownTime = 3;

    [SerializeField]
    private int _killsToWin = 3;

    public GameState CurrentGameState { get; private set; } = GameState.Lobby;

    public Button ReadyButton;

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

        _lobbyLeaderboard.SetActive(false);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        OnLeaderboardActive.Invoke();
        _lobbyLeaderboard.SetActive(true);
    }


    public void PlayerJoined(int playerID, Player player)
    {
        if (!base.IsServer) return;

        Players.Add(playerID, player);

        SetLeaderboardStateObserversRpc(Players.Values.OrderByDescending(x => x.Kills).ToArray());
    }

    public void PlayerKilled(int playerID, int attackerID)
    {
        if (!base.IsServer) return;

        Players[attackerID].Kills++;
        Players[playerID].Deaths++;
            
        SetLeaderboardStateObserversRpc(Players.Values.OrderByDescending(x => x.Kills).ToArray());

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

            SetLeaderboardStateObserversRpc(Players.Values.OrderByDescending(x => x.Kills).ToArray());

            OnGameEnd.Invoke();
            EnableLeaderboardObserversRpc();
            CurrentGameState = GameState.Lobby;
        }   
    }

    [ObserversRpc]
    private void EnableLeaderboardObserversRpc()
    {
        _lobbyLeaderboard.SetActive(true);
    }

    public void SetUsername(int playerID, string username)
    {
        if (!base.IsServer) return;

        Players[playerID].Username = username;

        SetLeaderboardStateObserversRpc(Players.Values.OrderByDescending(x => x.Kills).ToArray());
    }

    public void SetPlayerReady(NetworkConnection conn, bool isReady)
    {

        if (CurrentGameState != GameState.Lobby) return;

        var player = Players.Values.First(x => x.Connection == conn);

        player.IsReady = isReady;

        SetPlayerReadyTargetRpc(conn, player, isReady);

        SetLeaderboardStateObserversRpc(Players.Values.OrderByDescending(x => x.Kills).ToArray());

        if (Players.Values.All(x => x.IsReady))
        {
            StartGame();
        }
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

            SetLeaderboardStateObserversRpc(Players.Values.OrderByDescending(x => x.Kills).ToArray());
            // TODO: Start countdown
            // For now just start game
            /*
            CurrentGameState = GameState.Countdown;
            OnInitiateCountdown.Invoke();
            StartCoroutine(CountdownCoroutine());
            */
    }

    [TargetRpc]
    private void SetPlayerReadyTargetRpc(NetworkConnection conn, Player player, bool isReady)
    {
        player.GameObject.GetComponent<LobbyManager>().SetReady(isReady);
    }

    [ObserversRpc]
    private void SetLeaderboardStateObserversRpc(Player[] playersList)
    {
        for (var i = 1; i < _playersList.transform.childCount; i++)
        {
            Object.Destroy(_playersList.transform.GetChild(i).gameObject);
        }

        for (var i = 0; i < playersList.Length; i++)
        {
            var playerListItem = Instantiate(_playersListItem, _playersList.transform);
            playerListItem.GetComponent<PlayerListItem>().SetPlayer(playersList[i], i + 1);
        }
    }

    private IEnumerator CountdownCoroutine()
    {
        if (_countdownTime > 0)
        {
            _countdownTime--;

            yield return new WaitForSeconds(1f);
        }

        OnGameStart.Invoke();
        CurrentGameState = GameState.Game;
        _countdownTime = 3;
        yield return null;
    }

    public class Player
    {
        public NetworkConnection Connection { get; set; }
        public GameObject GameObject { get; set; }
        public string Username { get; set; } = "user123";
        public int Kills { get; set; } = 0;
        public int Deaths { get; set; } = 0;
        public bool IsReady { get; set; } = false;
    }

    public enum GameState
    {
        Lobby,
        Countdown,
        Game
    }
}
