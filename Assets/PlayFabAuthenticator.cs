using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Multiplayer;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// When the player first loads the game, they will be automatically logged in to PlayFab.
/// If the login fails, the Host and Join buttons will be disabled, and a button to retry 
/// the login will be displayed.
/// </summary>
public class PlayFabAuthenticator : MonoBehaviour
{
    public UserInfo UserInfo;

    [SerializeField]
    private Button _hostButton;
    [SerializeField]
    private Button _joinButton;
    [SerializeField]
    private GameObject _loginFailedPanel;

    private void Start()
    {
        // Disable the Host and Join buttons until the player is logged in.
        _hostButton.interactable = false;
        _joinButton.interactable = false;

        // Hide the login failed panel.
        _loginFailedPanel.SetActive(false);

        // Attempt to log in to PlayFab.
        InitializePlayFab();
    }

    private async void InitializePlayFab()
    {
        // Attempt to log in to PlayFab.
        LoginResult loginResult = await Login();

        if (loginResult == null)
        {
            Debug.Log("Login failed, returning to main menu");

            // Display the login failed panel.  
            // This panel contains a button that will retry the login.
            _loginFailedPanel.SetActive(true);

            return;
        }

        // Enable the Host and Join buttons.
        _hostButton.interactable = true;
        _joinButton.interactable = true;

        // Set the player's entity key.
        UserInfo.EntityKey = loginResult.EntityToken.Entity;
        UserInfo.EntityToken = loginResult.EntityToken.EntityToken;

        Debug.Log("Login success");
    }
    
    private Task<LoginResult> Login()
    {
        var tcs = new TaskCompletionSource<LoginResult>();

        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            CustomId = SystemInfo.deviceUniqueIdentifier + UserInfo.Username
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result =>
            {
                PlayFabMultiplayer.SetEntityToken(new PFEntityKey(
                    result.EntityToken.Entity.Id,
                    result.EntityToken.Entity.Type), result.EntityToken.EntityToken
                    );
                tcs.SetResult(result);
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
                tcs.SetResult((LoginResult)null);
            });

        return tcs.Task;
    }

    public void RetryLogin()
    {
        // Hide the login failed panel.
        _loginFailedPanel.SetActive(false);

        // Attempt to log in to PlayFab.
        InitializePlayFab();
    }

    public void HideLoginFailed()
    {
        // Hide the login failed panel.
        _loginFailedPanel.SetActive(false);
    }
}
