using Cinemachine;
using FishNet.Object;
using UnityEngine;

public class CameraController : NetworkBehaviour
{

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            Camera cam = GetComponent<Camera>();
            cam.enabled = true;
        }
    }

    private void Awake()
    {
        //FirstObjectNotifier.OnFirstObjectSpawned += FirstObjectNotifier_OnFirstObjectSpawned;
    }

    private void OnDestroy()
    {
        //FirstObjectNotifier.OnFirstObjectSpawned -= FirstObjectNotifier_OnFirstObjectSpawned;
    }


    private void FirstObjectNotifier_OnFirstObjectSpawned(Transform obj)
    {
        //CinemachineVirtualCamera vc = GetComponent<CinemachineVirtualCamera>();
        //vc.Follow = obj;
    }
}
