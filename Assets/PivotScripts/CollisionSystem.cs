using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
[RequireComponent (typeof (PlayerCollision))]
[RequireComponent (typeof (PlayerVelocity))]
public class CollisionSystem : MonoBehaviour
{

    BoxCollider2D boxCollider;
    PlayerCollision collision;
    PlayerVelocity velocity;

	public void OnAwake() {
		boxCollider = GetComponent<BoxCollider2D>();
        collision = GetComponent<PlayerCollision>();
        velocity = GetComponent<PlayerVelocity>();

		InitializeCollision();
	}

	void InitializeCollision() {
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (collision.skinWidth * -2);

		collision.horizontalRayCount = Mathf.Clamp (collision.horizontalRayCount, 2, int.MaxValue);
		collision.verticalRayCount = Mathf.Clamp (collision.verticalRayCount, 2, int.MaxValue);

		collision.horizontalRaySpacing = bounds.size.y / (collision.horizontalRayCount - 1);
		collision.verticalRaySpacing = bounds.size.x / (collision.verticalRayCount - 1);
	}

    public void ResetVelocityOnVerticalCollision() {
        // Reset velocity when grounded -- below
        // Reset velocity when hitting head on block -- above
		if (collision.collisionInfo.above || collision.collisionInfo.below) {
			velocity.y = 0;
		}
    }

	public Vector3 GetVelocityFromCollisions() {
        Vector3 newVelocity = new Vector3(velocity.x, velocity.y) * Time.deltaTime;

		if (newVelocity.x != 0) {
			HorizontalCollisions(ref newVelocity);
		}
		if (newVelocity.y != 0) {
			VerticalCollisions(ref newVelocity);
		}

        return newVelocity;
	}

    public void ResetCollisionInfo() {
		collision.collisionInfo.above = false;
        collision.collisionInfo.below = false;
		collision.collisionInfo.left = false;
        collision.collisionInfo.right = false;
    }

	public void HorizontalCollisions(ref Vector3 v) {
		float directionX = Mathf.Sign(v.x);
		float rayLength = Mathf.Abs(v.x) + collision.skinWidth;
		
		for (int i = 0; i < collision.horizontalRayCount; i ++) {
			Vector2 rayOrigin = (directionX == -1) ?
                collision.raycastOrigins.bottomLeft :
                collision.raycastOrigins.bottomRight;

			rayOrigin += Vector2.up * (collision.horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collision.mask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.red);

			if (hit) {
				v.x = (hit.distance - collision.skinWidth) * directionX;
				rayLength = hit.distance;

				collision.collisionInfo.left = directionX == -1;
				collision.collisionInfo.right = directionX == 1;
			}
		}
	}

	public void VerticalCollisions(ref Vector3 v) {
		float directionY = Mathf.Sign(v.y);
		float rayLength = Mathf.Abs(v.y) + collision.skinWidth;

		for (int i = 0; i < collision.verticalRayCount; i ++) {
			Vector2 rayOrigin = (directionY == -1) ?
            collision.raycastOrigins.bottomLeft :
            collision.raycastOrigins.topLeft;

			rayOrigin += Vector2.right * (collision.verticalRaySpacing * i + v.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collision.mask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength,Color.red);

			if (hit) {
				v.y = (hit.distance - collision.skinWidth) * directionY;
				rayLength = hit.distance;

				collision.collisionInfo.below = directionY == -1;
				collision.collisionInfo.above = directionY == 1;
			}
		}
	}

	public void UpdateRaycastOrigins() {
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (collision.skinWidth * -2);

		collision.raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		collision.raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		collision.raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		collision.raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}
}
