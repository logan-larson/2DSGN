using UnityEngine;

[RequireComponent (typeof(PlayerInput))]
[RequireComponent (typeof(PlayerPosition))]
[RequireComponent (typeof(PlayerVelocity))]
[RequireComponent (typeof(PlayerMovementProperties))]
[RequireComponent (typeof(PlayerGrounded))]
[RequireComponent (typeof(PlayerMode))]
public class MovementSystem : MonoBehaviour {

    PlayerInput input;
    PlayerPosition position;
    PlayerVelocity velocity;
    PlayerMovementProperties movementProperties;
    PlayerGrounded grounded;
    PlayerMode mode;

    RaycastOrigins raycastOrigins;

    public struct RaycastOrigins {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    bool jumpNow = false;

    Vector3 predictPos = new Vector3();
    Vector3 predictNorm = new Vector3();

    bool recalculateLanding = false;
    public float fFactor = 6.75f;
    public float minJumpTime = 1f;

    public void OnStart() {
        input = GetComponent<PlayerInput>();
        position = GetComponent<PlayerPosition>();
        velocity = GetComponent<PlayerVelocity>();
        movementProperties = GetComponent<PlayerMovementProperties>();
        grounded = GetComponent<PlayerGrounded>();
        mode = GetComponent<PlayerMode>();
    }

    public void OnUpdate() {

        UpdateMode();

        UpdateRaycastOrigins();

        if (movementProperties.timeSinceGrounded > minJumpTime)  {
            UpdateGrounded();
        } else {
            movementProperties.timeSinceGrounded += Time.fixedDeltaTime;
        }

        UpdateVelocity();
            

        // This is a velocity relative to the player not the world
        Vector2 adjVelo = new Vector2(velocity.x, velocity.y) * Time.fixedDeltaTime;

        // If grounded, match body to ground
        if (grounded.isGrounded) {
            //movementProperties.maxRotationDegrees = 2f;

            ShowPredictedTrajectory();

            Ray2D leftRay = new Ray2D(raycastOrigins.bottomLeft + adjVelo, -transform.up);
            Ray2D rightRay = new Ray2D(raycastOrigins.bottomRight + adjVelo, -transform.up);

            RaycastHit2D leftHit = Physics2D.Raycast(leftRay.origin, leftRay.direction, movementProperties.groundedHeight, grounded.mask);
            RaycastHit2D rightHit = Physics2D.Raycast(rightRay.origin, rightRay.direction, movementProperties.groundedHeight, grounded.mask);

            // Use override hit to prevent clipping
            RaycastHit2D overrideHit = Physics2D.Raycast((leftRay.origin + rightRay.origin) / 2, transform.right, movementProperties.overrideRayLength * velocity.x, grounded.mask);
            if (velocity.x < 0f && overrideHit.collider != null) {
                leftHit = overrideHit;
            } else if (velocity.x > 0f && overrideHit.collider != null) {
                rightHit = overrideHit;
            }

            // Apply rotation to orient body to match ground
            if (leftHit && rightHit) {
                Vector2 avgPoint = (leftHit.point + rightHit.point) / 2;
                Vector2 avgNorm = (leftHit.normal + rightHit.normal) / 2;

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, avgNorm);
                Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, movementProperties.maxRotationDegrees);

                transform.rotation = Quaternion.Euler(0f, 0f, finalRotation.eulerAngles.z);
                position.rotation = finalRotation.eulerAngles.z;
            }

