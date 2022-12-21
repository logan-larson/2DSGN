using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animating : MonoBehaviour
{
    //PlayerPosition position;
    // PlayerVelocity velocity;
    // PlayerMode mode;
    // PlayerAnimation playerAnimation;

    public float lerpValue = 0.1f;
    public float positionLerpValue = 0.1f;

    private void Awake()
    {
        //position = GetComponentInParent<PlayerPosition>();
        // velocity = GetComponentInParent<PlayerVelocity>();
        // mode = GetComponentInParent<PlayerMode>();
        // playerAnimation = GetComponentInParent<PlayerAnimation>();
    }

    private void Update()
    {
        // Set direction facing by velocity
        // TODO Adjust this for in flight velocity
        /*
        if (velocity.x > 1f || velocity.veloOffGround.x > 1f) {
            playerAnimation.isFacingLeft = false;
        } else if (velocity.x < -1f || velocity.veloOffGround.x < -1f) {
            playerAnimation.isFacingLeft = true;
        }
        */

        // Update animation position based on player velocity
        Vector2 positionLerped = Vector2.Lerp(transform.position, transform.parent.transform.position, positionLerpValue);

        Vector2 vectorInPixels = new Vector2(
            Mathf.RoundToInt(positionLerped.x * 32),
            Mathf.RoundToInt(positionLerped.y * 32)
        );

        //return vectorInPixels / 32;
        Vector2 posInUnits = vectorInPixels / 32;

        Vector2 lerpedPos = Vector2.Lerp(transform.position, posInUnits, lerpValue);

        transform.SetPositionAndRotation(new Vector3(lerpedPos.x, lerpedPos.y), transform.rotation);
        // Adjust the targeted rotation by 90 degrees if in combat mode
        /*
        Quaternion tRot = mode.inCombatMode ? transform.rotation * Quaternion.Euler(0f, 0f, 90f) : transform.rotation;
        Quaternion lerpedRot = Quaternion.Lerp(transform.rotation, tRot, lerpValue);

        
        // Set the actual direction the sprite faces
        if (playerAnimation.isFacingLeft == true) {
            if (mode.inParkourMode) {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            } else if (mode.inCombatMode) {
                transform.localScale = new Vector3(1f, -1f, 1f);
            }
        } else {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        */
        
    }
}
