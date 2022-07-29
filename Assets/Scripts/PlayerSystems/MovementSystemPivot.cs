using UnityEngine;

[RequireComponent (typeof (CollisionSystem))]

[RequireComponent (typeof (PlayerInput))]
[RequireComponent (typeof (PlayerVelocity))]
[RequireComponent (typeof (PlayerMovementProperties))]
[RequireComponent (typeof (PlayerCollision))]
public class MovementSystemPivot : MonoBehaviour {

    // Define system scripts
    CollisionSystem collisionSystem;

    // Define component scripts
    PlayerInput input;
    PlayerVelocity velocity;
    PlayerMovementProperties movementProperties;
    PlayerCollision collision;

    public void OnAwake() {
        // Get attached systems
		collisionSystem = GetComponent<CollisionSystem>();

        // Get attached components
        input = GetComponent<PlayerInput>();
        movementProperties = GetComponent<PlayerMovementProperties>();
        collision = GetComponent<PlayerCollision>();
        velocity = GetComponent<PlayerVelocity>();

        // Call subsystem OnAwake
        collisionSystem.OnAwake();

        InitializeMovement();
	}

    void InitializeMovement() {
		movementProperties.gravity = -(2 * movementProperties.jumpHeight) / Mathf.Pow (movementProperties.timeToJumpApex, 2);
		movementProperties.jumpVelocity = Mathf.Abs(movementProperties.gravity) * movementProperties.timeToJumpApex;
        movementProperties.sprintMultiplier = 2f;

        movementProperties.coyoteTime = 0.075f;
        movementProperties.timeSinceGrounded = 0.5f;
    }

	public void OnUpdate() {

        collisionSystem.ResetVelocityOnVerticalCollision();


        // Jumping
        if (collision.collisionInfo.below) {
            movementProperties.timeSinceGrounded = 0f;
        }

		if (input.isJumpKeyPressed && (collision.collisionInfo.below || movementProperties.timeSinceGrounded < movementProperties.coyoteTime)) {
			velocity.y = movementProperties.jumpVelocity;
            movementProperties.timeSinceGrounded = 0.5f;
		}

        if (movementProperties.timeSinceGrounded < 0.5f) {
            movementProperties.timeSinceGrounded += Time.deltaTime;
        }

        // Add gravity
		velocity.y += movementProperties.gravity * Time.deltaTime;

        // Sprinting
        float sprintMultiplier = input.isSprintKeyPressed ? movementProperties.sprintMultiplier : 1f;

        // Horizontal movement
		velocity.x = input.horizontalMovementInput * movementProperties.horizontalSpeed * sprintMultiplier;

        collisionSystem.UpdateRaycastOrigins();
        collisionSystem.ResetCollisionInfo();

		Vector3 velocityFromCollisions = collisionSystem.GetVelocityFromCollisions();

        transform.Translate(velocityFromCollisions);
    }
}