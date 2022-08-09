using UnityEngine;

[RequireComponent (typeof (InputSystemPivot))]
[RequireComponent (typeof (MovementSystemPivot))]
[RequireComponent (typeof (AnimationSystemPivot))]
public class PlayerSystemPivot : MonoBehaviour
{
    InputSystemPivot inputSystem;
    MovementSystemPivot movementSystem;
    AnimationSystemPivot animationSystem;

    void Awake()
    {
        // Get attached scripts
        inputSystem = GetComponent<InputSystemPivot>();
        movementSystem = GetComponent<MovementSystemPivot>();
        animationSystem = GetComponent<AnimationSystemPivot>();

        // Call OnAwake methods in scripts
        inputSystem.OnAwake();
        movementSystem.OnAwake();
        animationSystem.OnAwake();
    }

    void Update()
    {
        inputSystem.OnUpdate();
        movementSystem.OnUpdate();
        animationSystem.OnUpdate();
    }
}
