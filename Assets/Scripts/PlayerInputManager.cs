using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerInputManager : MonoBehaviour
    {

        private Vector2 keyboardInput;
        private Vector2 mouseInput;


        public void GetInputs()
        {
            keyboardInput.x = Input.GetAxisRaw("Horizontal");
            keyboardInput.y = Input.GetAxisRaw("Vertical");

            mouseInput.x = Input.GetAxisRaw("Mouse X");
            mouseInput.y = Input.GetAxisRaw("Mouse Y");
        }

        public Vector2 GetKeyboardInput()
        {
            return keyboardInput;
        }

        public Vector2 GetMouseInput()
        {
            return mouseInput;
        }
    }
}

