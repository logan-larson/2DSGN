using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
[RequireComponent (typeof (PlayerCollision))]
public class CollisionSystem : MonoBehaviour
{

    BoxCollider2D boxCollider;
    PlayerCollision collision;

    public void OnAwake() {
        boxCollider = GetComponent<BoxCollider2D>();
        collision = GetComponent<PlayerCollision>();

        InitializeCollision();

        CalculateRaySpacing();
    }

    void InitializeCollision() {
        collision.skinWidth = 0.015f;

        collision.horizontalRayCount = 4;
        collision.verticalRayCount = 4;
    }

    public void UpdateRaycastOrigins() {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(collision.skinWidth * -2);

        collision.raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        collision.raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        collision.raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        collision.raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing() {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(collision.skinWidth * -2);

        collision.horizontalRayCount = Mathf.Clamp(collision.horizontalRayCount, 2, int.MaxValue);
        collision.verticalRayCount = Mathf.Clamp(collision.verticalRayCount, 2, int.MaxValue);

        collision.horizontalRaySpacing = bounds.size.y / (collision.horizontalRayCount - 1);
        collision.verticalRaySpacing = bounds.size.x / (collision.verticalRayCount - 1);
    }
}
