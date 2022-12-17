using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof(PlayerInputValues))]
[RequireComponent (typeof(MovementSystem))]
public class InputSystem : MonoBehaviour {

    PlayerInputValues input;
    MovementSystem movement;

    void Start() { // public void OnStart
        input = GetComponent<PlayerInputValues>();
        movement = GetComponent<MovementSystem>();
    }

    public void OnMove(InputValue value)
    {
        input.HorizontalMovementInput = value.Get<Vector2>().x;
    }

    public void OnSprint(InputValue value) {
        input.IsSprintKeyPressed = value.Get<float>() == 1f;
    }
    
    public void OnJump(InputValue value) {
        input.IsJumpKeyPressed = value.Get<float>() == 1f;
        //movement.Jump();
    }

}
