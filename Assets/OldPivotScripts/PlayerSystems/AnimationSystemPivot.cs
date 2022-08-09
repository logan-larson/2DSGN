using UnityEngine;

[RequireComponent (typeof (PlayerVelocity))]
[RequireComponent (typeof (SpriteRenderer))]
public class AnimationSystemPivot : MonoBehaviour {

    PlayerVelocity velocity;
    PlayerAnimation playerAnimation;
    SpriteRenderer spriteRenderer;

    public void OnAwake() {
        velocity = GetComponent<PlayerVelocity>();
        playerAnimation = GetComponent<PlayerAnimation>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnUpdate() {
        UpdateDirection();
    }

    void UpdateDirection() {
        if (velocity.x < 0f) {
            playerAnimation.isFacingLeft = true;
        } else if (velocity.x > 0f) {
            playerAnimation.isFacingLeft = false;
        }

        if (playerAnimation.isFacingLeft) {
            spriteRenderer.flipX = true;
        } else {
            spriteRenderer.flipX = false;
        }
    }
}
