using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementProperties", menuName = "Settings/PlayerMovementProperties", order = 1)]
public class PlayerMovementProperties : ScriptableObject
{
    /// <summary>
    /// The amount by which the player's speed is changed when they move.
    /// </summary>
    public float Acceleration = 0.1f;
    /// <summary>
    /// The amount by which the player's speed is changed when they aren't moving.
    /// </summary>
    public float Friction = 0.1f;
    /// <summary>
    /// The maximum speed at which the player can move.
    /// </summary>
    public float MaxSpeed = 5f;
}
