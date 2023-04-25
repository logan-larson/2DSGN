using UnityEngine;
using FishNet.Managing;

/**
<summary>
NetworkHandler is responsible for starting the server or client connection.
</summary>
*/
public class NetworkHandler : MonoBehaviour
{
    public UserInfo UserInfo;
    public NetworkInfo NetworkInfo;

    private NetworkManager _networkManager;

    private void Start()
    {
        _networkManager = FindObjectOfType<NetworkManager>();

        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        if (NetworkInfo.IsServerBuild)
        {
            _networkManager.ServerManager.StartConnection();
        }
        else
        {
            _networkManager.ClientManager.StartConnection();
        }
    }
}
