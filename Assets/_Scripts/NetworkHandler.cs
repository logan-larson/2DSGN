using UnityEngine;
using FishNet.Managing;
using PlayFab;
using System.Collections;

/**
<summary>
NetworkHandler is responsible for starting the server or client connection.
</summary>
*/
public class NetworkHandler : MonoBehaviour
{
    public bool IsServerBuild;

    [SerializeField]
    private NetworkManager _networkManager;

    private void Start()
    {
        // Start connection to PlayFab Multiplayer Agent

        _networkManager ??= FindObjectOfType<NetworkManager>();

        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }


        if (IsServerBuild)
        {
            PlayFabMultiplayerAgentAPI.Start();
            _networkManager.ServerManager.StartConnection();

            // Register the PlayFab Multiplayer Agent's callback for when a connection is established
            StartCoroutine(ReadyForPlayers());
        }
        else
        {
            _networkManager.ClientManager.StartConnection();
        }
    }

    private IEnumerator ReadyForPlayers()
    {
        yield return new WaitForSeconds(.5f);
        PlayFabMultiplayerAgentAPI.ReadyForPlayers();
    }
}
