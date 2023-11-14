using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyPlayerListElement : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _place;
    [SerializeField]
    private TMP_Text _username;
    [SerializeField]
    private TMP_Text _kills;
    [SerializeField]
    private TMP_Text _death;

    public void SetPlayer(Player player, int place)
    {
        _place.text = place.ToString();
        _username.text = player.Username;
        _kills.text = player.Kills.ToString();
        _death.text = player.Deaths.ToString();
    }
}

public class Player
{
    public string Username { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
}
