using PlayFab.Multiplayer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField]
    private JoinLobbyManager _joinLobbyManager;

    [SerializeField]
    private TMP_Text _lobbyName;

    [SerializeField]
    private TMP_Text _hostUsername;

    [SerializeField]
    private TMP_Text _currentMemberCount;

    private string _connectionString;

    private void Start()
    {
        _joinLobbyManager = FindObjectOfType<JoinLobbyManager>();
    }

    public void Initialize(LobbySearchResult lobby)
    {
        _connectionString = lobby.ConnectionString;

        _lobbyName.text = lobby.LobbyId;

        _hostUsername.text = lobby.OwnerEntity.Id;

        _currentMemberCount.text = lobby.CurrentMemberCount + "/" + lobby.MaxMemberCount;
    }

    public void OnJoinLobby()
    {
        _joinLobbyManager.OnJoinLobby(_connectionString);
    }
}
