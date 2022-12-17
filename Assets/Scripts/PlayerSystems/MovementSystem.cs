using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;

[RequireComponent (typeof(PlayerInputValues))]
[RequireComponent (typeof(PlayerPosition))]
[RequireComponent (typeof(PlayerVelocity))]
[RequireComponent (typeof(PlayerGrounded))]
[RequireComponent (typeof(PlayerMode))]
public class MovementSystem : NetworkBehaviour
{

    #region Types

    /// <summary>
    /// All the data needed to move the player on the client and server.
    /// </summary>
    public struct MoveData
    {
        public float Horizontal;
        /*
        public bool Jump;
        public bool Sprint;
        public bool ChangeToCombat; // Fire key
        public bool ChangeToParkour; // Sprint key
        public bool InParkourMode;
        */
    }

    /// <summary>
    /// All the data needed to reconcile the player on the client and server.
    /// </summary>
    public struct ReconcileData
    {
        public Vector2 Position;
        public Vector2 Velocity;
        /*
        public float Rotation;
        public Vector2 AirborneVelocity;
        public bool IsGrounded;
        public bool InParkourMode;
        public bool InCombatMode;
        */
    }

    #endregion

    #region Script References

    [HideInInspector]
    public PlayerInputValues Input;
    [SerializeField]
    private PlayerMovementProperties MovementProperties;

    #endregion

    #region Public Variables

    /*
    public Vector2 Position; 
    public float Rotation;
    public Vector2 Velocity;
    public Vector2 AirborneVelocity;

    public bool IsGrounded;
    public float GroundDistance;
    public LayerMask Mask;
    public bool InParkourMode;
    public bool InCombatMode;

    RaycastOrigins raycastOrigins;

    public struct RaycastOrigins {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }
    */

    #endregion

    #region Private Variables

    /// <summary>
    /// The current velocity of the player.
    /// </summary>
    private Vector3 _currentVelocity = new Vector3();

    /// <summary>
    /// True if subscribed to the TimeManager.
    private bool _subscribedToTimeManager = false;


    /*
    /// <summary>
    /// True if the player is currently jumping.
    /// </summary>
    private bool jumpNow = false;
    */

    //private Vector3 predictPos = new Vector3();
    //private Vector3 predictNorm = new Vector3();

    //private bool recalculateLanding = false;

    #endregion


    #region Time Management

    private void SubscribeToTimeManager(bool subscribe)
    {
        if (base.TimeManager == null)
            return;

        if (subscribe == _subscribedToTimeManager)
            return;

        _subscribedToTimeManager = subscribe;

        if (subscribe)
        {
            base.TimeManager.OnTick += OnTick;
        }
        else
        {
            base.TimeManager.OnTick -= OnTick;
        }
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        SubscribeToTimeManager(true);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        SubscribeToTimeManager(false);
    }

    #endregion


    #region Movement

    private void Awake() { // public void OnStart
        Input = GetComponent<PlayerInputValues>();

        if (Input == null)
            Debug.LogError("PlayerInput not found on " + gameObject.name);
    }

