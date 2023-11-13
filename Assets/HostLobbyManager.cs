using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        // TEMP: Hardcoded access policy
        AccessPolicy = AccessPolicy.Private;

        var entity = new PlayFab.MultiplayerModels.EntityKey
        {
            Id = result.EntityToken.Entity.Id,
            Type = result.EntityToken.Entity.Type
        };

        List<Member> members = new List<Member>()
        {
            new Member
            {
                MemberEntity = entity,
                MemberData = new Dictionary<string, string>
                {
                    { "Team", "Blue" }
                }

            } 
        };

        var createLobbyRequest = new CreateLobbyRequest
        {
            MaxPlayers = 9,
            Owner = entity,
            UseConnections = true,
            Members = members,
            AccessPolicy = AccessPolicy,
            OwnerMigrationPolicy = OwnerMigrationPolicy.Automatic 
        };

        PlayFabMultiplayerAPI.CreateLobby(createLobbyRequest, OnCreateLobbySuccess, OnCreateLobbyFailure);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    private void OnCreateLobbySuccess(CreateLobbyResult result)
    {
        Debug.Log("Lobby Created");
    }

    private void OnCreateLobbyFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
}
