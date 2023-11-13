using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public struct EntityKey
{
    public string Id;
    public string Type;
}

/// <summary>
/// Currently this is the Ready Up manager
/// </summary>
public class LobbyManager : MonoBehaviour
{
    public UserInfo UserInfo;

    public AccessPolicy AccessPolicy = AccessPolicy.Public;

    /// <summary>
    /// Event for when the player's list is updated.
    /// </summary>
    public UnityEvent OnUpdatePlayerList = new UnityEvent();

    public List<Member> PlayerList = new List<Member>();

    private EntityKey _entityKey;

    private void Start()
    {
        if (UserInfo.IsHost)
        {
            // Create a new lobby
            CreateLobby();
        }
        else
        {

        }
    }

    private void CreateLobby()
    {
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

        // Create the entity key
        _entityKey = new EntityKey
        {
            Id = result.EntityToken.Entity.Id,
            Type = result.EntityToken.Entity.Type
        };

        var entity = new PlayFab.MultiplayerModels.EntityKey
        {
            Id = _entityKey.Id,
            Type = _entityKey.Type
        };

        List<Member> members = new List<Member>()
        {
            new Member
            {
                MemberEntity = entity,
                MemberData = new Dictionary<string, string>
                {
                    { "Username", UserInfo.Username },
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

        // TEMP: Get the lobby details to see if it was created and if the player is joined
        GetLobbyRequest request = new GetLobbyRequest
        {
            LobbyId = result.LobbyId
        };

        PlayFabMultiplayerAPI.GetLobby(request, OnGetLobbySuccess, OnGetLobbyFailure);
    }

    private void OnCreateLobbyFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }


    private void OnGetLobbySuccess(GetLobbyResult result)
    {
        PlayerList = result.Lobby.Members;

        OnUpdatePlayerList.Invoke();
    }

    private void OnGetLobbyFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
}

