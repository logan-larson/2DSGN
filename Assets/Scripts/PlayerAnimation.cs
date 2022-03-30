using UnityEngine;

namespace Player {

    public class PlayerAnimation : MonoBehaviour
    {

        [Header("Linked Scripts")]
        public PlayerInputManager inputManager;
        public Animator animator;

        void FixedUpdate()
        {
            if (inputManager.GetKeyboardInput().x < 0) {
                animator.SetFloat("Speed", 1f);
                transform.rotation = Quaternion.Euler(0, 180, -90);
            } else if (inputManager.GetKeyboardInput().x > 0) {
                animator.SetFloat("Speed", 1f);
                transform.rotation = Quaternion.Euler(0, 0, -90);
            } else {
                animator.SetFloat("Speed", 0f);
            }
            
        }
    }

}
