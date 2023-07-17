using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : NetworkBehaviour
{
    public Camera Camera;
    public CameraController CameraController;

    private RespawnManager _respawnManager;

    private void Awake()
    {
        _respawnManager = gameObject.GetComponent<RespawnManager>();
        _respawnManager.OnRespawn.AddListener(OnRespawn);
    }

    private void OnRespawn()
    {
        SetPlayerFollowTargetRpc(base.Owner, gameObject);
    }

    public void SetCamera(Camera camera, CameraController controller)
    {
        Camera = camera;
        CameraController = controller;
    }

    [TargetRpc]
    public void SetPlayerFollowTargetRpc(NetworkConnection target, GameObject player)
    {
        CameraController.SetPlayer(player.transform);
    }
}
