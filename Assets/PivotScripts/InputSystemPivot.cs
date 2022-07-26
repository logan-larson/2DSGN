using UnityEngine;

public class InputSystemPivot : MonoBehaviour
{
    PlayerInput input;

    public void OnAwake()
    {
        input = gameObject.GetComponent<PlayerInput>();
        
        InitializeInput();
    }

    public void OnUpdate()
    {
        UpdateHorizontalMovement();
    }

    void InitializeInput()
    {
        input.horizontalMovementInput = 0;
        input.verticalMovementInput = 0;
        input.isJumpKeyPressed = false;
        input.isSprintKeyPressed = false;
        input.horizontalAimInput = 0;
        input.verticalAimInput = 0;
    }

    // Movement Input Subsystem
    void UpdateHorizontalMovement()
    {
        input.horizontalMovementInput = Input.GetAxisRaw("Horizontal");
        //input.horizontalMovementInput = Input.GetAxis("Horizontal");
    }

    void UpdateVerticalMovement()
    {
        input.verticalMovementInput = Input.GetAxisRaw("Vertical");
        //input.verticalMovementInput = Input.GetAxis("Vertical");
    }

    void UpdateJump()
    {
        input.isJumpKeyPressed = Input.GetButtonDown("Jump");
    }

    void UpdateSprint()
    {
        input.isSprintKeyPressed = Input.GetKey(KeyCode.LeftShift);
    }

    // Aim Input Subsystem
    void UpdateHorizontalAim()
    {
        input.horizontalAimInput = Input.GetAxisRaw("Mouse X");
    }

    void UpdateVerticalAim()
    {
        input.verticalAimInput = Input.GetAxisRaw("Mouse Y");
    }
}
