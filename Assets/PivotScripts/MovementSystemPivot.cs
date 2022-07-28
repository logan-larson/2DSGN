using UnityEngine;

[RequireComponent (typeof (CollisionSystem))]

[RequireComponent (typeof (PlayerInput))]
[RequireComponent (typeof (PlayerPosition))]
[RequireComponent (typeof (PlayerVelocity))]
[RequireComponent (typeof (PlayerMovementProperties))]
[RequireComponent (typeof (PlayerGrounded))]
public class MovementSystemPivot : MonoBehaviour {

    // Define system scripts
    CollisionSystem collisionSystem;

    // Define component scripts
    PlayerInput input;
    PlayerPosition position;
    PlayerVelocity velocity;
    PlayerMovementProperties movementProperties;
    PlayerGrounded grounded;

    // Testing vars
    public float gravity;
    public float minJumpVelocity;
    public float maxJumpVelocity;

    public float minJumpHeight = 1f;
    public float maxJumpHeight = 4f;
    public float timeToJumpApex = 0.4f;

    public void OnAwake() {

        // Get attached systems
        collisionSystem = GetComponent<CollisionSystem>();

        // Get attached components
        input = GetComponent<PlayerInput>();
        movementProperties = GetComponent<PlayerMovementProperties>();
        position = GetComponent<PlayerPosition>();
        velocity = GetComponent<PlayerVelocity>();
        grounded = GetComponent<PlayerGrounded>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        collisionSystem.OnAwake();
        InitializeMovement();
    }

    public void OnUpdate() {
        UpdateGrounded();

        UpdateVelocity();

        ApplyMovement();
    }

    void InitializeMovement() {
        // Position
        position.x = transform.position.x;
        position.y = transform.position.y;

        movementProperties.maxXSpeed = 10f;
        movementProperties.maxYSpeed = 20f;
        movementProperties.movementAcceleration = 0.025f;

        movementProperties.jumpAcceleration = 30f;
        movementProperties.jumpVelocity = 20f;
        movementProperties.maxSpeedSprintMultiplier = 1.5f;
        movementProperties.groundedHeight = 1f;
        movementProperties.jumpDuration = 0.15f;
        movementProperties.timeSinceJump = 3f;

        movementProperties.verticalDistance = new Vector2(0f, 0.75f);
        movementProperties.horizontalDistance = new Vector2(0.3f, 0f);
        
        movementProperties.isCollidingAbove = false;
        movementProperties.timeSinceCollidingAbove = 3f;
        movementProperties.collidingAboveDuration = 0.25f;
    }

    void UpdateGrounded() {
        RaycastHit2D hitBelow = Physics2D.Raycast(
            new Vector2(position.x, position.y),
            Vector2.down,
            movementProperties.groundedHeight);

        RaycastHit2D hitBelowLeft = Physics2D.Raycast(
            new Vector2(position.x, position.y) - movementProperties.horizontalDistance,
            Vector2.down,
            movementProperties.groundedHeight);

        RaycastHit2D hitBelowRight = Physics2D.Raycast(
            new Vector2(position.x, position.y) + movementProperties.horizontalDistance,
            Vector2.down,
            movementProperties.groundedHeight);

        if (movementProperties.timeSinceJump < movementProperties.jumpDuration) {
            grounded.isGrounded = false;
        } else {
            grounded.isGrounded = hitBelow.collider || hitBelowLeft.collider || hitBelowRight.collider;
        }
    }

