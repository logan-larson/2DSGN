using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : MonoBehaviour
{

    PlayerInputSystem inputSystem;
    PlayerMovementSystem movementSystem;
    PlayerAnimationSystem animationSystem;

    void Start()
    {
        // Get scripts attached to player
        inputSystem = gameObject.GetComponent<PlayerInputSystem>();
        movementSystem = gameObject.GetComponent<PlayerMovementSystem>();
        animationSystem = gameObject.GetComponent<PlayerAnimationSystem>();

        // Call initialize functions on systems
        inputSystem.OnStart();
        movementSystem.OnStart();
        animationSystem.OnStart();
    }

    void Update()
    {
        // Call update functions
        inputSystem.OnUpdate();
        movementSystem.OnUpdate();
        animationSystem.OnUpdate();
    }
}
