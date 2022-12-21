using UnityEngine;

public class SmoothPixels : MonoBehaviour
{

    [SerializeField]
    private float lerpValue = 0.1f;

    private void Update()
    {

        // Round to nearest pixel
        Vector2 vectorInPixels = new Vector2(
            Mathf.RoundToInt(transform.position.x * 32),
            Mathf.RoundToInt(transform.position.y * 32)
        );

        // Convert to units
        Vector2 posInUnits = vectorInPixels / 32;

        // Lerp to nearest pixel
        Vector2 lerpedPos = Vector2.Lerp(transform.position, posInUnits, lerpValue);

        // Set position
        transform.SetPositionAndRotation(new Vector3(lerpedPos.x, lerpedPos.y), transform.rotation);
    }
}
