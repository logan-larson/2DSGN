using UnityEngine;

[RequireComponent (typeof(PlayerMode))]
public class CombatSystem : MonoBehaviour
{
    PlayerMode mode;

    void Start() { // public void OnStart
        mode = GetComponent<PlayerMode>();
    }

    void Update() { // public void OnUpdate
        
    }

    public void ShootPrimary() {
        mode.inCombatMode = true;
        mode.inParkourMode = false;
    }
}
