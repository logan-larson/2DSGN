using UnityEngine;

/**
<summary>
CameraController moves the camera's position and rotation to follow the player's position and rotation.
</summary>
*/
public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player = null;
    [SerializeField] private float posLerpValue = 0.05f;
    [SerializeField] private float rotLerpValue = 0.05f;

    [SerializeField] private float startingZ = -10f;
    /// <summary>
    /// The factor by which the z position is effected by the player's velocity.
    /// </summary>
    [SerializeField] private float zFactor = 0.4f;

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

        // Calculate the camera z position based on the player's velocity
        // Zoom out when moving faster
        Vector3 velocity = movementSystem.PublicData.Velocity;

        Vector3 targetPos = player.position;

        targetPos.z = startingZ - (velocity.magnitude * zFactor);

        targetPos.z = Mathf.Clamp(targetPos.z, -100f, -1f);

        // Lerp to nearest position and rotation
        Vector3 lerpedPos = Vector3.Lerp(this.transform.position, targetPos, posLerpValue);
        Quaternion lerpedRot = Quaternion.Lerp(this.transform.rotation, player.rotation, rotLerpValue);

        this.transform.SetPositionAndRotation(new Vector3(lerpedPos.x, lerpedPos.y, lerpedPos.z), lerpedRot);
    }
}
