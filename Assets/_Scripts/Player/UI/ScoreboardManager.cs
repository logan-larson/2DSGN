using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreboardManager : NetworkBehaviour
{
    /// <summary>
    /// The UI canvas for the scoreboard. This is the parent component.
    /// </summary>
    public GameObject ScoreboardUI;

    /// <summary>
    /// The parent component for the rows in the scoreboard.
    /// </summary>
    [SerializeField]
    private GameObject _playersList;

    /// <summary>
    /// Prefab for the player list item. This is a row in the scoreboard.
    /// </summary>
    [SerializeField]
    private GameObject _playersListItem;

    public override void OnStartClient()
    {
        base.OnStartClient();

        ScoreboardUI.SetActive(false);

        UpdateScoreboard();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (GameStateManager.Instance == null) return;

        // Add listeners for the scoreboard.
        GameStateManager.Instance.OnPlayersChanged.AddListener(UpdateScoreboardObserversRpc);

        GameStateManager.Instance.OnInitiateCountdown.AddListener(DisableScoreboardObserversRpc);
        GameStateManager.Instance.OnGameStart.AddListener(DisableScoreboardObserversRpc);

        GameStateManager.Instance.OnGameEnd.AddListener(EnableScoreboardObserversRpc);
    }

    public void ToggleScoreboard()
    {
        ScoreboardUI.SetActive(ScoreboardUI.activeSelf ? false : true);
    }

    [ObserversRpc]
    private void EnableScoreboardObserversRpc()
    {
        ScoreboardUI.SetActive(true);
        UpdateScoreboard();
    }

    [ObserversRpc]
    private void DisableScoreboardObserversRpc()
    {
        ScoreboardUI.SetActive(false);
        UpdateScoreboard();
    }

    [ObserversRpc]
    private void UpdateScoreboardObserversRpc()
    {
        UpdateScoreboard();
    }

    private void UpdateScoreboard()
    {
        if (GameStateManager.Instance == null) return;

        // Sort the players by kills.
        Dictionary<int, GameStateManager.Player> orderedPlayers = GameStateManager.Instance.Players.OrderByDescending(x => x.Value.Kills).ToDictionary(x => x.Key, x => x.Value);

        // Clear the scoreboard besides the header row.
        for (var i = 1; i < _playersList.transform.childCount; i++)
        {
            Destroy(_playersList.transform.GetChild(i).gameObject);
        }

        // Create a new row for each player.
        foreach (var (player, index) in orderedPlayers.Values.Select((p, i) => (p, i)))
        {
            var playerListItem = Instantiate(_playersListItem, _playersList.transform);
            playerListItem.GetComponent<PlayerListItem>().SetPlayer(player, index + 1);
        }
    }
}
