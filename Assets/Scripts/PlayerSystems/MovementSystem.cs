using UnityEngine;

[RequireComponent (typeof(PlayerInput))]
[RequireComponent (typeof(PlayerVelocity))]
[RequireComponent (typeof(PlayerMovementProperties))]
[RequireComponent (typeof(PlayerGrounded))]
public class MovementSystem : MonoBehaviour {

    PlayerInput input;
    PlayerVelocity velocity;
    PlayerMovementProperties movementProperties;
    PlayerGrounded grounded;

    RaycastOrigins raycastOrigins;

    public struct RaycastOrigins {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    public void OnStart() {
        input = GetComponent<PlayerInput>();
        velocity = GetComponent<PlayerVelocity>();
        movementProperties = GetComponent<PlayerMovementProperties>();
        grounded = GetComponent<PlayerGrounded>();
    }

    public void OnUpdate() {
        UpdateRaycastOrigins();

        RaycastHit2D groundedHit = GetGroundedHit();
        grounded.isGrounded = groundedHit.collider != null;

        // Horizontal input
        velocity.x = Input.GetAxisRaw("Horizontal") * movementProperties.moveSpeed;

        if (grounded) {
            float distFromGround = groundedHit.distance;
            if (distFromGround < 0.75f) {
                velocity.y += 0.25f; 
            } else if (distFromGround > 1.25f) {
                velocity.y -= 0.25f; 
            } else {
                velocity.y = 0f;
            }
        } else {
            velocity.y -= movementProperties.gravity * Time.deltaTime;
        }

        Vector2 adjVelo = new Vector2(velocity.x, velocity.y) * Time.deltaTime;

        Ray2D leftRay = new Ray2D(raycastOrigins.bottomLeft + adjVelo, -transform.up);
        Ray2D rightRay = new Ray2D(raycastOrigins.bottomRight + adjVelo, -transform.up);

        RaycastHit2D leftHit = Physics2D.Raycast(leftRay.origin, leftRay.direction, 100f, grounded.mask);
        RaycastHit2D rightHit = Physics2D.Raycast(rightRay.origin, rightRay.direction, 100f, grounded.mask);

        Debug.DrawRay(leftRay.origin, leftRay.direction, Color.yellow);
        Debug.DrawRay(rightRay.origin, rightRay.direction, Color.red);

        if (leftHit && rightHit) {
            Vector2 avgPoint = (leftHit.point + rightHit.point) / 2;
            Vector2 avgNorm = (leftHit.normal + rightHit.normal) / 2;

            Debug.DrawRay(leftHit.point, leftHit.normal, Color.green);
            Debug.DrawRay(rightHit.point, rightHit.normal, Color.magenta);

            Debug.DrawRay(avgPoint, avgNorm, Color.cyan);

            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, avgNorm);
            Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, movementProperties.maxRotationDegrees);

            transform.rotation = Quaternion.Euler(0f, 0f, finalRotation.eulerAngles.z);
        }

        // Apply movement
        transform.Translate(adjVelo);

    }

    void UpdateRaycastOrigins() {
		raycastOrigins.bottomLeft = transform.position - (transform.right / 2) - (transform.up / 2);
		raycastOrigins.bottomRight = transform.position + (transform.right / 2) - (transform.up / 2);
		raycastOrigins.topLeft = transform.position - (transform.right / 2) + (transform.up / 2);
		raycastOrigins.topRight = transform.position + (transform.right / 2) + (transform.up / 2);
    }

    RaycastHit2D GetGroundedHit() {
        return Physics2D.Raycast(transform.position, -transform.up, movementProperties.groundedHeight, grounded.mask);
    }
}
