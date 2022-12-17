using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementProperties", menuName = "Settings/PlayerMovementProperties", order = 1)]
public class PlayerMovementProperties : ScriptableObject
{
    [Header("Movement")]
    /// <summary>
    /// The amount by which the player's speed is changed when they move.
    /// </summary>
    public float Acceleration = 2f;
    /// <summary>
    /// The amount by which the player's speed is changed when they aren't moving.
    /// </summary>
    public float Friction = 1f;
    /// <summary>
    /// The maximum speed at which the player can move.
    /// </summary>
    public float MaxSpeed = 7f;
    /// <summary>
    /// The amount by which the player's speed is changed when they sprint.
    /// </summary>
    public float SprintMultiplier = 1.5f; 
    /// <summary>
    /// The worlds gravity
    /// TODO: test with 9.81f and higher jump velocity.
    /// </summary>
    public float Gravity = 5f;
    /// <summary>
    /// The maximum angle the player can rotate at.
    /// </summary>
    public float MaxRotationDegrees = 1f;

    [Header("Grounded")]
    /// <summary>
    /// The height above the ground at which the player is considered to be grounded.
    /// </summary>
    public float GroundedHeight = 1.5f;
    /// <summary>
    /// Layer mask for obstacles.
    /// </summary>
    public LayerMask ObstacleMask;
    /// <summary>
    /// The length of the ray used to override player angle checks.
    /// </summary>
    public float OverrideRayLength = 0.25f;
    
}
