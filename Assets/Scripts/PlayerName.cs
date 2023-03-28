using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using TMPro;
using System;

public class PlayerName : NetworkBehaviour
{
    [SyncVar (OnChange = nameof(OnUsernameChanged))]
    public string Username;

    public UserInfo UserInfo;

    [SerializeField]
    private TMP_Text _usernameText;

    [SerializeField]
    private MovementSystem _movementSystem;

    private void OnUsernameChanged(string oldValue, string newValue, bool isServer)
    {
        _usernameText.text = newValue;
    }


    [SyncVar(OnChange = nameof(OnChangeMode))]
    public int Mode;

    private void OnChangeMode(int oldValue, int newValue, bool isServer)
    {
        if (newValue == 0)
        {
            transform.localPosition = new Vector3(0, 0, 0);
        }
        else
        {
            transform.localPosition = new Vector3(0, 1f, 0);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
        {
            _usernameText.color = Color.red;
            return;
        }

        _movementSystem = GetComponentInParent<MovementSystem>();

        _movementSystem.OnChangeToCombatMode += OnChangeToCombatMode;
        _movementSystem.OnChangeToParkourMode += OnChangeToParkourMode;

        SetMode(0);
        
        _usernameText.color = Color.green;

        Username = UserInfo.Username;

        ServerSetUsername(Username);
    }

    private void OnChangeToCombatMode(bool inCombat)
    {
        SetMode(1);
    }

    private void OnChangeToParkourMode(bool inParkour)
    {
        SetMode(0);
    }

    [ServerRpc]
    public void SetMode(int mode)
    {
        Mode = mode;
    }

    [ServerRpc]
    public void ServerSetUsername(string username)
    {
        Username = username;
    }
}
