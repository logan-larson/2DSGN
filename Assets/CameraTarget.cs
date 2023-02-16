using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player = null;
    [SerializeField] private float threshold;
    [SerializeField] private float posLerpValue = 0.1f;
    [SerializeField] private float rotLerpValue = 0.1f;

    [SerializeField] private float startingZ = -1f;
    [SerializeField] private float zMod = 0.2f;


    [SerializeField] private Vector2 velocity;
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


    void Update()
    {
        if (player == null) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));
        // Vector3 targetPos = (player.position + new Vector3(mousePos.x, mousePos.y, 0f)) / 2f;
        Vector3 targetPos = (player.position + mousePos) / 2f;

        // targetPos.x = Mathf.Clamp(targetPos.x, player.position.x - threshold, player.position.x + threshold);
        // targetPos.y = Mathf.Clamp(targetPos.y, player.position.y - threshold, player.position.y + threshold);
        targetPos = player.position;

        velocity = movementSystem._currentVelocity;

        targetPos.z = startingZ - (velocity.magnitude * zMod);

        targetPos.z = Mathf.Clamp(targetPos.z, -100f, -1f);

        this.transform.position = Vector3.Lerp(this.transform.position, targetPos, posLerpValue);
        // this.transform.position = Vector3.Lerp(this.transform.position, player.position, posLerpValue);

        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, player.rotation, rotLerpValue);
        // cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, player.rotation, rotLerpValue);
    }
}
