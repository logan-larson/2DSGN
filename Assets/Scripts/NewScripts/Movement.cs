using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    bool grounded = true;
    RaycastHit2D landingPoint;

    void Start()
    {
        
    }

    public void OnUpdate()
    {
        grounded = GetIsGrounded();
        
        if (grounded) {
            OrientSelf();
            
            SetHeight();
        } else {
            landingPoint = GetLandingPoint();

            AddGravity();
        }

    }

    /* Update driven events */
    bool GetIsGrounded() {
        return true;
    }
    
    void OrientSelf() {}

    void SetHeight() {}

    RaycastHit2D GetLandingPoint() {
        return new RaycastHit2D();
    }

    void AddGravity() {}

    /* Input driven events */
    public void Move(Vector2 movement) {

    }

    public void Jump() {

    }

    public void Sprint() {

    }
}
