using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Multiplayer;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinLobbyManager : MonoBehaviour
{
    public UserInfo UserInfo;
    public LobbyInfo LobbyInfo;

    public AccessPolicy AccessPolicy;

    [SerializeField]
    private TMP_InputField _usernameInput;

    [SerializeField]
    private TMP_InputField _connectionStringInput;

    [SerializeField]
    private GameObject _usernameErrorIndicator;

    [SerializeField]
    private GameObject _lobbyList;

    [SerializeField]
    private GameObject _lobbyItemPrefab;

    private void Start()
    {
        // Get a list of the lobbies that are available to join
        PlayFabMultiplayer.OnLobbyFindLobbiesCompleted += OnLobbyFindLobbiesCompleted;
        Debug.Log("Finding lobbies...");
        PlayFabMultiplayer.FindLobbies(new PFEntityKey(UserInfo.EntityKey.Id, UserInfo.EntityKey.Type), new LobbySearchConfiguration());
    }

    private void OnLobbyFindLobbiesCompleted(IList<LobbySearchResult> searchResults, PFEntityKey newMember, int reason)
    {
        for (var i = 1; i < _lobbyList.transform.childCount; i++)
        {
            Destroy(_lobbyList.transform.GetChild(i).gameObject);
        }

        if (LobbyError.SUCCEEDED(reason))
        {

            Debug.Log("Found " + searchResults.Count + " lobbies");
            foreach (LobbySearchResult result in searchResults)
            {
                Debug.Log("Lobby Id: " + result.LobbyId + " - connection string: " + result.ConnectionString);
                Debug.Log("Current Member Count: " + result.CurrentMemberCount);

                GameObject lobbyItem = Instantiate(_lobbyItemPrefab, _lobbyList.transform);
                lobbyItem.GetComponent<LobbyItem>().Initialize(result);
            }
        }
        else
        {
            Debug.Log("Error finding lobbies");
        }
    }

    public void OnJoinLobby(string connectionString)
    {
        PlayFabMultiplayer.OnLobbyJoinCompleted += OnLobbyJoinCompleted;

        _usernameErrorIndicator.SetActive(false);

        if (string.IsNullOrEmpty(_usernameInput.text))
        {
            _usernameErrorIndicator.SetActive(true);
            return;
        }

        UserInfo.Username = _usernameInput.text;
        UserInfo.IsHost = false;

        LobbyInfo.ConnectionString = connectionString;


        // Send request to join lobby on PlayFab
        PlayFabMultiplayer.JoinLobby(
            new PFEntityKey(UserInfo.EntityKey.Id, UserInfo.EntityKey.Type),
            LobbyInfo.ConnectionString,
            new Dictionary<string, string>() { { "Username", UserInfo.Username } }
        );

        // Change to the Lobby scene
        SceneManager.LoadScene("Lobby");
    }

    private void OnLobbyJoinCompleted(PlayFab.Multiplayer.Lobby lobby, PFEntityKey newMember, int result)
    {
        if (LobbyError.SUCCEEDED(result))
        {
            Debug.Log("Lobby joined successfully");

            LobbyInfo.LobbyID = lobby.Id;
            LobbyInfo.ConnectionString = lobby.ConnectionString;

            // Change to the Lobby scene
            SceneManager.LoadScene("Lobby");
        }
        else
        {
            Debug.Log("Lobby join failed");
        }
    }
}
