using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyPlayerListManager : MonoBehaviour
{
    /// <summary>
    /// The parent component for the rows in the scoreboard.
    /// </summary>
    [SerializeField]
    private GameObject _playerList;

    /// <summary>
    /// Prefab for the player list item. This is a row in the scoreboard.
    /// </summary>
    [SerializeField]
    private GameObject _playerListElement;


    [SerializeField]
    private LobbyManager _lobbyManager;

    private void Start()
    {
        _lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();

        // Listen for changes from the LobbyManager.
        _lobbyManager.OnUpdatePlayerList.AddListener(UpdateScoreboard);

        UpdateScoreboard();
    }

    private void UpdateScoreboard()
    {
        Debug.Log("Updating scoreboard");

        // Set player list to the current list of players.
        var members = _lobbyManager.PlayerList;
        var players = members.Select(m => new Player()
        {
            Username = m.MemberData["Username"],
            Kills = 0,
            Deaths = 0
        }).ToList();

        // Clear the scoreboard besides the header row.
        for (var i = 1; i < _playerList.transform.childCount; i++)
        {
            Destroy(_playerList.transform.GetChild(i).gameObject);
        }

        // Create a new row for each player.
        foreach (var (player, index) in players.Select((p, i) => (p, i)))
        {
            var playerListItem = Instantiate(_playerListElement, _playerList.transform);
            playerListItem.GetComponent<LobbyPlayerListElement>().SetPlayer(player, index + 1);
        }
    }
}
