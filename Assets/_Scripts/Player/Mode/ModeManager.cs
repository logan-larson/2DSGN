using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FishNet.Object;
using FishNet.Object.Synchronizing;


public class ModeManager : NetworkBehaviour
{
    /*
    Currently commenting out all references to sliding as it is not yet implemented.
    */

    public enum Mode
    {
        Parkour,
        Combat,
        // Sliding
    }

    public UnityEvent OnChangeToParkour = new UnityEvent();
    public UnityEvent OnChangeToCombat = new UnityEvent();
    // public UnityEvent ChangeToSliding = new UnityEvent();

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
        /*
        else if (newValue == Mode.Sliding)
        {
            ChangeToSliding.Invoke();
        }
        */
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;
    }

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

    /*
    public void ChangeToSlidingMode()
    {
        if (CurrentMode != Mode.Sliding)
            CurrentMode = Mode.Sliding;
    }
    */

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
