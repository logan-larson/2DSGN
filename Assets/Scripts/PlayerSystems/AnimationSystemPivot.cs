using UnityEngine;

[RequireComponent (typeof (PlayerVelocity))]
[RequireComponent (typeof (SpriteRenderer))]
public class AnimationSystemPivot : MonoBehaviour {

    PlayerVelocity velocity;
    SpriteRenderer spriteRenderer;

    public void OnAwake() {
        velocity = GetComponent<PlayerVelocity>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnUpdate() {
        UpdateDirection();
    }

    void UpdateDirection() {
        spriteRenderer.flipX = velocity.x < 0;
        //spriteRenderer.flipX = true;
    }
}
