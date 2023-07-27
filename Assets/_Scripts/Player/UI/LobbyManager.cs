using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{

    public GameObject LobbyLeaderboard;

    private bool _isReady = false;

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameStateManager.Instance.OnLeaderboardActive.AddListener(() =>
        {
            var PlayerUI = GameObject.Find("PlayerUI");
            var leaderboard = PlayerUI.transform.GetChild(2).gameObject;

            LobbyLeaderboard = leaderboard;
        });
    }

    public void ToggleLeaderboard()
    {
        if (LobbyLeaderboard != null)
            LobbyLeaderboard.SetActive(!LobbyLeaderboard.activeSelf);
    }

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
