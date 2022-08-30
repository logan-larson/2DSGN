using UnityEngine;

public class CameraController : MonoBehaviour
{
    GameObject player;
    PlayerPosition playerPosition;

    public float speed = 2f;

    void Start()
    {
        player = GameObject.Find("Player");
        playerPosition = player.GetComponent<PlayerPosition>();
    }

    void LateUpdate()
    {
        float lerpValue = speed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, new Vector3(playerPosition.x, playerPosition.y, -10f), lerpValue);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, playerPosition.rotation), lerpValue);
    }
}
