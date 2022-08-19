using UnityEngine;

[RequireComponent (typeof(PlayerVelocity))]
[RequireComponent (typeof(PlayerAnimation))]
public class AnimationSystem : MonoBehaviour {

    PlayerVelocity velocity;
    PlayerAnimation playerAnimation;

    public void OnStart() {
        velocity = GetComponent<PlayerVelocity>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    public void OnUpdate() {
        if (velocity.x < 0f) {
            playerAnimation.isFacingLeft = true;
        } else if (velocity.x > 0f) {
            playerAnimation.isFacingLeft = false;
        }
            
        if (playerAnimation.isFacingLeft == true) {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        } else {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
