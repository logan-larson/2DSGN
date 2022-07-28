using UnityEngine;

public class InputSystemPivot : MonoBehaviour {

    PlayerInput input;

    public void OnAwake() {
        input = gameObject.GetComponent<PlayerInput>();
    }

    public void OnUpdate() {
        UpdateHorizontalMovement();
        UpdateSprint();
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
        input.isJumpKeyPressed = Input.GetKey(KeyCode.Space);
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
