using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinLobbyManager : MonoBehaviour
{
    public UserInfo UserInfo;

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
        UserInfo.ConnectionString = _connectionStringInput.text;

        // Change to the Lobby scene
        SceneManager.LoadScene("Lobby");
    }

}
