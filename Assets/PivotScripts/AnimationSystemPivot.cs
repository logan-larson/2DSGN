using UnityEngine;

public class AnimationSystemPivot : MonoBehaviour {

    PlayerVelocity velocity;
    SpriteRenderer spriteRenderer;

    public void OnAwake() {
        velocity = gameObject.GetComponent<PlayerVelocity>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public void OnUpdate() {
        UpdateDirection();
    }

    void UpdateDirection() {
        spriteRenderer.flipX = velocity.x < 0;
        //spriteRenderer.flipX = true;
    }
}
