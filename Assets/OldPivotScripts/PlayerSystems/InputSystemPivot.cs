using UnityEngine;

[RequireComponent (typeof (PlayerInput))]
public class InputSystemPivot : MonoBehaviour {

    PlayerInput input;

    public void OnAwake() {
        input = GetComponent<PlayerInput>();

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
