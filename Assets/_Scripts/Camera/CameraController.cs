using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using UnityEngine;

/**
<summary>
CameraController moves the camera's position and rotation to follow the player's position and rotation.
</summary>
*/
public class CameraController : NetworkBehaviour
{
    [SerializeField] private Camera cam;
    public Transform player = null;
    [SerializeField] private float posLerpValue = 0.05f;
    [SerializeField] private float _airborneRotLerpValue = 0.01f;
    [SerializeField] private float _grounedRotLerpValue = 0.05f;

    [SerializeField] private float startingZ = -10f;
    /// <summary>
    /// The factor by which the z position is effected by the player's velocity.
    /// </summary>
    [SerializeField] private float zFactor = 0.4f;
    [SerializeField] private float _rotationDelay = 0.25f;

    private MovementSystem _movementSystem;
    private InputSystem _inputSystem;
    private PlayerInputValues _inputValues;
    private ModeManager _modeManager;

    public float CurrentZ => this.transform.position.z;

    private IEnumerator _delayRotationCoroutine;
    private bool _prevIsFirePressed = false;

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
        _movementSystem = go.GetComponent<MovementSystem>();
        _inputSystem = go.GetComponent<InputSystem>();
        _inputValues = _inputSystem.InputValues;
        _modeManager = go.GetComponent<ModeManager>();
        go.GetComponent<CameraManager>().SetCamera(cam, this);
    }

    public void SetPlayer(Transform player)
    {
        this.player = player;
    }


    void LateUpdate()
    {
        if (player == null) return;

        // Calculate the camera z position based on the player's velocity
        // Zoom out when moving faster
        Vector3 velocity = _movementSystem.PublicData.Velocity;

        Vector3 targetPos = player.position;

        targetPos.z = startingZ - (velocity.magnitude * zFactor);

        targetPos.z = Mathf.Clamp(targetPos.z, -100f, -1f);

        // Lerp to nearest position and rotation
        Vector3 lerpedPos = Vector3.Lerp(this.transform.position, targetPos, posLerpValue);

        // When the player releases the fire button, delay the camera rotation to give the player
        // a chance to start firing again and not have the camera rotate
        if (_prevIsFirePressed != _inputValues.IsFirePressed)
        {
            if (_inputValues.IsFirePressed)
            {
                if (_delayRotationCoroutine != null)
                {
                    StopCoroutine(_delayRotationCoroutine);
                }
            }
            else if (_modeManager.CurrentMode != ModeManager.Mode.Sliding)
            {
                _delayRotationCoroutine = DelayRotationCoroutine();
                StartCoroutine(_delayRotationCoroutine);
            }
        }

        // Set the rotation lerp value based on whether the player is grounded or not
        var rotLerpValue = _movementSystem.PublicData.IsGrounded ? _grounedRotLerpValue : _airborneRotLerpValue;

        // If the player is shooting don't adjust the camera rotation, unless they are sliding
        Quaternion lerpedRot = (_inputValues.IsFirePressed && _modeManager.CurrentMode != ModeManager.Mode.Sliding) || _delayRotationCoroutine != null
            ? this.transform.rotation
            : Quaternion.Lerp(this.transform.rotation, player.rotation, rotLerpValue);

        this.transform.SetPositionAndRotation(new Vector3(lerpedPos.x, lerpedPos.y, lerpedPos.z), lerpedRot);

        _prevIsFirePressed = _inputValues.IsFirePressed;
    }

    private IEnumerator DelayRotationCoroutine()
    {
        yield return new WaitForSeconds(_rotationDelay);
        _delayRotationCoroutine = null;
    }
}
