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

        UpdateJumping();

        UpdateVelocity();

        AddGravity();
        

        // Testing purposes only
        UpdateReset();

        UpdatePosition();

        SetHeightAboveGround();
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
        if (!movement.jump.isJumping)
        {
            RaycastHit2D hit = GetGroundRaycastHitBelow();

            movement.grounded.isGrounded = hit.collider != null ? true : false;
        }
        else
        {
            movement.grounded.isGrounded = false;
        }
    }

    RaycastHit2D GetGroundRaycastHitBelow()
    {
        float radRotation = (movement.position.rotation - 90) * Mathf.Deg2Rad;

        Vector2 down = new Vector2(Mathf.Cos(radRotation), Mathf.Sin(radRotation));
        
        down.Normalize();

        Debug.DrawRay(transform.position, down, Color.green);

        LayerMask environmentMask = LayerMask.GetMask("Environment");

        return Physics2D.Raycast(transform.position, down, movement.properties.groundedHeightThreshold, environmentMask);
    }
    
    RaycastHit2D GetGroundRaycastHitClosest()
    {

        RaycastHit2D hit = GetGroundRaycastHitBelow();

        Vector3 leftPos = transform.position - (transform.right / 4);
        Vector3 rightPos = transform.position + (transform.right / 4);
        Vector2 left = hit.point - new Vector2(leftPos.x, leftPos.y);
        Vector2 right = hit.point - new Vector2(rightPos.x, rightPos.y);
        
        left.Normalize();
        right.Normalize();

        Debug.DrawRay(transform.position - (transform.right / 4), left, Color.yellow);
        Debug.DrawRay(transform.position + (transform.right / 4), right, Color.blue);

        LayerMask environmentMask = LayerMask.GetMask("Environment");

        RaycastHit2D leftHit = Physics2D.Raycast(transform.position - (transform.right / 4), left, movement.properties.groundedHeightThreshold, environmentMask);
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position + (transform.right / 4), right, movement.properties.groundedHeightThreshold, environmentMask);

        return rightHit.distance < leftHit.distance ? rightHit : leftHit;
    }

    void MatchBodyToGround()
    {
        if (movement.grounded.isGrounded) {
            RaycastHit2D hit = GetGroundRaycastHitBelow();
            //RaycastHit2D hit = GetGroundRaycastHitClosest();

            //Debug.DrawRay(transform.position, new Vector3(closestHit.point.x, closestHit.point.y, 0f) - transform.position);

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

    void UpdateJumping()
    {
        // If player is grounded, don't add jump force
        if (movement.grounded.isGrounded)
        {
            movement.jump.isJumping = false;
            movement.jump.timeFromLastJump = 0f;
        }

        // If player is able to jump setup jump forces
        if (input.isJumpKeyPressed && movement.jump.timeFromLastJump == 0f)
        {
            movement.jump.isJumping = true;
  
            var mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, transform.position.z - Camera.main.transform.position.z));

            Vector3 dir = mousePos - transform.position;

            dir = Vector3.Normalize(dir);

            movement.jump.x = dir.x;
            movement.jump.y = dir.y;
        }

        // Limit duration of jump force being applied
        if (movement.jump.isJumping && movement.jump.timeFromLastJump < 0.03f) 
        {
            movement.jump.timeFromLastJump += Time.deltaTime;
        }
        else if (movement.jump.isJumping)
        {
            movement.jump.isJumping = false;
            movement.jump.timeFromLastJump = 0f;
        }
    }

    // Lateral Movement Subsystem
    void UpdateVelocity()
    {
        // Calculate scaling vectors for x and y based on rotation
        // TODO Clean this up
        float radRotation = movement.position.rotation * Mathf.Deg2Rad;

        Vector2 scaled = new Vector2(Mathf.Cos(radRotation), Mathf.Sin(radRotation)).normalized;

        // Adjust velocity for player movement input
        if (movement.grounded.isGrounded) {
            movement.velocity.x = input.horizontalMovementInput * movement.properties.movementAcceleration;
            movement.velocity.y = input.verticalMovementInput * movement.properties.movementAcceleration;

            // Clamp movement speed to max
            movement.velocity.x = Mathf.Clamp(movement.velocity.x, -movement.properties.maxXSpeed * scaled.x, movement.properties.maxXSpeed * scaled.x);
            movement.velocity.y = Mathf.Clamp(movement.velocity.y, -movement.properties.maxYSpeed * scaled.y, movement.properties.maxYSpeed * scaled.y);

            if (input.horizontalMovementInput == 0f) {
                movement.velocity.x = 0f;
                movement.velocity.y = 0f;
            }

            float sprintMultiplier = input.isSprintKeyPressed ? movement.properties.maxSpeedSprintMultiplier : 1f;

            movement.velocity.x *= sprintMultiplier;
            movement.velocity.y *= sprintMultiplier;
        }
        else if (movement.jump.isJumping)
        {
            movement.velocity.x += movement.jump.x;
            movement.velocity.y += movement.jump.y;
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
        

    }

    void AddGravity()
    {
        if (!movement.grounded.isGrounded) {
            movement.velocity.y -= movement.properties.gravity;
        }
    }


    void SetHeightAboveGround()
    {
        if (movement.grounded.isGrounded) {
            RaycastHit2D hit = GetGroundRaycastHitBelow();

            movement.position.x = hit.point.x + (hit.normal.x * movement.properties.groundedHeight);
            movement.position.y = hit.point.y + (hit.normal.y * movement.properties.groundedHeight);
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
