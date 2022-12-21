using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof(PlayerInputValues))]
[RequireComponent (typeof(MovementSystem))]
public class InputSystem : MonoBehaviour {


    // TODO: At some point I think I would like to make these events
    // So other systems can subscribe to them without having
    // to know about the input system

    [HideInInspector]
    public PlayerInputValues InputValues;

    private MovementSystem _movement;

    private void Awake() {
        _movement = _movement ?? GetComponent<MovementSystem>();

        InputValues = (PlayerInputValues) ScriptableObject.CreateInstance(typeof(PlayerInputValues));
    }

    public void OnMove(InputValue value)
    {
        InputValues.HorizontalMovementInput = value.Get<Vector2>().x;
    }

    public void OnSprint(InputValue value) {
        InputValues.IsSprintKeyPressed = value.Get<float>() == 1f;
    }
    
    // TODO: Move these events to triggers
    public void OnJump(InputValue value) {
        InputValues.IsJumpKeyPressed = value.Get<float>() == 1f;
    }

    public void OnFire(InputValue value) {
        InputValues.IsFirePressed = value.Get<float>() == 1f;
        //movement.Fire();
    }

}
