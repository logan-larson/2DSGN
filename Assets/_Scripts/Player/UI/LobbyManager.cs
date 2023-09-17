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

    public void ToggleReady()
    {
        if (!base.IsOwner) return;

        ToggleReadyServerRpc(base.LocalConnection);
    }

    [ServerRpc]
    private void ToggleReadyServerRpc(NetworkConnection conn)
    {
        GameStateManager.Instance.TogglePlayerReady(conn);
    }
}