    private void OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);

            BuildActions(out MoveData moveData);

            Move(moveData, false);
        }

        if (base.IsServer)
        {
            Move(default, true);

            ReconcileData reconcileData = new ReconcileData()
            {
                Position = transform.position,
                Velocity = _currentVelocity,
                /*
                Rotation = Rotation,
                AirborneVelocity = AirborneVelocity,
                IsGrounded = IsGrounded,
                InParkourMode = InParkourMode,
                InCombatMode = InCombatMode
                */
            };

            Reconciliation(reconcileData, true);
        }
    }

    private void BuildActions(out MoveData moveData)
    {
        moveData = default;

        //if (moveData.Horizontal == Input.HorizontalMovementInput) return;

        moveData.Horizontal = Input.HorizontalMovementInput;
        /*
        moveData.Jump = input.isJumpKeyPressed;
        moveData.Sprint = input.isSprintKeyPressed;

        if (input.isSprintKeyPressed && !moveData.InParkourMode)
        {
            moveData.ChangeToParkour = true;
            moveData.ChangeToCombat = false;
        }

        if (input.isShootingKeyPressed && moveData.InParkourMode)
        {
            moveData.ChangeToParkour = false;
            moveData.ChangeToCombat = true;
        }
        */
    }

    [Replicate]
    private void Move(MoveData moveData, bool asServer, bool replaying = false)
    {
        /*
        UpdateMode(moveData);

        UpdateRaycastOrigins();

        if (movementProperties.timeSinceGrounded > minJumpTime)  {
            UpdateGrounded();
        } else {
            movementProperties.timeSinceGrounded += Time.fixedDeltaTime;
        }
        */

        UpdateVelocity(moveData);
            
        /*
        // This is a velocity relative to the player not the world
        Vector2 adjVelo = new Vector2(Velocity.x, Velocity.y) * Time.fixedDeltaTime;

        // If grounded, match body to ground
        if (IsGrounded) {
            //movementProperties.maxRotationDegrees = 2f;


            Ray2D leftRay = new Ray2D(raycastOrigins.bottomLeft + adjVelo, -transform.up);
            Ray2D rightRay = new Ray2D(raycastOrigins.bottomRight + adjVelo, -transform.up);

            RaycastHit2D leftHit = Physics2D.Raycast(leftRay.origin, leftRay.direction, movementProperties.groundedHeight, Mask);
            RaycastHit2D rightHit = Physics2D.Raycast(rightRay.origin, rightRay.direction, movementProperties.groundedHeight, Mask);

            // Use override hit to prevent clipping
            RaycastHit2D overrideHit = Physics2D.Raycast((leftRay.origin + rightRay.origin) / 2, transform.right, movementProperties.overrideRayLength * Mathf.Sign(Velocity.x), Mask);
            if (Velocity.x < 0f && overrideHit.collider != null) {
                leftHit = overrideHit;
            } else if (Velocity.x > 0f && overrideHit.collider != null) {
                rightHit = overrideHit;
            }

            // Apply rotation to orient body to match ground
            if (leftHit && rightHit) {
                Vector2 avgPoint = (leftHit.point + rightHit.point) / 2;
                Vector2 avgNorm = (leftHit.normal + rightHit.normal) / 2;

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, avgNorm);
                Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, movementProperties.maxRotationDegrees);

                transform.rotation = Quaternion.Euler(0f, 0f, finalRotation.eulerAngles.z);
                Rotation = finalRotation.eulerAngles.z;
            }

            transform.Translate(adjVelo);
            Position.x = transform.position.x;
            Position.y = transform.position.y;
        } else { 
            //movementProperties.maxRotationDegrees = 8f;
            // We are in the air, so move according to worldspace not self
            transform.position += new Vector3(AirborneVelocity.x, AirborneVelocity.y, 0f) * Time.fixedDeltaTime;

            Position.x = transform.position.x;
            Position.y = transform.position.y;

            // And rotate to the predicted landing spots normal
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, predictNorm);
            Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, movementProperties.maxRotationDegrees);

            transform.rotation = Quaternion.Euler(0f, 0f, finalRotation.eulerAngles.z);
            Rotation = finalRotation.eulerAngles.z;
        }
        */
    }

    [Reconcile]
    private void Reconciliation(ReconcileData data, bool asServer)
    {
        transform.position = data.Position;
        _currentVelocity = data.Velocity;
        /*
        transform.rotation = Quaternion.Euler(0, 0, data.Rotation);
        AirborneVelocity = data.AirborneVelocity;
        IsGrounded = data.IsGrounded;
        InParkourMode = data.InParkourMode;
        InCombatMode = data.InCombatMode;
        */
    }

    /*
    private void FixedUpdate() { // public void OnUpdate

        if (!base.IsOwner)
            return;

        UpdateMode();

        UpdateRaycastOrigins();

        if (movementProperties.timeSinceGrounded > minJumpTime)  {
            UpdateGrounded();
        } else {
            movementProperties.timeSinceGrounded += Time.fixedDeltaTime;
        }

        UpdateVelocity();
            

        // This is a velocity relative to the player not the world
        Vector2 adjVelo = new Vector2(Velocity.x, Velocity.y) * Time.fixedDeltaTime;

        // If grounded, match body to ground
        if (IsGrounded) {
            //movementProperties.maxRotationDegrees = 2f;


            Ray2D leftRay = new Ray2D(raycastOrigins.bottomLeft + adjVelo, -transform.up);
            Ray2D rightRay = new Ray2D(raycastOrigins.bottomRight + adjVelo, -transform.up);

            RaycastHit2D leftHit = Physics2D.Raycast(leftRay.origin, leftRay.direction, movementProperties.groundedHeight, Mask);
            RaycastHit2D rightHit = Physics2D.Raycast(rightRay.origin, rightRay.direction, movementProperties.groundedHeight, Mask);

            // Use override hit to prevent clipping
            RaycastHit2D overrideHit = Physics2D.Raycast((leftRay.origin + rightRay.origin) / 2, transform.right, movementProperties.overrideRayLength * Mathf.Sign(Velocity.x), Mask);
            if (Velocity.x < 0f && overrideHit.collider != null) {
                leftHit = overrideHit;
            } else if (Velocity.x > 0f && overrideHit.collider != null) {
                rightHit = overrideHit;
            }

            // Apply rotation to orient body to match ground
            if (leftHit && rightHit) {
                Vector2 avgPoint = (leftHit.point + rightHit.point) / 2;
                Vector2 avgNorm = (leftHit.normal + rightHit.normal) / 2;

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, avgNorm);
                Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, movementProperties.maxRotationDegrees);

                transform.rotation = Quaternion.Euler(0f, 0f, finalRotation.eulerAngles.z);
                Rotation = finalRotation.eulerAngles.z;
            }

            transform.Translate(adjVelo);
            Position.x = transform.position.x;
            Position.y = transform.position.y;
        } else { 
            //movementProperties.maxRotationDegrees = 8f;
            // We are in the air, so move according to worldspace not self
            transform.position += new Vector3(AirborneVelocity.x, AirborneVelocity.y, 0f) * Time.fixedDeltaTime;

            Position.x = transform.position.x;
            Position.y = transform.position.y;

            // And rotate to the predicted landing spots normal
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, predictNorm);
            Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, movementProperties.maxRotationDegrees);

            transform.rotation = Quaternion.Euler(0f, 0f, finalRotation.eulerAngles.z);
            Rotation = finalRotation.eulerAngles.z;
        }
    }
    */

    void UpdateMode(MoveData moveData = new MoveData())
    {
        /*
        if (moveData.ChangeToParkour)
        {
            InCombatMode = false;
            InParkourMode = true;
        }
        else if (moveData.ChangeToCombat)
        {
            InCombatMode = true;
            InParkourMode = false;
        }
        */
    }

    void UpdateRaycastOrigins() {
        /*
		raycastOrigins.bottomLeft = transform.position - (transform.right / 2) - (transform.up / 2);
		raycastOrigins.bottomRight = transform.position + (transform.right / 2) - (transform.up / 2);
		raycastOrigins.topLeft = transform.position - (transform.right / 2) + (transform.up / 2);
		raycastOrigins.topRight = transform.position + (transform.right / 2) + (transform.up / 2);
        */
    }

    void UpdateGrounded() {
        /*
        if (IsGrounded) {
            RaycastHit2D groundedHit = Physics2D.Raycast(transform.position, -transform.up, movementProperties.groundedHeight * 1.5f, Mask);
            IsGrounded = groundedHit.collider != null;
            GroundDistance = groundedHit.distance;
        } else {
            RaycastHit2D groundedHit = Physics2D.Raycast(transform.position, -transform.up, movementProperties.groundedHeight, Mask);
            IsGrounded = groundedHit.collider != null;
            GroundDistance = groundedHit.distance;
        }
        */
    }

    public void Jump() {
        /*
        if (IsGrounded) {
            jumpNow = true;
        }
        */
    }

    void UpdateVelocity(MoveData moveData = new MoveData()) {

        if (moveData.Horizontal != 0f)
        {
            _currentVelocity += new Vector3(moveData.Horizontal, 0f, 0f) * MovementProperties.Acceleration;
        }
        else
        {
            _currentVelocity = Vector3.MoveTowards(_currentVelocity, Vector3.zero, MovementProperties.Friction);
        }

        if (_currentVelocity.magnitude > MovementProperties.MaxSpeed) {
            _currentVelocity = _currentVelocity.normalized * MovementProperties.MaxSpeed;
        }

        transform.position += _currentVelocity * (float) TimeManager.TickDelta;

        // Add velo based on horizontal input, accelerate faster when sprint key is pressed
        /*
        if (IsGrounded) {
            
            if (moveData.Sprint) {
                Velocity.x += moveData.Horizontal * (movementProperties.acceleration * 2f);
            } else {
                Velocity.x += moveData.Horizontal * movementProperties.acceleration;
            }

            predictPos = new Vector3();
            predictNorm = new Vector3();
        }

        // Apply friction when no input is held
        if (IsGrounded && moveData.Horizontal == 0f) {
            if (Mathf.Abs(Velocity.x - movementProperties.friction) < 0.5f) {
                Velocity.x = 0f;
            } else {
                if (Velocity.x > 0f) {
                    Velocity.x = Mathf.Clamp(Velocity.x, 0f, Velocity.x - movementProperties.friction);
                } else {
                    Velocity.x = Mathf.Clamp(Velocity.x, Velocity.x + movementProperties.friction, 0f);
                }
            }
        }

        // Set height above ground
        if (IsGrounded) {
            if (jumpNow) {
                jumpNow = false;
                IsGrounded = false;
                movementProperties.timeSinceGrounded = 0f;
                Velocity.y = movementProperties.jumpVelocity;
            } else { // This needs a lot of work to make player landings smoother
                if (GroundDistance < 0.9f) {
                    Velocity.y += 0.05f; 
                } else if (GroundDistance > 1f) {
                    Velocity.y -= 0.05f; 
                } else {
                    Velocity.y = 0f;
                }
            }

            AirborneVelocity = new Vector2(0f, 0f);
        } else {
            // Add gravity in downward direction relative to worldspace
            AirborneVelocity += (Vector2.down * movementProperties.gravity * Time.fixedDeltaTime);

            // This is where airborne movement forces can be applied

            // If forces were applied then we need to recalculate the landing
            recalculateLanding = false;
        }

        // Set sprint multiplier
        if (moveData.Sprint && moveData.InParkourMode) {
            movementProperties.sprintMultiplier += movementProperties.acceleration * 2f;
            movementProperties.sprintMultiplier = Mathf.Clamp(movementProperties.sprintMultiplier, 0f, movementProperties.maxSprintSpeed);
        } else {
            movementProperties.sprintMultiplier -= movementProperties.friction;
            movementProperties.sprintMultiplier = Mathf.Clamp(movementProperties.sprintMultiplier, 0f, movementProperties.maxSprintSpeed);
        }

        // Limit top speed
        Velocity.x = Mathf.Clamp(Velocity.x, -movementProperties.maxXSpeed - movementProperties.sprintMultiplier, movementProperties.maxXSpeed + movementProperties.sprintMultiplier);

        // This is triggered directly after the jump is initialized
        if (!IsGrounded && movementProperties.timeSinceGrounded == 0f) {
            // Get the velocity directly after the jump because we don't allow
            // airborne movement controls besides using a weapon to move

            // Theta = yAngleFromRight
            // X1 = xComponentOfYVelo
            // Y1 = yComponentOfYVelo

            // Gamma = xAngleFromRight
            // X2 = xComponentOfXVelo
            // Y2 = yComponentOfXVelo 

            float theta = Vector2.SignedAngle(Vector2.right, transform.up) * Mathf.Deg2Rad;
            float gamma = Vector2.SignedAngle(Vector2.right, transform.right) * Mathf.Deg2Rad;

            float xComponentOfYVelo = Mathf.Cos(theta) * Velocity.y;
            float yComponentOfYVelo = Mathf.Sin(theta) * Velocity.y;

            float xComponentOfXVelo = Mathf.Cos(gamma) * Velocity.x;
            float yComponentOfXVelo = Mathf.Sin(gamma) * Velocity.x;

            AirborneVelocity = new Vector2(xComponentOfXVelo + xComponentOfYVelo, yComponentOfXVelo + yComponentOfYVelo);

            // Reset ground velocities
            Velocity.x = 0f;
            Velocity.y = 0f;

            //Debug.Log("Initial velo off ground: (" + AirborneVelocity.x + ", " + AirborneVelocity.y + ")");

            RecalculateLandingPos();
        } else if (!IsGrounded && predictNorm == new Vector3() && predictPos == new Vector3()) {
            // Ideally this never gets called because the player should never detach
            // from a surface unless they jump
            RecalculateLandingPos();
        } else if (recalculateLanding) {
            RecalculateLandingPos();
        }
        */
    }

    void RecalculateLandingPos() {
        /*
        // Predict the landing spot 
        RaycastHit2D predictHit = new RaycastHit2D();
        RaycastHit2D predictHit2 = new RaycastHit2D();
        Vector2 pos = transform.position - (transform.up * 0.7f);
        Vector2 pos2 = transform.position + (transform.up * 0.7f);

        Vector2 velo = new Vector2(AirborneVelocity.x, AirborneVelocity.y) * Time.fixedDeltaTime * fFactor;

        int count = 0;
        while ((predictHit.collider == null && predictHit2.collider == null) && count < 100) {

            // Generate new ray
            Ray2D ray = new Ray2D(pos, velo.normalized);
            Ray2D ray2 = new Ray2D(pos2, velo.normalized);

            Color randColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);

            Debug.DrawRay(ray.origin, ray.direction * velo.magnitude, randColor, 2f);
            Debug.DrawRay(ray2.origin, ray2.direction * velo.magnitude, randColor, 2f);

            // Update predictHit
            predictHit = Physics2D.Raycast(ray.origin, ray.direction, velo.magnitude, Mask);
            predictHit2 = Physics2D.Raycast(ray2.origin, ray2.direction, velo.magnitude, Mask);

            // Update position to end of predictHit ray
            pos += (ray.direction * velo.magnitude);
            pos2 += (ray2.direction * velo.magnitude);

            velo += (Vector2.down * movementProperties.gravity * Time.fixedDeltaTime);

            count++;
        }

        // If landing position is spotted
        if (predictHit.collider != null) {
            // Set the predicted landing position
            predictPos = predictHit.point;

            // And the predicted landing normal
            predictNorm = predictHit.normal;
        } else if (predictHit2.collider != null) {
            // Set the predicted landing position
            predictPos = predictHit2.point;

            // And the predicted landing normal
            predictNorm = predictHit2.normal;
        } else {
            //Debug.Log("Hit the max count");
        }
        */
    }

    #endregion
}
