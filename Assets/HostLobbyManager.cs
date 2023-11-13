using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostLobbyManager : MonoBehaviour
{
    public UserInfo UserInfo;

    public AccessPolicy AccessPolicy;

    [SerializeField]
    private TMP_InputField _usernameInput;

    [SerializeField]
    private GameObject _errorIndicator;

    public void OnCreateLobby()
    {
        _errorIndicator.SetActive(false);

        if (string.IsNullOrEmpty(_usernameInput.text))
        {
            _errorIndicator.SetActive(true);
            return;
        }

        UserInfo.Username = _usernameInput.text;
        UserInfo.IsHost = true;

        // Change to the Lobby scene
        SceneManager.LoadScene("Lobby");
    }

}
