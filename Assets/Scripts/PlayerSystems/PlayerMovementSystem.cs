using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementSystem : MonoBehaviour
{

    PlayerMovement movementComponent;

    public void OnStart()
    {
        movementComponent = new PlayerMovement();
    }

    public void OnUpdate()
    {
        
    }
}
