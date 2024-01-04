using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Multiplayer;
//using PlayFab.MultiplayerModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostLobbyManager : MonoBehaviour
{
    public UserInfo UserInfo;
    public LobbyInfo LobbyInfo;

    public LobbyAccessPolicy AccessPolicy = LobbyAccessPolicy.Private;

    [SerializeField]
    private TMP_InputField _usernameInput;

    [SerializeField]
    private GameObject _errorIndicator;

    public void OnCreateLobby()
    {
        PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted += OnLobbyCreateAndJoinCompleted;

        _errorIndicator.SetActive(false);

        if (string.IsNullOrEmpty(_usernameInput.text))
        {
            _errorIndicator.SetActive(true);
            return;
        }

        // Set this user as the host
        UserInfo.Username = _usernameInput.text;
        UserInfo.IsHost = true;


        // Send request to create lobby on PlayFab

        var lobbyConfig = new LobbyCreateConfiguration()
        {
            MaxMemberCount = 9,
            OwnerMigrationPolicy = LobbyOwnerMigrationPolicy.Automatic,
            AccessPolicy = AccessPolicy,
        };

        lobbyConfig.LobbyProperties["map"] = "BigMap";

        var joinConfig = new LobbyJoinConfiguration();
        joinConfig.MemberProperties["Username"] = UserInfo.Username;

        PlayFabMultiplayer.CreateAndJoinLobby(
            new PFEntityKey(UserInfo.EntityKey.Id, UserInfo.EntityKey.Type),
            lobbyConfig,
            joinConfig
        );
       
        /*

        // Create the entity key
        var entity = new PlayFab.MultiplayerModels.EntityKey
        {
            Id = UserInfo.EntityKey.Id,
            Type = UserInfo.EntityKey.Type
        };

        // Set the members list to contain the host
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

            // TODO: Show error toast
        });

        */
    }

    private void OnLobbyCreateAndJoinCompleted(Lobby lobby, int result)
    {
        if (LobbyError.SUCCEEDED(result))
        {
            Debug.Log("Lobby created successfully");

            // Store the lobby ID and connection string in the LobbyInfo
            LobbyInfo.LobbyID = lobby.Id;
            LobbyInfo.ConnectionString = lobby.ConnectionString;

            // Change to the Lobby scene
            SceneManager.LoadScene("Lobby");
        }
        else
        {
            Debug.Log("Lobby creation failed");
        }
    }

    /*
    private void OnCreateLobbySuccess(CreateLobbyResult result)
    {
        Debug.Log("Lobby created successfully");

        // Store the lobby ID and connection string in the LobbyInfo
        LobbyInfo.LobbyID = result.LobbyId;
        LobbyInfo.ConnectionString = result.ConnectionString;

        // Change to the Lobby scene
        SceneManager.LoadScene("Lobby");
    }
    */
}
