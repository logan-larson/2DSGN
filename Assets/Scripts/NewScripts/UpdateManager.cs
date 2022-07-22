using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    Movement movement;

    void Awake()
    {
        movement = gameObject.GetComponent<Movement>();
    }

    void Update()
    {
        movement.OnUpdate();
    }
}
