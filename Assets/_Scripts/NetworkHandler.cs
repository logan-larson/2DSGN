using UnityEngine;
using FishNet.Managing;

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
        _networkManager ??= FindObjectOfType<NetworkManager>();

        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        if (IsServerBuild)
        {
            _networkManager.ServerManager.StartConnection();
        }
        else
        {
            _networkManager.ClientManager.StartConnection();
        }
    }
}