    void UpdateVelocity() {
        // Gravity
        if (grounded.isGrounded) {
            velocity.y = 0f;
        } else {
            velocity.y += gravity * Time.deltaTime;
        }

        if (movementProperties.timeSinceJump < movementProperties.jumpDuration) {
            grounded.isGrounded = false;
        }

        // Sprinting
        float maxXSpeed = input.isSprintKeyPressed ?
            movementProperties.maxXSpeed * movementProperties.maxSpeedSprintMultiplier :
            movementProperties.maxXSpeed;

        // Horizontal input
        float t = Time.deltaTime / movementProperties.movementAcceleration; // 0.5f is duration of acceleration period
        // TODO look into if this is taxing on the CPU
        velocity.x = Mathf.SmoothStep(velocity.x, maxXSpeed * input.horizontalMovementInput, t);

        if (velocity.x < 0f && IsCollidingLeft()) {
            velocity.x = Mathf.Clamp(velocity.x, 0f, maxXSpeed);
        } else if (velocity.x > 0f && IsCollidingRight()) {
            velocity.x = Mathf.Clamp(velocity.x, -maxXSpeed, 0f);
        } else if (Mathf.Abs(velocity.x) < 0.025f) {
            velocity.x = 0f;
        } else {
            velocity.x = Mathf.Clamp(velocity.x, -maxXSpeed, maxXSpeed);
        }

        // Jumping
        // Allow player to get off ground
        if (movementProperties.timeSinceJump < movementProperties.jumpDuration) {
            grounded.isGrounded = false;
        }

        if (grounded.isGrounded && input.isJumpKeyPressed) {
            grounded.isGrounded = false;
            velocity.y = maxJumpVelocity;
            movementProperties.timeSinceJump = 0f;
        } else {
            // Clamp velocity to max
            if (movementProperties.timeSinceJump >= movementProperties.jumpDuration) {
                velocity.y = Mathf.Clamp(velocity.y, -movementProperties.maxYSpeed, movementProperties.maxYSpeed);
            }
        }

        // Above collisions
        /*
        if (IsCollidingAbove() && !movementProperties.isCollidingAbove) {
            movementProperties.isCollidingAbove = true;
            movementProperties.timeSinceCollidingAbove = 0f;
            velocity.y = 0f;
        } else if (movementProperties.isCollidingAbove && movementProperties.timeSinceCollidingAbove < movementProperties.collidingAboveDuration) {
            velocity.y += gravity * Time.deltaTime;
        } else {
            movementProperties.isCollidingAbove = false;
        }
        
        if (movementProperties.timeSinceCollidingAbove < 3f) {
            movementProperties.timeSinceCollidingAbove += Time.deltaTime;
        }
        */

        if (movementProperties.timeSinceJump < 3f) {
            movementProperties.timeSinceJump += Time.deltaTime;
        }
    }

    bool IsCollidingLeft() {
        RaycastHit2D hitLeft = Physics2D.Raycast(
            new Vector2(position.x, position.y),
            Vector2.left,
            movementProperties.horizontalDistance.magnitude
        );

        RaycastHit2D hitLeftTop = Physics2D.Raycast(
            new Vector2(position.x, position.y) + movementProperties.verticalDistance,
            Vector2.left,
            movementProperties.horizontalDistance.magnitude
        );

        RaycastHit2D hitLeftBottom = Physics2D.Raycast(
            new Vector2(position.x, position.y) - movementProperties.verticalDistance,
            Vector2.left,
            movementProperties.horizontalDistance.magnitude
        );

        return hitLeft.collider || hitLeftTop.collider || hitLeftBottom.collider;
    }

    bool IsCollidingRight() {
        RaycastHit2D hitRight = Physics2D.Raycast(
            new Vector2(position.x, position.y),
            Vector2.right,
            movementProperties.horizontalDistance.magnitude
        );

        RaycastHit2D hitRightTop = Physics2D.Raycast(
            new Vector2(position.x, position.y) + movementProperties.verticalDistance,
            Vector2.right,
            movementProperties.horizontalDistance.magnitude
        );

        RaycastHit2D hitRightBottom = Physics2D.Raycast(
            new Vector2(position.x, position.y) - movementProperties.verticalDistance,
            Vector2.right,
            movementProperties.horizontalDistance.magnitude
        );

        return hitRight.collider || hitRightTop.collider || hitRightBottom.collider;
    }
    
    bool IsCollidingAbove() {
        RaycastHit2D hitAbove = Physics2D.Raycast(
            new Vector2(position.x, position.y),
            Vector2.up,
            movementProperties.groundedHeight);

        RaycastHit2D hitAboveLeft = Physics2D.Raycast(
            new Vector2(position.x, position.y) - movementProperties.horizontalDistance,
            Vector2.up,
            movementProperties.groundedHeight);

        RaycastHit2D hitAboveRight = Physics2D.Raycast(
            new Vector2(position.x, position.y) + movementProperties.horizontalDistance,
            Vector2.up,
            movementProperties.groundedHeight);

        return hitAbove.collider || hitAboveLeft.collider || hitAboveRight.collider;
    }

    void ApplyMovement() {
        collisionSystem.UpdateRaycastOrigins();

        transform.Translate(new Vector3(velocity.x, velocity.y) * Time.deltaTime);
    }
}
