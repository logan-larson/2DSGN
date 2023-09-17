using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InGameLeaderboard : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text _first;
    [SerializeField]
    private TMP_Text _second;
    [SerializeField]
    private TMP_Text _third;
    [SerializeField]
    private TMP_Text _you;

    public int PlayerID;

    //private void Start()
    public override void OnStartServer()
    {
        base.OnStartServer();
        GameStateManager.Instance.OnPlayerKilled.AddListener(OnPlayerKilled);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Set the text
        _third.text = "";
        _second.text = "";
        _first.text = "";
        _you.text = "";
    }

    private void OnPlayerKilled()
    {
        // Order the players by kills
        GameStateManager.Player[] players = GameStateManager.Instance.Players.Values.OrderByDescending(p => p.Kills).ToArray();

        OnPlayerKilledObserversRpc(players);
    }

    [ObserversRpc]
    private void OnPlayerKilledObserversRpc(
        GameStateManager.Player[] players
    )
    {
        // Set the text
        if (players.Length >= 3)
            _third.text = $"3. {players[2].Username} - {players[2].Kills}";
        else
            _third.text = "";

        if (players.Length >= 2)
            _second.text = $"2. {players[1].Username} - {players[1].Kills}";
        else
            _second.text = "";

        if (players.Length >= 1)
            _first.text = $"1. {players[0].Username} - {players[0].Kills}";


        var you = players.FirstOrDefault(x => x.Connection == base.LocalConnection);

        _you.text = $"You. {you.Username} - {you.Kills}";
    }
}
