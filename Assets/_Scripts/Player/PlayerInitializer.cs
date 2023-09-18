using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class PlayerInitializer : NetworkBehaviour
{

    [SerializeField]
    private PlayerHealth _playerHealth;
    [SerializeField]
    private ModeManager _modeManager;
    [SerializeField]
    private NetworkDirection _networkDirection;

    [SerializeField]
    private GameObject _crosshair;

    [SerializeField]
    private LineRenderer _lineRenderer;

    public override void OnStartClient()
    {
        base.OnStartClient();

        InitializeServerRpc();

        _modeManager.ChangeMode();

        _networkDirection.ServerSetDirection(false);
        
        _lineRenderer.enabled = false;

        if (!base.IsOwner)
        {
            _crosshair.SetActive(false);
        }
    }

    [ServerRpc]
    private void InitializeServerRpc()
    {
        if (PlayerManager.Instance is null) return;

        PlayerManager.Instance.Players.Add(gameObject.GetInstanceID(), new PlayerManager.Player() { PlayerHealth = _playerHealth, GameObject = gameObject, Connection = base.Owner });

        if (GameStateManager.Instance is null) return;

        GameStateManager.Instance.PlayerJoined(gameObject.GetInstanceID(), new GameStateManager.Player() { Connection = base.Owner, GameObject = gameObject });
    }
}
