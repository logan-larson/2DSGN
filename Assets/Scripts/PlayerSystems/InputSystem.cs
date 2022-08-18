using UnityEngine;

[RequireComponent (typeof(PlayerInput))]
[RequireComponent (typeof(MovementSystem))]
public class InputSystem : MonoBehaviour {

    PlayerInput input;
    MovementSystem movement;

    public void OnStart() {
        input = GetComponent<PlayerInput>();
        movement = GetComponent<MovementSystem>();

        InitializeInput();
    }

    public void OnUpdate() {
        UpdateHorizontalMovement();

        UpdateJump();

        UpdateSprint();
    }

    void InitializeInput() {
        input.horizontalMovementInput = 0f;
        input.verticalMovementInput = 0f;
        input.isJumpKeyPressed = false;
        input.isSprintKeyPressed = false;
        input.horizontalAimInput = 0f;
        input.verticalAimInput = 0f;
    }

    // Movement Input Subsystem
    void UpdateHorizontalMovement() {
        input.horizontalMovementInput = Input.GetAxisRaw("Horizontal");
        //input.horizontalMovementInput = Input.GetAxis("Horizontal");
    }

    void UpdateVerticalMovement() {
        input.verticalMovementInput = Input.GetAxisRaw("Vertical");
        //input.verticalMovementInput = Input.GetAxis("Vertical");
    }

    void UpdateJump() {
        input.isJumpKeyPressed = Input.GetKeyDown(KeyCode.W);
        if (input.isJumpKeyPressed) {
            movement.Jump();
        }
    }

    void UpdateSprint() {
        input.isSprintKeyPressed = Input.GetKey(KeyCode.LeftShift);
    }

    // Aim Input Subsystem
    void UpdateHorizontalAim() {
        input.horizontalAimInput = Input.GetAxisRaw("Mouse X");
    }

    void UpdateVerticalAim() {
        input.verticalAimInput = Input.GetAxisRaw("Mouse Y");
    }
}
