using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public float skinWidth = 0.015f;

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    public float horizontalRaySpacing;
    public float verticalRaySpacing;

    public RaycastOrigins raycastOrigins;

    public struct RaycastOrigins {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }
}
