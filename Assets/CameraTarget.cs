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


    private void Awake()
    {
        FirstObjectNotifier.OnFirstObjectSpawned += FirstObjectNotifier_OnFirstObjectSpawned;
    }

    private void OnDestroy()
    {
        FirstObjectNotifier.OnFirstObjectSpawned -= FirstObjectNotifier_OnFirstObjectSpawned;
    }


    private void FirstObjectNotifier_OnFirstObjectSpawned(Transform obj)
    {
        player = obj;
    }


    void Update()
    {
        if (player == null) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));
        // Vector3 targetPos = (player.position + new Vector3(mousePos.x, mousePos.y, 0f)) / 2f;
        Vector3 targetPos = (player.position + mousePos) / 2f;
        
        targetPos.x = Mathf.Clamp(targetPos.x, player.position.x - threshold, player.position.x + threshold);
        targetPos.y = Mathf.Clamp(targetPos.y, player.position.y - threshold, player.position.y + threshold);
        //targetPos.z = -10f;

        this.transform.position = Vector3.Lerp(this.transform.position, targetPos, posLerpValue);
        
        //this.transform.rotation = Quaternion.Lerp(this.transform.rotation, player.rotation, lerpValue);
        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, player.rotation, rotLerpValue);
    }
}
