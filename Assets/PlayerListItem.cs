using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _place;
    [SerializeField]
    private TMP_Text _username;
    [SerializeField]
    private TMP_Text _kills;
    [SerializeField]
    private TMP_Text _death;
    [SerializeField]
    private TMP_Text _ready;

    public void SetPlayer(GameStateManager.Player player, int place)
    {
        _place.text = place.ToString();
        _username.text = player.Username;
        _kills.text = player.Kills.ToString();
        _death.text = player.Deaths.ToString();
        _ready.text = player.IsReady ? "Ready" : "Not Ready";
        _ready.color = player.IsReady ? Color.green : Color.red;
    }
}
