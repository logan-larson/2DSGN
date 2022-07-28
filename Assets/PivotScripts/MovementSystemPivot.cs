using UnityEngine;

public class MovementSystemPivot : MonoBehaviour {

    // Define scripts
    PlayerInput input;
    PlayerPosition position;
    PlayerVelocity velocity;
    PlayerMovementProperties movementProperties;
    PlayerGrounded grounded;

    public void OnAwake() {
        // Get attached components
        input = gameObject.GetComponent<PlayerInput>();
        movementProperties = gameObject.GetComponent<PlayerMovementProperties>();
        position = gameObject.GetComponent<PlayerPosition>();
        velocity = gameObject.GetComponent<PlayerVelocity>();
        grounded = gameObject.GetComponent<PlayerGrounded>();

        InitializeMovement();
    }

    public void OnUpdate() {
        UpdateGrounded();

        UpdateVelocity();

        UpdatePosition();

        ApplyMovement();
    }

    void InitializeMovement() {
        // Position
        position.x = transform.position.x;
        position.y = transform.position.y;

        movementProperties.maxXSpeed = 5f;
        movementProperties.maxYSpeed = 5f;
        movementProperties.movementAcceleration = 0.025f;
        movementProperties.maxSpeedSprintMultiplier = 1.5f;
        movementProperties.groundedHeight = 1f;
        movementProperties.collisionDistance = 1f;
    }

    void UpdateGrounded() {
        RaycastHit2D hitBelow = Physics2D.Raycast(
            new Vector2(position.x, position.y),
            Vector2.down,
            movementProperties.groundedHeight);

        grounded.isGrounded = hitBelow.collider != null ? true : false;
    }

    void UpdateVelocity() {
        // Gravity
        if (grounded.isGrounded) {
            velocity.y = 0f;
        } else {
            velocity.y -= 5f;
        }

        // Sprinting
        float maxXSpeed = input.isSprintKeyPressed ?
            movementProperties.maxXSpeed * movementProperties.maxSpeedSprintMultiplier :
            movementProperties.maxXSpeed;

        // Horizontal input
        float t = Time.deltaTime / movementProperties.movementAcceleration; // 0.5f is duration of acceleration period
        // TODO look into if this is taxing on the CPU
        velocity.x = Mathf.SmoothStep(velocity.x, maxXSpeed * input.horizontalMovementInput, t);

        if (IsCollidingLeft()) {
            velocity.x = Mathf.Clamp(velocity.x, 0f, maxXSpeed);
        } else if (IsCollidingRight()) {
            velocity.x = Mathf.Clamp(velocity.x, -maxXSpeed, 0f);
        } else {
            velocity.x = Mathf.Clamp(velocity.x, -maxXSpeed, maxXSpeed);
        }

        // Clamp velocity to max
        velocity.y = Mathf.Clamp(velocity.y, -movementProperties.maxYSpeed, movementProperties.maxYSpeed);

    }

    bool IsCollidingLeft() {
        RaycastHit2D hitLeft = Physics2D.Raycast(
            new Vector2(position.x, position.y),
            Vector2.left,
            movementProperties.collisionDistance);

        return hitLeft.collider != null;
    }

    bool IsCollidingRight() {
        RaycastHit2D hitRight = Physics2D.Raycast(
            new Vector2(position.x, position.y),
            Vector2.right,
            movementProperties.collisionDistance);

        return hitRight.collider != null;
    }

    void UpdatePosition() {
        position.x += velocity.x * Time.deltaTime;
        position.y += velocity.y * Time.deltaTime;
    }

    void ApplyMovement() {
        transform.position = new Vector3(position.x, position.y);
    }
}
