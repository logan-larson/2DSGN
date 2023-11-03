using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System.Collections.Generic;

/**
<summary>
JoinGame is responsible for passing the username to the Game scene and loading the game scene.
TODO: This should be refactored into two separate scripts. One for passing the username and one for loading the game scene.
</summary>
*/
public class JoinGame : MonoBehaviour
{
    public UserInfo UserInfo;
    public bool IsServerBuild;

    [SerializeField]
    private TMP_InputField _usernameInput;

    private void Start()
    {
        if (IsServerBuild)
        {
            SceneManager.LoadScene("BigMap");
        }
    }

    public void OnJoinGame()
    {
        UserInfo.Username = _usernameInput.text;

        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            CustomId = SystemInfo.deviceUniqueIdentifier
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login Success");

        RequestMultiplayerServer();

        SceneManager.LoadScene("BigMap");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    private void RequestMultiplayerServer()
    {
        Debug.Log("Requesting Multiplayer Server");

        RequestMultiplayerServerRequest request = new RequestMultiplayerServerRequest()
        {
            BuildId = "b7d314f0-6cd7-4530-a237-e44cd693b17f",
            SessionId = System.Guid.NewGuid().ToString(),
            PreferredRegions = new List<string> { "EastUS" },
        };

        PlayFabMultiplayerAPI.RequestMultiplayerServer(request, OnRequestMultiplayerServerSuccess, OnRequestMultiplayerServerFailure);
    }

    private void OnRequestMultiplayerServerSuccess(RequestMultiplayerServerResponse response)
    {
        if (response == null) return;

        Debug.Log("**** These are the connection details **** -- IP: " + response.IPV4Address + " Port: " + (ushort)response.Ports[0].Num);

        UserInfo.IP = response.IPV4Address;
        UserInfo.Port = (ushort) response.Ports[0].Num;
    }

    private void OnRequestMultiplayerServerFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

}
