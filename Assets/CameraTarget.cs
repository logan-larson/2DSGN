using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player = null;
    [SerializeField] private float posLerpValue = 0.05f;
    [SerializeField] private float rotLerpValue = 0.05f;

    [SerializeField] private float startingZ = -10f;
    [SerializeField] private float zMod = 0.4f;

    private Vector3 velocity = Vector3.zero;


    private MovementSystem movementSystem;


    private void Awake()
    {
        FirstObjectNotifier.OnFirstObjectSpawned += FirstObjectNotifier_OnFirstObjectSpawned;
    }

    private void OnDestroy()
    {
        FirstObjectNotifier.OnFirstObjectSpawned -= FirstObjectNotifier_OnFirstObjectSpawned;
    }


    private void FirstObjectNotifier_OnFirstObjectSpawned(Transform obj, GameObject go)
    {
        player = obj;
        movementSystem = go.GetComponent<MovementSystem>();
    }


    void LateUpdate()
    {
        if (player == null) return;

        // Get the mouse position in world space
        Vector3 mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));

        // Set the target position to the player position
        Vector3 targetPos = player.position;


        // Calculate the camera z position based on the player's velocity
        // Zoom out when moving faster
        Vector3 velocity = movementSystem.PublicData.Velocity;

        targetPos.z = startingZ - (velocity.magnitude * zMod);

        targetPos.z = Mathf.Clamp(targetPos.z, -100f, -1f);

        // Round target position to nearest pixel
        Vector3 vectorInPixels = new Vector3(
            Mathf.RoundToInt(targetPos.x * 32),
            Mathf.RoundToInt(targetPos.y * 32),
            Mathf.RoundToInt(targetPos.z * 32)
        );

        // Convert position to units
        Vector3 posInUnits = vectorInPixels / 32;

        // Lerp to nearest position and rotation
        Vector3 lerpedPos = Vector3.Lerp(transform.position, posInUnits, posLerpValue);
        // Vector3 dampedPos = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, posLerpValue);
        Quaternion lerpedRot = Quaternion.Lerp(this.transform.rotation, player.rotation, rotLerpValue);

        this.transform.SetPositionAndRotation(new Vector3(lerpedPos.x, lerpedPos.y, lerpedPos.z), lerpedRot);
        // this.transform.SetPositionAndRotation(new Vector3(dampedPos.x, dampedPos.y, dampedPos.z), lerpedRot);
    }
}
