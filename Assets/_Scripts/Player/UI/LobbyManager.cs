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

    private NetworkConnection _connection;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _connection = base.LocalConnection;

        SetConnectionServerRpc(_connection);

        GameStateManager.Instance.OnLeaderboardActive.AddListener(() =>
        {
            var PlayerUI = GameObject.Find("PlayerUI");
            var leaderboard = PlayerUI.transform.GetChild(2).gameObject;

            LobbyLeaderboard = leaderboard;
        });
    }

    [ServerRpc]
    private void SetConnectionServerRpc(NetworkConnection conn)
    {
        _connection = conn;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameStateManager.Instance.OnInitiateCountdown.AddListener(() =>
        {
            SetLeaderboard(_connection, false);
        });

        GameStateManager.Instance.OnGameEnd.AddListener(() =>
        {
            SetLeaderboard(_connection, true);
        });
    }

    [TargetRpc]
    private void SetLeaderboard(NetworkConnection conn, bool isActive)
    {
        LobbyLeaderboard.SetActive(isActive);
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
