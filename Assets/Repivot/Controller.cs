using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller : MonoBehaviour
{
    BoxCollider2D boxCollider;

    public float moveSpeed = 5f;
    public float gravity = 20f;
    public float maxRotationDegrees = 0.25f;
    public float groundedHeight = 2f;
    
    public LayerMask mask;

    public Vector2 velocity;
    public bool grounded;
    public RaycastOrigins raycastOrigins;

    //public RaycastHit2D leftHit, rightHit, belowHit;
    public struct RaycastOrigins {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight, center;
    }

    void Start() {
        boxCollider = GetComponent<BoxCollider2D>();
        grounded = false;
    }

    void Update() {

        UpdateRaycastOrigins();

        SetIsGrounded();

        // Horizontal input
        velocity.x = Input.GetAxisRaw("Horizontal") * moveSpeed;

        if (!grounded) {
            velocity.y += gravity * Time.deltaTime;
        } else {
            //velocity.y 
        }

        Vector2 adjVelo = velocity * Time.deltaTime;

        Ray2D leftRay = new Ray2D(raycastOrigins.bottomLeft + adjVelo, -transform.up);
        Ray2D rightRay = new Ray2D(raycastOrigins.bottomRight + adjVelo, -transform.up);

        RaycastHit2D leftHit = Physics2D.Raycast(leftRay.origin, leftRay.direction, 100f, mask);
        RaycastHit2D rightHit = Physics2D.Raycast(rightRay.origin, rightRay.direction, 100f, mask);

        Debug.DrawRay(leftRay.origin, leftRay.direction, Color.yellow);
        Debug.DrawRay(rightRay.origin, rightRay.direction, Color.red);

        if (leftHit && rightHit) {
            Vector2 avgPoint = (leftHit.point + rightHit.point) / 2;
            Vector2 avgNorm = (leftHit.normal + rightHit.normal) / 2;

            Debug.DrawRay(leftHit.point, leftHit.normal, Color.green);
            Debug.DrawRay(rightHit.point, rightHit.normal, Color.magenta);

            Debug.DrawRay(avgPoint, avgNorm, Color.cyan);

            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, avgNorm);
            Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationDegrees);

            transform.rotation = Quaternion.Euler(0f, 0f, finalRotation.eulerAngles.z);

            //transform.position = avgPoint + avgNorm;
        }

        // Apply movement
        transform.Translate(adjVelo * Time.deltaTime);
    }

	void UpdateRaycastOrigins() {
		Bounds bounds = boxCollider.bounds;

		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
        raycastOrigins.center = bounds.center;
	}

    void SetIsGrounded() {
        RaycastHit2D groundedHit = Physics2D.Raycast(raycastOrigins.center, -transform.up, groundedHeight, mask);

        grounded =  groundedHit.collider != null;
    }

}