            transform.Translate(adjVelo);
            position.x = transform.position.x;
            position.y = transform.position.y;
        } else { 
            //movementProperties.maxRotationDegrees = 8f;
            // We are in the air, so move according to worldspace not self
            transform.position += new Vector3(velocity.veloOffGround.x, velocity.veloOffGround.y, 0f) * Time.fixedDeltaTime;

            position.x = transform.position.x;
            position.y = transform.position.y;

            // And rotate to the predicted landing spots normal
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, predictNorm);
            Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, movementProperties.maxRotationDegrees);

            transform.rotation = Quaternion.Euler(0f, 0f, finalRotation.eulerAngles.z);
            position.rotation = finalRotation.eulerAngles.z;
        }
    }

    void UpdateMode() {
        if (input.isSprintKeyPressed) {
            mode.inCombatMode = false;
            mode.inParkourMode = true;
        }
    }

    void UpdateRaycastOrigins() {
		raycastOrigins.bottomLeft = transform.position - (transform.right / 2) - (transform.up / 2);
		raycastOrigins.bottomRight = transform.position + (transform.right / 2) - (transform.up / 2);
		raycastOrigins.topLeft = transform.position - (transform.right / 2) + (transform.up / 2);
		raycastOrigins.topRight = transform.position + (transform.right / 2) + (transform.up / 2);
    }

    void UpdateGrounded() {
        if (grounded.isGrounded) {
            RaycastHit2D groundedHit = Physics2D.Raycast(transform.position, -transform.up, movementProperties.groundedHeight * 1.5f, grounded.mask);
            grounded.isGrounded = groundedHit.collider != null;
            grounded.groundDistance = groundedHit.distance;
        } else {
            RaycastHit2D groundedHit = Physics2D.Raycast(transform.position, -transform.up, movementProperties.groundedHeight, grounded.mask);
            grounded.isGrounded = groundedHit.collider != null;
            grounded.groundDistance = groundedHit.distance;
        }
    }

    public void Jump() {
        if (grounded.isGrounded) {
            jumpNow = true;
        }
    }

    void UpdateVelocity() {
        // Add velo based on horizontal input, accelerate faster when sprint key is pressed
        if (grounded.isGrounded) {
            if (input.isSprintKeyPressed) {
                velocity.x += input.horizontalMovementInput * (movementProperties.acceleration * 2f);
            } else {
                velocity.x += input.horizontalMovementInput * movementProperties.acceleration;
            }

            predictPos = new Vector3();
            predictNorm = new Vector3();
        }

        // Apply friction when no input is held
        if (grounded.isGrounded && input.horizontalMovementInput == 0f) {
            if (Mathf.Abs(velocity.x - movementProperties.friction) < 0.5f) {
                velocity.x = 0f;
            } else {
                if (velocity.x > 0f) {
                    velocity.x = Mathf.Clamp(velocity.x, 0f, velocity.x - movementProperties.friction);
                } else {
                    velocity.x = Mathf.Clamp(velocity.x, velocity.x + movementProperties.friction, 0f);
                }
            }
        }

        // Set height above ground
        if (grounded.isGrounded) {
            if (jumpNow) {
                jumpNow = false;
                grounded.isGrounded = false;
                movementProperties.timeSinceGrounded = 0f;
                velocity.y = movementProperties.jumpVelocity;
            } else { // This needs a lot of work to make player landings smoother
                if (grounded.groundDistance < 0.75f) {
                    velocity.y += 0.25f; 
                } else if (grounded.groundDistance > 1.25f) {
                    velocity.y -= 0.25f; 
                } else {
                    velocity.y = 0f;
                }
            }

            velocity.veloOffGround = new Vector2(0f, 0f);
        } else {
            // Add gravity in downward direction relative to worldspace
            velocity.veloOffGround += (Vector2.down * movementProperties.gravity * Time.fixedDeltaTime);

            // This is where airborne movement forces can be applied

            // If forces were applied then we need to recalculate the landing
            recalculateLanding = false;
        }

        // Set sprint multiplier
        if (input.isSprintKeyPressed && mode.inParkourMode) {
            movementProperties.sprintMultiplier += movementProperties.acceleration * 2f;
            movementProperties.sprintMultiplier = Mathf.Clamp(movementProperties.sprintMultiplier, 0f, movementProperties.maxSprintSpeed);
        } else {
            movementProperties.sprintMultiplier -= movementProperties.friction;
            movementProperties.sprintMultiplier = Mathf.Clamp(movementProperties.sprintMultiplier, 0f, movementProperties.maxSprintSpeed);
        }

        // Limit top speed
        velocity.x = Mathf.Clamp(velocity.x, -movementProperties.maxXSpeed - movementProperties.sprintMultiplier, movementProperties.maxXSpeed + movementProperties.sprintMultiplier);

        // This is triggered directly after the jump is initialized
        if (!grounded.isGrounded && movementProperties.timeSinceGrounded == 0f) {
            // Get the velocity directly after the jump because we don't allow
            // airborne movement controls besides using a weapon to move

            /*
            Theta = yAngleFromRight
            X1 = xComponentOfYVelo
            Y1 = yComponentOfYVelo

            Gamma = xAngleFromRight
            X2 = xComponentOfXVelo
            Y2 = yComponentOfXVelo 
            */

            float theta = Vector2.SignedAngle(Vector2.right, transform.up) * Mathf.Deg2Rad;
            float gamma = Vector2.SignedAngle(Vector2.right, transform.right) * Mathf.Deg2Rad;

            float xComponentOfYVelo = Mathf.Cos(theta) * velocity.y;
            float yComponentOfYVelo = Mathf.Sin(theta) * velocity.y;

            float xComponentOfXVelo = Mathf.Cos(gamma) * velocity.x;
            float yComponentOfXVelo = Mathf.Sin(gamma) * velocity.x;

            velocity.veloOffGround = new Vector2(xComponentOfXVelo + xComponentOfYVelo, yComponentOfXVelo + yComponentOfYVelo);

            // Reset ground velocities
            velocity.x = 0f;
            velocity.y = 0f;

            Debug.Log("Initial velo off ground: (" + velocity.veloOffGround.x + ", " + velocity.veloOffGround.y + ")");

            RecalculateLandingPos();
        } else if (!grounded.isGrounded && predictNorm == new Vector3() && predictPos == new Vector3()) {
            // Ideally this never gets called because the player should never detach
            // from a surface unless they jump
            RecalculateLandingPos();
        } else if (recalculateLanding) {
            RecalculateLandingPos();
        }
    }

    void RecalculateLandingPos() {
            // Predict the landing spot 
            RaycastHit2D predictHit = new RaycastHit2D();
            RaycastHit2D predictHit2 = new RaycastHit2D();
            Vector2 pos = transform.position - (transform.up * 0.7f);
            Vector2 pos2 = transform.position + (transform.up * 0.7f);

            Vector2 velo = new Vector2(velocity.veloOffGround.x, velocity.veloOffGround.y) * Time.fixedDeltaTime * fFactor;

            int count = 0;
            while ((predictHit.collider == null && predictHit2.collider == null) && count < 100) {

                // Generate new ray
                Ray2D ray = new Ray2D(pos, velo.normalized);
                Ray2D ray2 = new Ray2D(pos2, velo.normalized);

                Color randColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);

                Debug.DrawRay(ray.origin, ray.direction * velo.magnitude, randColor, 2f);
                Debug.DrawRay(ray2.origin, ray2.direction * velo.magnitude, randColor, 2f);

                // Update predictHit
                predictHit = Physics2D.Raycast(ray.origin, ray.direction, velo.magnitude, grounded.mask);
                predictHit2 = Physics2D.Raycast(ray2.origin, ray2.direction, velo.magnitude, grounded.mask);

                // Update position to end of predictHit ray
                pos += (ray.direction * velo.magnitude);
                pos2 += (ray2.direction * velo.magnitude);

                velo += (Vector2.down * movementProperties.gravity * Time.fixedDeltaTime);

                count++;
            }

            // If landing position is spotted
            if (predictHit.collider != null) {
                // Set the predicted landing position
                predictPos = predictHit.point;

                // And the predicted landing normal
                predictNorm = predictHit.normal;
            } else if (predictHit2.collider != null) {
                // Set the predicted landing position
                predictPos = predictHit2.point;

                // And the predicted landing normal
                predictNorm = predictHit2.normal;
            } else {
                Debug.Log("Hit the max count");
            }
    }

    void ShowPredictedTrajectory() {

        //if (velocity.x == 0f) {
            //return;
        //}

        float theta = Vector2.SignedAngle(Vector2.right, transform.up) * Mathf.Deg2Rad;
        float gamma = Vector2.SignedAngle(Vector2.right, transform.right) * Mathf.Deg2Rad;

        float xComponentOfYVelo = Mathf.Cos(theta) * movementProperties.jumpVelocity;
        float yComponentOfYVelo = Mathf.Sin(theta) * movementProperties.jumpVelocity;

        float xComponentOfXVelo = Mathf.Cos(gamma) * velocity.x;
        float yComponentOfXVelo = Mathf.Sin(gamma) * velocity.x;

        Vector2 predVelo = new Vector2(xComponentOfXVelo + xComponentOfYVelo, yComponentOfXVelo + yComponentOfYVelo);

        RaycastHit2D predictHit = new RaycastHit2D();
        Vector2 pos = transform.position;

        Vector2 velo = new Vector2(predVelo.x, predVelo.y) * Time.fixedDeltaTime * fFactor;

        int count = 0;
        while (predictHit.collider == null && count < 100) {

            // Generate new ray
            Ray2D ray = new Ray2D(pos, velo.normalized);

            if (count % 2 == 1) {
                Debug.DrawRay(ray.origin, ray.direction * velo.magnitude, Color.green);
            }

            // Update predictHit
            predictHit = Physics2D.Raycast(ray.origin, ray.direction, velo.magnitude, grounded.mask);

            // Update position to end of predictHit ray
            pos += (ray.direction * velo.magnitude);

            velo += (Vector2.down * movementProperties.gravity * Time.fixedDeltaTime);

            count++;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(predictPos, 0.25f);
    }
}
