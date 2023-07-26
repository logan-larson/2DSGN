using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{

    public GameObject LobbyLeaderboard;

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

}
