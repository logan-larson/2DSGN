using UnityEngine;

public class PlayerInputSystem : MonoBehaviour
{

    public PlayerInput input;

    public void OnStart()
    {
        input = gameObject.GetComponent<PlayerInput>();

        // Initialize component values
        input.horizontalMovementInput = 0;
        input.verticalMovementInput = 0;
        input.isJumpKeyPressed = false;
        input.isSprintKeyPressed = false;
        input.horizontalAimInput = 0;
        input.verticalAimInput = 0;
    }

    public void OnUpdate()
    {
        // Update input component values based on player inputs
        UpdateHorizontalMovementInput();
        UpdateJumpInput();
        UpdateSprintInput();
        UpdateHorizontalAimInput();
        UpdateVerticalAimInput();

        // Testing purposes only
        UpdateResetInput();
    }

    // Movement Input Subsystem
    void UpdateHorizontalMovementInput()
    {
        //input.horizontalMovementInput = Input.GetAxisRaw("Horizontal");
        input.horizontalMovementInput = Input.GetAxis("Horizontal");
    }

    void UpdateVerticalMovementInput()
    {
        //input.horizontalMovementInput = Input.GetAxisRaw("Horizontal");
        input.verticalMovementInput = Input.GetAxis("Vertical");
    }

    void UpdateJumpInput()
    {
        input.isJumpKeyPressed = Input.GetButtonDown("Jump");
    }

    void UpdateSprintInput()
    {
        input.isSprintKeyPressed = Input.GetKey(KeyCode.LeftShift);
    }

    // Aim Input Subsystem
    void UpdateHorizontalAimInput()
    {
        input.horizontalAimInput = Input.GetAxisRaw("Mouse X");
    }

    void UpdateVerticalAimInput()
    {
        input.verticalAimInput = Input.GetAxisRaw("Mouse Y");
    }

    void UpdateResetInput()
    {
        input.isResetKeyPressed = Input.GetKeyDown(KeyCode.R);
    }

}
