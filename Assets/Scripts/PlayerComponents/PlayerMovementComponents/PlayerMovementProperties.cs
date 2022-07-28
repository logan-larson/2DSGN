using UnityEngine;

public class PlayerMovementProperties : MonoBehaviour
{
    public float maxXSpeed;
    public float maxYSpeed;

    public float movementAcceleration;

    public float gravity;
    public float jumpAcceleration;
    public float jumpVelocity;

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
}