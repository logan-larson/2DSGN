using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Currently this is the Ready Up manager
/// </summary>
public class LobbyManager : NetworkBehaviour
{

    public GameObject LobbyLeaderboard;

    private bool _isReady = false;

    private void Start() { }

    public void ToggleReady()
    {
        ToggleReadyServerRpc(base.LocalConnection);
    }

    [ServerRpc]
    private void ToggleReadyServerRpc(NetworkConnection conn)
    {
        GameStateManager.Instance.SetPlayerReady(conn, !_isReady);
    }

    public void SetReady(bool isReady)
    {
        _isReady = isReady;
    }
}
