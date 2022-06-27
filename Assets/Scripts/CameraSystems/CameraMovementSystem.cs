using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementSystem : MonoBehaviour
{
    PlayerPosition playerPosition;

    public void OnStart()
    {
        GameObject player = GameObject.Find("Player");

        if (player != null)
        {
            playerPosition = player.GetComponent<PlayerPosition>();
        }
    }

    public void OnUpdate()
    {
        if (playerPosition != null)
        {
            transform.position = new Vector3(playerPosition.x, playerPosition.y, -10);
        }
        
    }
}
