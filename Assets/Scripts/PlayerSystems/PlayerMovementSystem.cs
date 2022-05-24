using UnityEngine;

public class PlayerMovementSystem : MonoBehaviour
{

    PlayerInput input;
    PlayerMovement movement;

    public void OnStart()
    {
        input = gameObject.GetComponent<PlayerInput>();
        movement = gameObject.GetComponent<PlayerMovement>();
        
        MovementStart();
    }

    public void OnUpdate()
    {
        UpdateGrounded();

        PredictLandingPosition();

        MatchBodyToGround();

        UpdateVelocity();

        AddGravity();
        
        // Testing purposes only
        UpdateReset();

        SetHeightAboveGround();

        UpdatePosition();
    }

    void MovementStart()
    {
        movement.position = gameObject.GetComponent<PlayerPosition>();
        movement.position.x = transform.position.x;
        movement.position.y = transform.position.y;

        movement.velocity = gameObject.GetComponent<PlayerVelocity>();
        movement.grounded = gameObject.GetComponent<PlayerGrounded>();
        movement.grounded.isGrounded = false;

        movement.jump = gameObject.GetComponent<PlayerJump>();
        movement.mode = gameObject.GetComponent<PlayerMode>();
        movement.properties = gameObject.GetComponent<PlayerMovementProperties>();
    }

    void UpdateGrounded()
    {
        RaycastHit2D hit = GetGroundRaycastHit();

        movement.grounded.isGrounded = hit.collider != null ? true : false;
    }
    
    RaycastHit2D GetGroundRaycastHit()
    {
        float radRotation = (movement.position.rotation - 90) * Mathf.Deg2Rad;
        Vector2 down = new Vector2(Mathf.Cos(radRotation), Mathf.Sin(radRotation));
        down.Normalize();

        Debug.DrawRay(transform.position, down, Color.green);

        LayerMask environmentMask = LayerMask.GetMask("Environment");

        return Physics2D.Raycast(transform.position, down, movement.properties.groundedHeightThreshold, environmentMask);
    }

    void MatchBodyToGround()
    {
        if (movement.grounded.isGrounded) {
            RaycastHit2D hit = GetGroundRaycastHit();

            float rotation = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg;

            Debug.DrawRay(hit.point, hit.normal, Color.red);

            movement.position.rotation = rotation - 90f;
        }
    }

    void PredictLandingPosition()
    {
        // Airborne, calculate landing position
        if (!movement.grounded.isGrounded) {
            
        }
    }

    // Lateral Movement Subsystem
    void UpdateVelocity()
    {
        // Calculate scaling vectors for x and y based on rotation
        // TODO Clean this up
        float radRotation = movement.position.rotation * Mathf.Deg2Rad;
        float xScale = Mathf.Cos(radRotation);
        float yScale = Mathf.Sin(radRotation);

        float xScaled = (Mathf.Abs(xScale) / (Mathf.Abs(xScale) + Mathf.Abs(yScale))) * Mathf.Sign(xScale);
        float yScaled = (Mathf.Abs(yScale) / (Mathf.Abs(xScale) + Mathf.Abs(yScale))) * Mathf.Sign(yScale);

        // Adjust velocity for player movement input
        if (movement.grounded.isGrounded) {
            movement.velocity.x = input.horizontalMovementInput * movement.properties.movementAcceleration;
            movement.velocity.y = input.horizontalMovementInput * movement.properties.movementAcceleration;
        }

        /*
        // Apply friction if not applying input
        if (Mathf.Abs(movement.velocity.x) < 0.25f) {
            movement.velocity.x = 0f;
        } else {
            movement.velocity.x *= movement.properties.friction;
        }

        if (Mathf.Abs(movement.velocity.y) < 0.25f) {
            movement.velocity.y = 0f;
        } else {
            movement.velocity.y *= movement.properties.friction;
        }
        */
        
        // Clamp movement speed to max
        movement.velocity.x = Mathf.Clamp(movement.velocity.x, -movement.properties.maxXSpeed * xScaled, movement.properties.maxXSpeed * xScaled);
        movement.velocity.y = Mathf.Clamp(movement.velocity.y, -movement.properties.maxYSpeed * yScaled, movement.properties.maxYSpeed * yScaled);

        if (input.horizontalMovementInput == 0f) {
            movement.velocity.x = 0f;
            movement.velocity.y = 0f;
        }

        float sprintMultiplier = input.isSprintKeyPressed ? movement.properties.maxSpeedSprintMultiplier : 1f;

        movement.velocity.x *= sprintMultiplier;
        movement.velocity.y *= sprintMultiplier;

    }

    void AddGravity()
    {
        if (!movement.grounded.isGrounded) {
            movement.velocity.y -= movement.properties.gravity;
        }
    }

    void UpdateReset()
    {
        if (input.isResetKeyPressed) {
            movement.position.x = 0f;
            movement.position.y = 0f;
            movement.position.rotation = 0f;
        }
    }

    void SetHeightAboveGround()
    {
        if (movement.grounded.isGrounded) {
            RaycastHit2D hit = GetGroundRaycastHit();

            movement.position.x = hit.point.x + (hit.normal.x * movement.properties.groundedHeight);
            movement.position.y = hit.point.y + (hit.normal.y * movement.properties.groundedHeight);
        }
    }

    // General Movement Subsystem
    void UpdatePosition()
    {

        movement.position.x += movement.velocity.x * Time.deltaTime;
        movement.position.y += movement.velocity.y * Time.deltaTime;

        Vector3 position = new Vector3(movement.position.x, movement.position.y, 0f);
        Quaternion rotation = Quaternion.Euler(0f, 0f, movement.position.rotation);
        transform.SetPositionAndRotation(position, rotation);
    }

}
