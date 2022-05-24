using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputSystem : MonoBehaviour
{

    PlayerInput inputComponent;

    public void OnStart()
    {
        inputComponent = gameObject.GetComponent<PlayerInput>();

        // Initialize component values
        inputComponent.horizontalMovementInput = 0;
        inputComponent.isJumpKeyPressed = false;
        inputComponent.isSprintKeyPressed = false;
        inputComponent.horizontalAimInput = 0;
        inputComponent.verticalAimInput = 0;
    }

    public void OnUpdate()
    {
        // Update input component values based on player inputs
        inputComponent.horizontalMovementInput = Input.GetAxis("Horizontal");
        inputComponent.isJumpKeyPressed = Input.GetKeyDown(KeyCode.Space);
        inputComponent.isSprintKeyPressed = Input.GetKey(KeyCode.LeftShift);
        inputComponent.horizontalAimInput = Input.GetAxisRaw("Mouse X");
        inputComponent.verticalAimInput = Input.GetAxisRaw("Mouse Y");
    }
}
