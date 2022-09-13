using UnityEngine;

[RequireComponent (typeof(PlayerVelocity))]
[RequireComponent (typeof(PlayerAnimation))]
[RequireComponent (typeof(PlayerMode))]
public class AnimationSystem : MonoBehaviour {

    PlayerPosition position;
    PlayerVelocity velocity;
    PlayerMode mode;
    PlayerAnimation playerAnimation;

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

        staticAnimations = GameObject.Find("PlayerStaticAnimation");
        boneBody = GameObject.Find("Bone_Body");

        staticAnimations.transform.position = transform.position;
    }

    public void OnUpdate() {
        SetMode();

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

        //staticAnimations.transform.Translate(veloInUnits * Time.deltaTime); 
        Quaternion lerpedRot = Quaternion.Lerp(staticAnimations.transform.rotation, transform.rotation, lerpValue);


        staticAnimations.transform.SetPositionAndRotation(new Vector3(lerpedPos.x, lerpedPos.y), lerpedRot);
            
        if (playerAnimation.isFacingLeft == true) {
            //transform.localScale = new Vector3(-1f, 1f, 1f);
            staticAnimations.transform.localScale = new Vector3(-1f, 1f, 1f);
        } else {
            //transform.localScale = new Vector3(1f, 1f, 1f);
            staticAnimations.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        //staticAnimations.transform.position = Vector3.Lerp(staticAnimations.transform.position, transform.position, lerpValue);

        //staticAnimations.transform.localRotation = Quaternion.Lerp(staticAnimations.transform.localRotation, transform.rotation, lerpValue);
    }

    void SetMode() {
        /*
        if (mode.inCombatMode) {
            boneBody.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        } else if (mode.inParkourMode) {
            boneBody.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        */
    }
}
