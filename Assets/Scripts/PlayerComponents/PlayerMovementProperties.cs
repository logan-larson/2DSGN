using UnityEngine;

public class PlayerMovementProperties : MonoBehaviour
{
	public float horizontalSpeed = 6;

    public float movementAcceleration;

    public float gravity;
    public float jumpAcceleration;
    public float jumpVelocity;
    public float velocityXSmoothing;

    public float maxSpeedSprintMultiplier;

    public float groundedHeightThreshold;
    public float groundedHeight;

    public float jumpDuration;
    public float timeSinceJump;

    public float maxSpeedCombatMultiplier;

    public Vector2 verticalDistance;
    public Vector2 horizontalDistance;

    public bool isCollidingAbove;
    public float timeSinceCollidingAbove;
    public float collidingAboveDuration;


    public float jumpHeight = 4;
	public float timeToJumpApex = .4f;
	public float accelerationTimeAirborne = .2f;
	public float accelerationTimeGrounded = .1f;

}