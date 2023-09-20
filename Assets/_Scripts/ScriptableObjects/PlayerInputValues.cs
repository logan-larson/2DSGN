using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInputValues", menuName = "Player/PlayerInputValues", order = 2)]
public class PlayerInputValues : ScriptableObject
{
    /// <summary>
    /// Horizontal movement input (Left stick, A and D keys)
    /// </summary>
    public float HorizontalMovementInput = 0f;

    /// <summary>
    /// Vertical movement input (Left stick, W and S keys)
    /// Not used yet
    /// </summary>
    public float VerticalMovementInput = 0f;

    /// <summary>
    /// Jump input (W Key, Space bar, A button)
    /// </summary>
    public bool IsJumpKeyPressed = false;

    /// <summary>
    /// Slide input (Left Control Key, Y button)
    /// </summary>
    public bool IsSlideKeyPressed = false;

    /// <summary>
    /// Sprint input (Left shift, B button)
    /// </summary>
    public bool IsSprintKeyPressed = false;

    /// <summary>
    /// Aim input (Right stick, mouse)
    /// </summary>
    public Vector2 AimInput = Vector2.zero;

    /// <summary>
    /// Fire input (Right trigger, Left mouse button)
    /// </summary>
    public bool IsFirePressed = false;

    /// <summary>
    /// Is gamepad
    /// </summary>
    public bool IsGamepad = false;


    // Testing purposes only
    public bool IsResetKeyPressed;
}
