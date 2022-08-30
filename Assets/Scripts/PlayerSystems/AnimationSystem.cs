using UnityEngine;

[RequireComponent (typeof(PlayerVelocity))]
[RequireComponent (typeof(PlayerAnimation))]
public class AnimationSystem : MonoBehaviour {

    PlayerPosition position;
    PlayerVelocity velocity;
    PlayerAnimation playerAnimation;

    // Child game objects
    GameObject staticAnimations;

    public float lerpValue = 0.1f;

    public void OnStart() {
        position = GetComponent<PlayerPosition>();
        velocity = GetComponent<PlayerVelocity>();
        playerAnimation = GetComponent<PlayerAnimation>();

        staticAnimations = GameObject.Find("PlayerStaticAnimation");
    }

    public void OnUpdate() {
        if (velocity.x > 1f) {
            playerAnimation.isFacingLeft = false;
        } else if (velocity.x < -1f) {
            playerAnimation.isFacingLeft = true;
        }
            
        if (playerAnimation.isFacingLeft == true) {
            //transform.localScale = new Vector3(-1f, 1f, 1f);
            //staticAnimations.transform.localScale = new Vector3(1f, -1f, 1f);
        } else {
            //transform.localScale = new Vector3(1f, 1f, 1f);
            //staticAnimations.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        //staticAnimations.transform.position = Vector3.Lerp(staticAnimations.transform.position, transform.position, lerpValue);

        //staticAnimations.transform.localRotation = Quaternion.Lerp(staticAnimations.transform.localRotation, transform.rotation, lerpValue);
    }
}
