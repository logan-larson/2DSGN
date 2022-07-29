using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
[RequireComponent (typeof (PlayerCollision))]
[RequireComponent (typeof (PlayerVelocity))]
public class CollisionSystem : MonoBehaviour
{

    BoxCollider2D boxCollider;
    PlayerCollision collision;
    PlayerVelocity velocity;

    /** SEBLAGUE CODE **/

	public void OnAwake() {
        
		boxCollider = GetComponent<BoxCollider2D>();
        collision = GetComponent<PlayerCollision>();

		CalculateRaySpacing ();
	}

	public void Move(Vector3 velocity) {
		UpdateRaycastOrigins ();
        ResetCollisionInfo();

		if (velocity.x != 0) {
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}

		transform.Translate (velocity);
	}

    void ResetCollisionInfo() {
		collision.collisionInfo.above = false;
        collision.collisionInfo.below = false;
		collision.collisionInfo.left = false;
        collision.collisionInfo.right = false;
    }

	void HorizontalCollisions(ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + collision.skinWidth;
		
		for (int i = 0; i < collision.horizontalRayCount; i ++) {
			Vector2 rayOrigin = (directionX == -1) ?
                collision.raycastOrigins.bottomLeft :
                collision.raycastOrigins.bottomRight;

			rayOrigin += Vector2.up * (collision.horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collision.mask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.red);

			if (hit) {
				velocity.x = (hit.distance - collision.skinWidth) * directionX;
				rayLength = hit.distance;

				collision.collisionInfo.left = directionX == -1;
				collision.collisionInfo.right = directionX == 1;
			}
		}
	}

	void VerticalCollisions(ref Vector3 velocity) {
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + collision.skinWidth;

		for (int i = 0; i < collision.verticalRayCount; i ++) {
			Vector2 rayOrigin = (directionY == -1) ?
            collision.raycastOrigins.bottomLeft :
            collision.raycastOrigins.topLeft;

			rayOrigin += Vector2.right * (collision.verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collision.mask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength,Color.red);

			if (hit) {
				velocity.y = (hit.distance - collision.skinWidth) * directionY;
				rayLength = hit.distance;

				collision.collisionInfo.below = directionY == -1;
				collision.collisionInfo.above = directionY == 1;
			}
		}
	}

	void UpdateRaycastOrigins() {
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (collision.skinWidth * -2);

		collision.raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		collision.raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		collision.raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		collision.raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	void CalculateRaySpacing() {
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (collision.skinWidth * -2);

		collision.horizontalRayCount = Mathf.Clamp (collision.horizontalRayCount, 2, int.MaxValue);
		collision.verticalRayCount = Mathf.Clamp (collision.verticalRayCount, 2, int.MaxValue);

		collision.horizontalRaySpacing = bounds.size.y / (collision.horizontalRayCount - 1);
		collision.verticalRaySpacing = bounds.size.x / (collision.verticalRayCount - 1);
	}



















    /** MY CODE **/

    public void OnAwakeME() {
        boxCollider = GetComponent<BoxCollider2D>();
        collision = GetComponent<PlayerCollision>();
        velocity = GetComponent<PlayerVelocity>();

        InitializeCollision();

        CalculateRaySpacing();
    }

    void InitializeCollision() {
        //collision.skinWidth = 0.015f;

        collision.horizontalRayCount = 4;
        collision.verticalRayCount = 4;
    }

    void CalculateRaySpacingME() {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(collision.skinWidth * -2);

        collision.horizontalRayCount = Mathf.Clamp(collision.horizontalRayCount, 2, int.MaxValue);
        collision.verticalRayCount = Mathf.Clamp(collision.verticalRayCount, 2, int.MaxValue);

        collision.horizontalRaySpacing = bounds.size.y / (collision.horizontalRayCount - 1);
        collision.verticalRaySpacing = bounds.size.x / (collision.verticalRayCount - 1);
    }

    public void UpdateRaycastOriginsME() {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(collision.skinWidth * -2);

        collision.raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        collision.raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        collision.raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        collision.raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

	public void HorizontalCollisions() {
		float directionX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + collision.skinWidth;
		
		for (int i = 0; i < collision.horizontalRayCount; i ++) {
			Vector2 rayOrigin = (directionX == -1) ? collision.raycastOrigins.bottomLeft : collision.raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (collision.horizontalRaySpacing * i);

			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collision.mask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.red);

			if (hit) {
				//velocity.x = (hit.distance - collision.skinWidth) * directionX;
				rayLength = hit.distance;

				//collision.collisions.left = directionX == -1;
				//collision.collisions.right = directionX == 1;
			}
		}
	}

    public float VerticalCollisions() {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + collision.skinWidth;

        for (int i = 0; i < collision.verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? collision.raycastOrigins.bottomLeft : collision.raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (collision.verticalRaySpacing * i + velocity.x);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collision.mask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit) {
                //velocity.y = (hit.distance - collision.skinWidth) * directionY;
                rayLength = hit.distance;

                //collision.collisions.below = directionY == -1;
                //collision.collisions.above = directionY == 1;
            }
        }

        return rayLength;
    }

}
