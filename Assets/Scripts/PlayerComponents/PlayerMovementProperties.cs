using UnityEngine;

public class PlayerMovementProperties : MonoBehaviour
{
	public float horizontalSpeed;

    //public float gravity;
    public float jumpVelocity;

    public float sprintMultiplier;

    public float timeSinceGrounded;
    public float coyoteTime;

    public float jumpHeight;
	public float timeToJumpApex;

    /* Repivot properties */
    public float moveSpeed = 5f;
    public float gravity = 5f;
    public float maxRotationDegrees = 1f;
    public float groundedHeight = 2f;

}