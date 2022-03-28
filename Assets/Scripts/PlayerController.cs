using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Linked Scripts")]
    public PlayerInputManager inputManager;

    [Header("Gameplay Variables")]
    public float movementSpeed = 5f;

    [SerializeField]
    Vector2 movementVector;

    void Start()
    {
        movementVector = new Vector2();
    }

    void FixedUpdate()
    {
        inputManager.GetInputs();

        movementVector = new Vector2();

        movementVector.x += inputManager.GetKeyboardInput().x * movementSpeed;
        movementVector.y += inputManager.GetKeyboardInput().y * movementSpeed;

        transform.position += new Vector3(movementVector.x, movementVector.y, 0);
    }
}
