using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    CameraMovementSystem movementSystem;
    
    void Start()
    {
        // Get scripts attached to camera
        movementSystem = gameObject.GetComponent<CameraMovementSystem>();

        // Call initialize functions on systems
        movementSystem.OnStart();
        
    }

    void Update()
    {
        movementSystem.OnUpdate();
    }
}
