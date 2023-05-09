using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FishNet.Object;
using FishNet.Object.Synchronizing;

/*
Currently commenting out all references to sliding as it is not yet implemented.
*/

public enum Mode
{
    Parkour,
    Combat,
    // Sliding
}

public class ModeManager : NetworkBehaviour
{
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
            CurrentMode = Mode.Combat;
        }
        else if (CurrentMode == Mode.Combat)
        {
            CurrentMode = Mode.Parkour;
        }
    }

    public void ChangeToParkourMode()
    {
        if (CurrentMode != Mode.Parkour)
            CurrentMode = Mode.Parkour;
    }

    public void ChangeToCombatMode()
    {
        if (CurrentMode != Mode.Combat)
            CurrentMode = Mode.Combat;
    }

    /*
    public void ChangeToSlidingMode()
    {
        if (CurrentMode != Mode.Sliding)
            CurrentMode = Mode.Sliding;
    }
    */
}
