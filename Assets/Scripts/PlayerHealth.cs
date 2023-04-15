using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar]
    public int Health = 10;

    [SerializeField]
    private TMP_Text _healthText;

    private void Update()
    {
        _healthText.text = $"Health: {Health.ToString()}";
    }

}
