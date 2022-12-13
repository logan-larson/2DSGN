using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof(PlayerInput))]
[RequireComponent (typeof(MovementSystem))]
public class InputSystem : MonoBehaviour {

    PlayerInput input;
    MovementSystem movement;

    void Start() { // public void OnStart
        input = GetComponent<PlayerInput>();
        movement = GetComponent<MovementSystem>();
    }

    public void OnMove(InputValue value) {
        input.horizontalMovementInput = value.Get<Vector2>().x;
    }

    public void OnSprint(InputValue value) {
        input.isSprintKeyPressed = value.Get<float>() == 1f;
    }
    
    public void OnJump(InputValue value) {
        movement.Jump();
    }

}
