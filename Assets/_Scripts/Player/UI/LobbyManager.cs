using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    private string _lobbyId;
    private string _connectionString;
    private Lobby _lobby;

    private LoginResult _loginResult;

    private void Start()
    {
        Initialize();
    }

    private async void Initialize()
    {
        _loginResult = await Login();

        if (_loginResult == null)
        {
            Debug.Log("Login failed, returning to main menu");
            SceneManager.LoadScene("Menu");
        }

        Debug.Log("Login success");

        if (UserInfo.IsHost)
        {
            // Create a new lobby
            CreateLobby();
        }
        else
        {
            // Join the lobby
            JoinLobby();
        }
    }

    #region Login

    private Task<LoginResult> Login()
    {
        var tcs = new TaskCompletionSource<LoginResult>();

        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            CustomId = SystemInfo.deviceUniqueIdentifier
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result => tcs.SetResult(result),
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
                tcs.SetResult((LoginResult)null);
            });

        return tcs.Task;
    }

    #endregion

    #region Create Lobby

    private void CreateLobby()
    {
        // TEMP: Hardcoded access policy
        AccessPolicy = AccessPolicy.Private;

        // Create the entity key
        var entity = new PlayFab.MultiplayerModels.EntityKey
        {
            Id = _loginResult.EntityToken.Entity.Id,
            Type = _loginResult.EntityToken.Entity.Type
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

        PlayFabMultiplayerAPI.CreateLobby(createLobbyRequest, OnCreateLobbySuccess, error =>
        {
            Debug.Log("Create lobby failed, returning to main menu");
            SceneManager.LoadScene("Menu");
        });
    }

    private void OnCreateLobbySuccess(CreateLobbyResult result)
    {
        Debug.Log("Lobby Created");

        _lobbyId = result.LobbyId;

        // String other players use to connect
        _connectionString = result.ConnectionString;
        Debug.Log(_connectionString);

        // Subscribe to lobby events

        // TEMP
        GetLobby();
    }

    #endregion

    #region Join Lobby

    private void JoinLobby()
    {
        var request = new JoinLobbyRequest()
        {
            ConnectionString = UserInfo.ConnectionString,
            MemberEntity = new PlayFab.MultiplayerModels.EntityKey
            {
                Id = _loginResult.EntityToken.Entity.Id,
                Type = _loginResult.EntityToken.Entity.Type
            }
        };

        PlayFabMultiplayerAPI.JoinLobby(request, OnJoinLobbySuccess, error =>
        {
            Debug.Log("Join lobby failed, returning to main menu");
            SceneManager.LoadScene("Menu");
        });
    }

    private void OnJoinLobbySuccess(JoinLobbyResult result)
    {
        _lobbyId = result.LobbyId;

        GetLobby();
    }

    #endregion

    #region Get Lobby

    private void GetLobby()
    {
        // TEMP: Get the lobby details to see if it was created and if the player is joined
        GetLobbyRequest request = new GetLobbyRequest
        {
            LobbyId = _lobbyId
        };

        PlayFabMultiplayerAPI.GetLobby(request, OnGetLobbySuccess, OnError);
    }

    private void OnGetLobbySuccess(GetLobbyResult result)
    {
        PlayerList = result.Lobby.Members;

        _lobby = result.Lobby;

        OnUpdatePlayerList.Invoke();
    }

    #endregion

    #region Leave Lobby

    public void LeaveLobby()
    {
        var request = new LeaveLobbyRequest
        {
            MemberEntity = new PlayFab.MultiplayerModels.EntityKey
            {
                Id = _loginResult.EntityToken.Entity.Id,
                Type = _loginResult.EntityToken.Entity.Type
            },
            LobbyId = _lobby.LobbyId
        };

        PlayFabMultiplayerAPI.LeaveLobby(request, OnLeaveLobbySuccess, OnError);
    }

    private void OnLeaveLobbySuccess(LobbyEmptyResult result)
    {
        Debug.Log("Left Lobby");

        SceneManager.LoadScene("Menu");
    }

    #endregion

    private void OnError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

}

