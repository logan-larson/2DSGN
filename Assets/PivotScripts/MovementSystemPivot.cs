using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSystemPivot : MonoBehaviour
{

    // Define scripts
    PlayerPosition position;
    PlayerVelocity velocity;
    PlayerMovementProperties movementProperties;
    PlayerGrounded grounded;

    public void OnAwake()
    {
        // Get attached scripts
        position = gameObject.GetComponent<PlayerPosition>();
        velocity = gameObject.GetComponent<PlayerVelocity>();
        movementProperties = gameObject.GetComponent<PlayerMovementProperties>();
        grounded = gameObject.GetComponent<PlayerGrounded>();
    }

    public void OnUpdate()
    {

        
    }
}
