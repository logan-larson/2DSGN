using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {

        [Header("Linked Scripts")]
        public PlayerInputManager inputManager;

        [Header("Gameplay Variables")]
        public float movementSpeed = 5f;
        public float gravity = 5f;

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
            //movementVector.y += inputManager.GetKeyboardInput().y * movementSpeed;

            //movementVector.y -= AddGravity();

            transform.position += new Vector3(movementVector.x, movementVector.y, 0) * Time.deltaTime;
        }

        float AddGravity()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, 2f, 6);

            if (hit.collider != null)
            {
                return 0;
            }
            else
            {
                return gravity;
            }

        }
    }

}
