using UnityEngine;

public class PlayerMovementSettings : MonoBehaviour
{
    /*
    public float jumpVelocity = 5f;

    public float maxSprintSpeed = 10f;
    public float sprintMultiplier;

    public float timeSinceGrounded = 1;
    public float coyoteTime;

    public float jumpHeight;
	public float timeToJumpApex;

    public float maxXSpeed = 6f;
    public float maxYSpeed = 5f;
    public float friction = 0.1f;

    public float gravity = 5f;
    public float maxRotationDegrees = 1f;
    public float groundedHeight = 2f;
    public float overrideRayLength = 0.25f;
    */

    /// <summary>
    /// The amount by which the player's speed is changed when they move.
    /// </summary>
    public float Acceleration = 0.1f;
    /// <summary>
    /// The amount by which the player's speed is changed when they aren't moving.
    /// </summary>
    public float Friction = 0.1f;
    /// <summary>
    /// The maximum speed at which the player can move.
    /// </summary>
    public float MaxSpeed = 5f;

}