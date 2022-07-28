using UnityEngine;

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

    void Start()
    {
    }

    void Update()
    {
        inputSystem.OnUpdate();
        movementSystem.OnUpdate();
        animationSystem.OnUpdate();
    }
}
