using UnityEngine;

[RequireComponent (typeof(PlayerVelocity))]
[RequireComponent (typeof(PlayerAnimation))]
[RequireComponent (typeof(PlayerMode))]
[RequireComponent (typeof(PlayerGrounded))]
public class AnimationSystem : MonoBehaviour {

    PlayerPosition position;
    PlayerVelocity velocity;
    PlayerMode mode;
    PlayerGrounded grounded;
    PlayerAnimation playerAnimation;

    Animator animator;

    // Child game objects
    GameObject staticAnimations;

    // Bones
    GameObject boneBody;

    public float lerpValue = 0.1f;

    public void OnStart() {
        position = GetComponent<PlayerPosition>();
        velocity = GetComponent<PlayerVelocity>();
        playerAnimation = GetComponent<PlayerAnimation>();
        mode = GetComponent<PlayerMode>();
        grounded = GetComponent<PlayerGrounded>();

        staticAnimations = GameObject.Find("PlayerAnimation");
        boneBody = GameObject.Find("Bone_Body");

        animator = staticAnimations.GetComponent<Animator>();

        staticAnimations.transform.position = transform.position;
    }

    public void OnUpdate() {

        animator.SetFloat("speed", Mathf.Abs(velocity.x));
        
        animator.SetBool("inCombat", mode.inCombatMode);

        animator.SetBool("isJumping", !grounded.isGrounded);


        if (velocity.x > 1f || velocity.veloOffGround.x > 1f) {
            playerAnimation.isFacingLeft = false;
        } else if (velocity.x < -1f || velocity.veloOffGround.x < -1f) {
            playerAnimation.isFacingLeft = true;
        }

        // Update animation position based on player velocity
        Vector2 vectorInPixels = new Vector2(
            Mathf.RoundToInt(position.x * 32),
            Mathf.RoundToInt(position.y * 32)
        );

        //return vectorInPixels / 32;
        Vector2 posInUnits = vectorInPixels / 32;

        Vector2 lerpedPos = Vector2.Lerp(staticAnimations.transform.position, posInUnits, lerpValue);

        // Adjust the targeted rotation by 90 degrees if in combat mode
        Quaternion tRot = mode.inCombatMode ? transform.rotation * Quaternion.Euler(0f, 0f, 90f) : transform.rotation;
        Quaternion lerpedRot = Quaternion.Lerp(staticAnimations.transform.rotation, tRot, lerpValue);

        staticAnimations.transform.SetPositionAndRotation(new Vector3(lerpedPos.x, lerpedPos.y), lerpedRot);
            
        if (playerAnimation.isFacingLeft == true) {
            if (mode.inParkourMode) {
                staticAnimations.transform.localScale = new Vector3(-1f, 1f, 1f);
            } else if (mode.inCombatMode) {
                staticAnimations.transform.localScale = new Vector3(1f, -1f, 1f);
            }
        } else {
            staticAnimations.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
