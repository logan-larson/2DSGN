using UnityEngine;

[RequireComponent (typeof (InputSystem))]
[RequireComponent (typeof (MovementSystem))]
[RequireComponent (typeof (AnimationSystem))]
public class PlayerSystem : MonoBehaviour {

    InputSystem inputSystem;
    MovementSystem movementSystem;
    AnimationSystem animationSystem;

    void Awake() {
        inputSystem = GetComponent<InputSystem>();
        movementSystem = GetComponent<MovementSystem>();
        animationSystem = GetComponent<AnimationSystem>();
    }

    void Start() {
        inputSystem.OnStart();
        movementSystem.OnStart();
        animationSystem.OnStart();
    }

    void Update() {
        inputSystem.OnUpdate();
        animationSystem.OnUpdate();
    }

    void FixedUpdate() {
        movementSystem.OnUpdate();
    }
}
