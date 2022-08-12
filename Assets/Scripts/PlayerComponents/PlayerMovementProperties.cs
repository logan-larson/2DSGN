using UnityEngine;

public class PlayerMovementProperties : MonoBehaviour
{
	public float horizontalSpeed;

    public float jumpVelocity = 5f;

    public float maxSprintSpeed = 10f;
    public float sprintMultiplier;

    public float timeSinceGrounded;
    public float coyoteTime;

    public float jumpHeight;
	public float timeToJumpApex;

    /* Repivot properties */
    public float maxXSpeed = 6f;
    public float maxYSpeed = 5f;
    public float acceleration = 0.1f;
    public float friction = 0.1f;

    public float gravity = 5f;
    public float maxRotationDegrees = 1f;
    public float groundedHeight = 2f;
    public float overrideRayLength = 0.25f;

}