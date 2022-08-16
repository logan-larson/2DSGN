using UnityEngine;

[RequireComponent (typeof(PlayerInput))]
[RequireComponent (typeof(PlayerVelocity))]
[RequireComponent (typeof(PlayerMovementProperties))]
[RequireComponent (typeof(PlayerGrounded))]
[RequireComponent (typeof(PlayerMode))]
public class MovementSystem : MonoBehaviour {

    PlayerInput input;
    PlayerVelocity velocity;
    PlayerMovementProperties movementProperties;
    PlayerGrounded grounded;
    PlayerMode mode;

    RaycastOrigins raycastOrigins;

    public struct RaycastOrigins {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    Vector3 predictPos = new Vector3();
    public float step = 0.25f;

    public void OnStart() {
        input = GetComponent<PlayerInput>();
        velocity = GetComponent<PlayerVelocity>();
        movementProperties = GetComponent<PlayerMovementProperties>();
        grounded = GetComponent<PlayerGrounded>();
        mode = GetComponent<PlayerMode>();
    }

    public void OnUpdate() {
        UpdateRaycastOrigins();

        if (movementProperties.timeSinceGrounded > 1f)  {
            UpdateGrounded();
        } else {
            movementProperties.timeSinceGrounded += Time.deltaTime;
        }

        UpdateVelocity();

        // This is a velocity relative to the player not the world
        Vector2 adjVelo = new Vector2(velocity.x, velocity.y) * Time.deltaTime;

        // If grounded, match body to ground
        if (grounded.isGrounded) {
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
            }

            transform.Translate(adjVelo);
        } else { // Otherwise, predict landing location and match body to that
            RaycastHit2D predictHit = new RaycastHit2D();
            Vector2 pos = transform.position;
            Vector2 velo = adjVelo;

            int count = 0;
            Color[] rayColors = new Color[] {
                Color.black, Color.blue, Color.green, Color.magenta, Color.cyan,
                Color.yellow, Color.red, Color.white, Color.gray, Color.green
            };
            while (predictHit.collider == null && count < 10) {

                // Generate new ray
                Ray2D ray = new Ray2D(pos, velo.normalized);

                Debug.DrawRay(ray.origin, ray.direction, rayColors[count]);

                // Update predictHit
                predictHit = Physics2D.Raycast(ray.origin, ray.direction, 1f, grounded.mask);

                velo += Vector2.down * step;

                // Update position to end of predictHit ray
                pos += ray.direction;

                count++;
            }

            if (predictHit.collider != null) {
                predictPos = predictHit.point;

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, predictHit.normal);
                Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, movementProperties.maxRotationDegrees);

                transform.rotation = Quaternion.Euler(0f, 0f, finalRotation.eulerAngles.z);
            }

            transform.position += new Vector3(velocity.veloOffGround.x, velocity.veloOffGround.y, 0f) * Time.deltaTime;
        }

    }

    void UpdateRaycastOrigins() {
		raycastOrigins.bottomLeft = transform.position - (transform.right / 2) - (transform.up / 2);
		raycastOrigins.bottomRight = transform.position + (transform.right / 2) - (transform.up / 2);
		raycastOrigins.topLeft = transform.position - (transform.right / 2) + (transform.up / 2);
		raycastOrigins.topRight = transform.position + (transform.right / 2) + (transform.up / 2);
    }

    void UpdateGrounded() {
        RaycastHit2D groundedHit = Physics2D.Raycast(transform.position, -transform.up, movementProperties.groundedHeight, grounded.mask);
        grounded.isGrounded = groundedHit.collider != null;
        grounded.groundDistance = groundedHit.distance;
    }

    void UpdateVelocity() {
        // Add velo based on horizontal input, accelerate faster when sprint key is pressed
        if (grounded.isGrounded) {
            if (input.isSprintKeyPressed) {
                velocity.x += input.horizontalMovementInput * (movementProperties.acceleration * 2f);
            } else {
                velocity.x += input.horizontalMovementInput * movementProperties.acceleration;
            }
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
            if (input.isJumpKeyPressed) {
                grounded.isGrounded = false;
                movementProperties.timeSinceGrounded = 0f;
                velocity.y = movementProperties.jumpVelocity;
            } else {
                if (grounded.groundDistance < 0.75f) {
                    velocity.y += 0.25f; 
                } else if (grounded.groundDistance > 1.25f) {
                    velocity.y -= 0.25f; 
                } else {
                    velocity.y = 0f;
                }
            }
        } else {
            /*
            // Add gravity in downward direction relative to worldspace
            float angle = Vector3.SignedAngle(Vector3.up, transform.up, Vector3.forward);

            float xMult = Mathf.Sin(angle * Mathf.Deg2Rad) * movementProperties.gravity;
            float yMult = Mathf.Cos(angle * Mathf.Deg2Rad) * movementProperties.gravity;

            velocity.x -= xMult * Time.deltaTime;
            velocity.y -= yMult * Time.deltaTime;
            Vector2 velo = new Vector2(velocity.x, velocity.y);
            velo += Vector2.down * movementProperties.gravity * Time.deltaTime;

            velocity.x = velo.x;
            velocity.y = velo.y;
            */
            //float angle = Vector3.SignedAngle(Vector3.up, transform.up, Vector3.forward);
            velocity.veloOffGround += (Vector2.down * movementProperties.gravity * Time.deltaTime);
            //velocity.x = velocity.veloOffGround.x * Mathf.Cos(velocity.angleOffGround * Mathf.Deg2Rad);
            //velocity.y = velocity.veloOffGround.y * Mathf.Sin(velocity.angleOffGround * Mathf.Deg2Rad);
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

        if (!grounded.isGrounded && movementProperties.timeSinceGrounded == 0f) {
            velocity.veloOffGround = new Vector2(velocity.x, velocity.y);
            //velocity.angleOffGround = transform.rotation.eulerAngles.z + 90f;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(predictPos, 0.25f);
    }
}
