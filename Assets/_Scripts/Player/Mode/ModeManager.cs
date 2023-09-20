using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using FishNet.Transporting;
using System.Security.Cryptography;

public class ModeManager : NetworkBehaviour
{
    public enum Mode
    {
        Parkour,
        Combat,
        Sliding
    }

    public UnityEvent OnChangeToParkour = new UnityEvent();
    public UnityEvent OnChangeToCombat = new UnityEvent();
    public UnityEvent OnChangeToSliding = new UnityEvent();

    [SyncVar(OnChange = nameof(OnChangeMode))]
    public Mode CurrentMode = Mode.Parkour; 

    private void OnChangeMode(Mode oldValue, Mode newValue, bool isServer)
    {
        if (newValue == Mode.Parkour)
        {
            OnChangeToParkour.Invoke();
        }
        else if (newValue == Mode.Combat)
        {
            OnChangeToCombat.Invoke();
        }
        else if (newValue == Mode.Sliding)
        {
            OnChangeToSliding.Invoke();
        }
    }

    private void Start() { }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;
    }

    /* This was my attempt at syncing the mode between clients. It didn't work for now...
    public override void OnStartServer()
    {
        base.OnStartServer();

        // Listen for new connections.
        ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        // When a new connection is made, send the current mode to the new connection so they can sync.
        TargetSetMode(conn, CurrentMode);
    }

    [TargetRpc]
    private void TargetSetMode(NetworkConnection conn, Mode mode)
    {
        CurrentMode = mode;
    }
    */

    // This is called by the dedicated change mode button which refers to changing between parkour and combat.
    public void ChangeMode()
    {
        if (CurrentMode == Mode.Parkour)
        {
            ChangeModeServer(Mode.Combat);
        }
        else if (CurrentMode == Mode.Combat)
        {
            ChangeModeServer(Mode.Parkour);
        }
    }

    public void ChangeToParkourMode()
    {
        if (CurrentMode != Mode.Parkour)
            ChangeModeServer(Mode.Parkour);
    }

    public void ChangeToCombatMode()
    {
        if (CurrentMode != Mode.Combat)
            ChangeModeServer(Mode.Combat);
    }

    public void ChangeToSlidingMode()
    {
        if (CurrentMode != Mode.Sliding)
            CurrentMode = Mode.Sliding;
    }

    [ServerRpc]
    private void ChangeModeServer(Mode mode)
    {
        CurrentMode = mode;
        ChangeModeObservers(mode);
    }

    [ObserversRpc]
    private void ChangeModeObservers(Mode mode)
    {
        CurrentMode = mode;
    }
}
