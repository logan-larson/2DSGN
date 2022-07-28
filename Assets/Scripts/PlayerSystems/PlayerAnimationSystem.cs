using UnityEngine;

public class PlayerAnimationSystem : MonoBehaviour
{
    GameObject playerAnimator;
    Animator animator;
    PlayerInput input;
    PlayerPosition position;

    public void OnStart()
    {
        // Setup scripts
        playerAnimator = GameObject.Find("PlayerAnimator");
        if (playerAnimator != null)
        {
            animator = playerAnimator.GetComponent<Animator>();
        }
        input = gameObject.GetComponent<PlayerInput>();
        position = gameObject.GetComponent<PlayerPosition>();
    }

    public void OnUpdate()
    {
        SetDirection();

        SetSpeed();
    }

    void SetDirection()
    {
        /*
        if (input.horizontalMovementInput < 0) {
            playerAnimator.transform.rotation = Quaternion.Euler(0, 180, -position.rotation - 90);
        } else if (input.horizontalMovementInput > 0) {
            playerAnimator.transform.rotation = Quaternion.Euler(0, 0, position.rotation - 90);
        }
        */
    }

    void SetSpeed()
    {
        if (input.horizontalMovementInput == 0)
        {
            animator.SetFloat("Speed", 0f);
        }
        else
        {
            animator.SetFloat("Speed", 1f);
        }
        
        animator.speed = input.isSprintKeyPressed ? 2f : 1f;
    }


}
