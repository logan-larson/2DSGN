using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class RotateTowardMouse : MonoBehaviour
{

    SpriteRenderer head;
    public Transform parent;
    public Vector3 dir;
    public float angle;
    public float parentAngle;
    public float locAngle;

    void Start() {
        head = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate() {

        /* TODO
        dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

        dir = new Vector3(dir.x, dir.y, 0f);

        dir.Normalize();

        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        parentAngle = parent.rotation.eulerAngles.z;

        locAngle = parentAngle - angle;

        if (locAngle > 90f || locAngle < -90f) {
            head.flipX = true;
        } else {
            head.flipX = false;
        }
        //transform.rotation = Quaternion.Euler(0f, 0f, Localization);
        */
    }
}
