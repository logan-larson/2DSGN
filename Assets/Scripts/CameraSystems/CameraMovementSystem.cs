using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementSystem : MonoBehaviour
{
    PlayerPosition playerPosition;
    GameObject player;

    public void OnStart()
    {
        player = GameObject.Find("PlayerNew");

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
        
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);
        
    }
}
