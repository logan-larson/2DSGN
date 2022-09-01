using UnityEngine;

[RequireComponent (typeof(PlayerMode))]
public class CombatSystem : MonoBehaviour
{
    PlayerMode mode;

    public void OnStart()
    {
        mode = GetComponent<PlayerMode>();
    }

    public void OnUpdate()
    {
        
    }

    public void ShootPrimary() {
        mode.inCombatMode = true;
        mode.inParkourMode = false;
    }
}
