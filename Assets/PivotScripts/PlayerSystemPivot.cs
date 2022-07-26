using UnityEngine;

public class PlayerSystemPivot : MonoBehaviour
{
    InputSystemPivot inputSystem;
    MovementSystemPivot movementSystem;

    void Awake()
    {
        // Get attached scripts
        inputSystem = gameObject.GetComponent<InputSystemPivot>();
        movementSystem = gameObject.GetComponent<MovementSystemPivot>();

        // Call OnAwake methods in scripts
        inputSystem.OnAwake();
        movementSystem.OnAwake();
    }

    void Start()
    {
    }

    void Update()
    {
        inputSystem.OnUpdate();
        movementSystem.OnUpdate();
    }
}
