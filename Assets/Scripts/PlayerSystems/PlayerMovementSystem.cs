using UnityEngine;

public class PlayerMovementSystem : MonoBehaviour
{
    PlayerInput input;
    PlayerMovement movement;
    PlayerMovementProperties movementProperties;

    public void OnStart()
    {
        input = gameObject.GetComponent<PlayerInput>();
        movement = gameObject.GetComponent<PlayerMovement>();
        movementProperties = gameObject.GetComponent<PlayerMovementProperties>();
        
        MovementStart();
    }

    public void OnUpdate()
    {
        UpdateVelocity();
        
        UpdatePosition();
    }

    void MovementStart()
    {
        movement.position = gameObject.GetComponent<PlayerPosition>();
        movement.velocity = gameObject.GetComponent<PlayerVelocity>();
        movement.grounded = gameObject.GetComponent<PlayerGrounded>();
        movement.jump = gameObject.GetComponent<PlayerJump>();
        movement.mode = gameObject.GetComponent<PlayerMode>();
    }

    // Lateral Movement Subsystem
    void UpdateVelocity()
    {
        
    }

    // General Movement Subsystem
    void UpdatePosition()
    {

    }

}
