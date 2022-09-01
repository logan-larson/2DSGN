using UnityEngine;

[RequireComponent (typeof (InputSystem))]
[RequireComponent (typeof (MovementSystem))]
[RequireComponent (typeof (AnimationSystem))]
[RequireComponent (typeof (CombatSystem))]
public class PlayerSystem : MonoBehaviour {

    InputSystem inputSystem;
    MovementSystem movementSystem;
    AnimationSystem animationSystem;
    CombatSystem combatSystem;

    void Awake() {
        inputSystem = GetComponent<InputSystem>();
        movementSystem = GetComponent<MovementSystem>();
        animationSystem = GetComponent<AnimationSystem>();
        combatSystem = GetComponent<CombatSystem>();
    }

    void Start() {
        inputSystem.OnStart();
        movementSystem.OnStart();
        animationSystem.OnStart();
        combatSystem.OnStart();
    }

    void Update() {
        inputSystem.OnUpdate();
        animationSystem.OnUpdate();
        combatSystem.OnUpdate();
    }

    void FixedUpdate() {
        movementSystem.OnUpdate();
    }
}
