using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class PlayerInitializer : NetworkBehaviour
{

    [SerializeField]
    private PlayerHealth _playerHealth;

    public override void OnStartClient()
    {
        base.OnStartClient();

        InitializeServerRpc();

        Cursor.visible = false;
    }

    [ServerRpc]
    private void InitializeServerRpc()
    {
        PlayerManager.Instance.Players.Add(gameObject.GetInstanceID(), new PlayerManager.Player() { Health = 100, Username = "user123", PlayerHealth = _playerHealth, GameObject = gameObject, Connection = base.Owner });
    }

}
