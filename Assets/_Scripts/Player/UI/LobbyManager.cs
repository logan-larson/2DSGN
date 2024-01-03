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
using Microsoft.AspNetCore.SignalR.Client;
using System;

/// <summary>
/// Currently this is the Ready Up manager
/// </summary>
public class LobbyManager : MonoBehaviour
{
    public struct HubSession
    {
        public string ConnectionHandle;
        public string[] Topics;
        public string Status;
        public string TraceID;
    }

    private HubSession _hubSession;

    public UserInfo UserInfo;
    public LobbyInfo LobbyInfo;

    public AccessPolicy AccessPolicy = AccessPolicy.Public;

    [SerializeField]
    private GameObject _startGameButton;

    /// <summary>
    /// Event for when the player's list is updated.
    /// </summary>
    public UnityEvent OnUpdatePlayerList = new UnityEvent();

    public List<Member> PlayerList = new List<Member>();

    private string _lobbyId;
    private string _connectionString;
    private Lobby _lobby;

    private LoginResult _loginResult;

    private HubConnection _hubConnection;
    private string _connectionHandle;

    private void Start()
    {
        //Initialize();
        GetLobbyWithLobbyInfo();
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
            _startGameButton.SetActive(true);

            // Create a new lobby
            CreateLobby();
        }
        else
        {
            _startGameButton.SetActive(false);

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
            CustomId = SystemInfo.deviceUniqueIdentifier + UserInfo.Username
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
        // TODO: In host menu, allow the host to select the access policy from the main menu
        AccessPolicy = AccessPolicy.Private;

        // Create the entity key
        var entity = new PlayFab.MultiplayerModels.EntityKey
        {
            Id = _loginResult.EntityToken.Entity.Id,
            Type = _loginResult.EntityToken.Entity.Type
        };

        // Set the members list
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

        // Send a request to create the lobby
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

        // Connect to the hub
        ConnectToHub();

        // String other players use to connect
        _connectionString = result.ConnectionString;
        Debug.Log($"ConnectionString: {_connectionString}");

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
            },
            MemberData = new Dictionary<string, string>
            {
                { "Username", UserInfo.Username },
            }
        };

        PlayFabMultiplayerAPI.JoinLobby(request, OnJoinLobbySuccess, error =>
        {
            Debug.Log("Join lobby failed, returning to main menu");
            Debug.Log($"{error.Error}: {error.ErrorMessage}");
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

    private void GetLobbyWithLobbyInfo()
    {
        GetLobbyRequest request = new GetLobbyRequest
        {
            LobbyId = LobbyInfo.LobbyID
        };

        PlayFabMultiplayerAPI.GetLobby(request, OnGetLobbySuccess, OnError);
    }

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
        // Set the player list and lobby details
        PlayerList = result.Lobby.Members;

        foreach (var member in PlayerList)
        {
            if (member.MemberData.ContainsKey("Username"))
            {
                Debug.Log(member.MemberData["Username"]);
            }
        }

        _lobby = result.Lobby;

        OnUpdatePlayerList.Invoke();

        // Connect to the lobby events
        // ConnectToLobbyEvents();
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

        if (_hubConnection != null)
        {
            _hubConnection.StopAsync();
        }

        PlayFabMultiplayerAPI.LeaveLobby(request, OnLeaveLobbySuccess, OnError);
    }

    private void OnLeaveLobbySuccess(LobbyEmptyResult result)
    {
        Debug.Log("Left Lobby");

        SceneManager.LoadScene("Menu");
    }

    #endregion

    #region Hub Connection

    private async void ConnectToHub()
    {
        try
        {
            var entityToken = _loginResult.EntityToken.EntityToken.ToString();

            Debug.Log($"EntityToken: {entityToken}");

            // Create the hub connection
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://5B649.playfabapi.com/PubSub", options => { options.Headers.Add("X-EntityToken", entityToken); })
                .WithAutomaticReconnect()
                .Build();

            await _hubConnection.StartAsync();

            var request = new StartOrRecoverSessionRequest(TraceContextGenerator.GenerateTraceParent());

            var response = await _hubConnection.InvokeAsync<StartOrRecoverSessionResponse>("StartOrRecoverSession", request);

            _hubSession = new HubSession
            {
                ConnectionHandle = response.NewConnectionHandle,
                Topics = response.RecoveredTopics,
                Status = response.Status,
                TraceID = response.TraceId
            };

            var subscribeRequest = new SubscribeToLobbyResourceRequest
            {
                PubSubConnectionHandle = _hubSession.ConnectionHandle,
                EntityKey = new PlayFab.MultiplayerModels.EntityKey
                {
                    Id = _loginResult.EntityToken.Entity.Id,
                    Type = _loginResult.EntityToken.Entity.Type
                },
                


            };

            //PlayFabMultiplayerAPI.SubscribeToLobbyResource()
        }
        catch (Exception ex)
        {
            Debug.Log($"Error in ConnectToHub: {ex.Message}");
        }
    }

    public class TraceContextGenerator
    {
        private static System.Random random = new System.Random();

        public static string GenerateTraceParent()
        {
            string version = "00";
            string traceId = GenerateRandomHexString(32);
            string parentId = GenerateRandomHexString(16);
            string traceFlags = "01"; // Recording is requested.

            return $"{version}-{traceId}-{parentId}-{traceFlags}";
        }

        private static string GenerateRandomHexString(int length)
        {
            byte[] buffer = new byte[length / 2];
            random.NextBytes(buffer);
            return BitConverter.ToString(buffer).Replace("-", "").ToLower();
        }
    }

    #endregion

    private void OnError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public class StartOrRecoverSessionRequest
    {
        public string TraceParent { get; set; }

        public StartOrRecoverSessionRequest(string traceParent)
        {
            this.TraceParent = traceParent;
        }
    }

    public class StartOrRecoverSessionResponse
    {
        public string NewConnectionHandle { get; set; }

        public string[] RecoveredTopics { get; set; }

        public string Status { get; set; }

        public string TraceId { get; set; }

        public StartOrRecoverSessionResponse(string newConnectionHandle, string[] recoveredTopics, string status, string traceId)
        {
            this.NewConnectionHandle = newConnectionHandle;
            this.RecoveredTopics = recoveredTopics;
            this.Status = status;
            this.TraceId = traceId;
        }
    }
}


