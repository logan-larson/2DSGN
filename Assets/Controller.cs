using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{

    

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();

    }

}
