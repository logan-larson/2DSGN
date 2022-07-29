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
    }

	public void OnUpdate() {

        collisionSystem.ResetVelocityOnVerticalCollision();

		if (input.isJumpKeyPressed && collision.collisionInfo.below) {
			velocity.y = movementProperties.jumpVelocity;
		}

		velocity.y += movementProperties.gravity * Time.deltaTime;

		velocity.x = input.horizontalMovementInput * movementProperties.horizontalSpeed;

        collisionSystem.UpdateRaycastOrigins();
        collisionSystem.ResetCollisionInfo();

		Vector3 velocityFromCollisions = collisionSystem.GetVelocityFromCollisions();

        transform.Translate(velocityFromCollisions);
    }
}