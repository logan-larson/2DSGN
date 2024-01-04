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
    private GameObject _connectionStringErrorIndicator;

    public void OnJoinLobby()
    {
        PlayFabMultiplayer.OnLobbyJoinCompleted += OnLobbyJoinCompleted;

        _usernameErrorIndicator.SetActive(false);
        _connectionStringErrorIndicator.SetActive(false);

        if (string.IsNullOrEmpty(_usernameInput.text))
        {
            _usernameErrorIndicator.SetActive(true);
            return;
        }

        if (string.IsNullOrEmpty(_connectionStringInput.text))
        {
            _connectionStringErrorIndicator.SetActive(true);
            return;
        }

        UserInfo.Username = _usernameInput.text;
        UserInfo.IsHost = false;

        LobbyInfo.ConnectionString = _connectionStringInput.text;


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
