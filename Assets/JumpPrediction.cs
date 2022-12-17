using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPrediction : MonoBehaviour
{

    [SerializeField]
    private PlayerMovementProperties _movementProperties;

    [SerializeField]
    private MovementSystem _movement;

    [SerializeField]
    private LineRenderer _line;

    //private float fFactor = 6.75f;

    void FixedUpdate()
    {
        /*
        float theta = Vector2.SignedAngle(Vector2.right, transform.up) * Mathf.Deg2Rad;
        float gamma = Vector2.SignedAngle(Vector2.right, transform.right) * Mathf.Deg2Rad;

        float xComponentOfYVelo = Mathf.Cos(theta) * _movementProperties.jumpVelocity;
        float yComponentOfYVelo = Mathf.Sin(theta) * _movementProperties.jumpVelocity;

        float xComponentOfXVelo = Mathf.Cos(gamma) * _movement.Velocity.x;
        float yComponentOfXVelo = Mathf.Sin(gamma) * _movement.Velocity.x;

        Vector2 predVelo;
        if (_movement.IsGrounded) {
            predVelo = new Vector2(xComponentOfXVelo + xComponentOfYVelo, yComponentOfXVelo + yComponentOfYVelo);
        } else {
            predVelo = _movement.AirborneVelocity;
        }

        RaycastHit2D predictHit = new RaycastHit2D();
        Vector2 pos = transform.position;

        Vector2 velo = new Vector2(predVelo.x, predVelo.y) * Time.fixedDeltaTime * fFactor;

        int count = 0;
        List<Vector3> points = new List<Vector3>();

        while (predictHit.collider == null && count < 100) {

            // Generate new ray
            Ray2D ray = new Ray2D(pos, velo.normalized);
            
            points.Add(pos);

            if (count % 2 == 1) {
                Debug.DrawRay(ray.origin, ray.direction * velo.magnitude, Color.green);
            }

            // Update predictHit
            predictHit = Physics2D.Raycast(ray.origin, ray.direction, velo.magnitude, _movement.Mask);

            // Update position to end of predictHit ray
            pos += (ray.direction * velo.magnitude);

            velo += (Vector2.down * _movementProperties.gravity * Time.fixedDeltaTime);

            count++;
        }

        _line.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++) {
            _line.SetPosition(i, points[i]);
        }
        */
    }
}
