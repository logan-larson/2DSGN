using FishNet.Object;
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

    public Dictionary<int, GameStateManager.Player> Players = new Dictionary<int, GameStateManager.Player>();

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameStateManager.Instance.OnScoreboardUpdate.AddListener(UpdateScoreboard);

        GameStateManager.Instance.OnInitiateCountdown.AddListener(() =>
        {
            ScoreboardUI.SetActive(false);
        });

        GameStateManager.Instance.OnGameStart.AddListener(() =>
        {
            ScoreboardUI.SetActive(false);
        });

        GameStateManager.Instance.OnGameEnd.AddListener(() =>
        {
            ScoreboardUI.SetActive(true);
        });

        ScoreboardUI.SetActive(false);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public void ToggleScoreboard()
    {
        ScoreboardUI.SetActive(ScoreboardUI.activeSelf ? false : true);
    }

    private void UpdateScoreboard()
    {
        // Update the players dictionary.
        Players = GameStateManager.Instance.Players;

        // Sort the players by kills.
        Players = Players.OrderByDescending(x => x.Value.Kills).ToDictionary(x => x.Key, x => x.Value);

        // Clear the scoreboard besides the header row.
        for (var i = 1; i < _playersList.transform.childCount; i++)
        {
            Destroy(_playersList.transform.GetChild(i).gameObject);
        }

        // Create a new row for each player.
        foreach (var (player, index) in Players.Values.Select((p, i) => (p, i)))
        {
            var playerListItem = Instantiate(_playersListItem, _playersList.transform);
            playerListItem.GetComponent<PlayerListItem>().SetPlayer(player, index + 1);
        }
    }
}
