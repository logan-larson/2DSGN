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
        public bool Sprint;
        /*
        public bool Jump;
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
        public Vector3 Position;
        public Vector3 Velocity;
        public Quaternion Rotation;
        public bool IsGrounded;
        public Vector2 AirborneVelocity;
        /*
        public bool InParkourMode;
        public bool InCombatMode;
        */
    }

    /// <summary>
    /// The origins of the raycasts used to determine if the player is grounded.
    /// </summary>
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    #endregion


    #region Script References

    [HideInInspector]
    public PlayerInputValues Input;

    [SerializeField]
    private PlayerMovementProperties MovementProperties;

    #endregion


    #region Public Variables // All commented out for now.

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

    */

    #endregion


    #region Private Variables // _currentVelocity, _subscribedToTimeManager, _raycastOrigins

    /// <summary>
    /// The current velocity of the player.
    /// </summary>
    private Vector3 _currentVelocity = new Vector3();

    /// <summary>
    /// The current airborne velocity of the player.
    /// </summary>
    private Vector3 _currentAirborneVelocity = new Vector3();

    /// <summary>
    /// The current rotation of the player.
    /// </summary>
    //private Quaternion _currentRotation = Quaternion.identity;

    /// <summary>
    /// True if subscribed to the TimeManager.
    /// </summary>
    private bool _subscribedToTimeManager = false;

    /// <summary>
    /// The raycast origins of the player.
    /// </summary>
    private RaycastOrigins _raycastOrigins;

    /// <summary>
    /// True if the player is currently grounded.
    /// </summary>
    private bool _isGrounded = false;

    /// <summary>
    /// The distance from the player to the ground.
    /// </summary>
    private float _groundDistance = 0f;

    /* JumpNow, PredictPos, PredictNorm, and RecalculateLanding are all commented out for now.
    /// <summary>
    /// True if the player is currently jumping.
    /// </summary>
    private bool jumpNow = false;

    //private Vector3 predictPos = new Vector3();
    //private Vector3 predictNorm = new Vector3();

    //private bool recalculateLanding = false;
    */

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
                Rotation = transform.rotation,
                /*
                AirborneVelocity = AirborneVelocity,
                IsGrounded = IsGrounded,
                InParkourMode = InParkourMode,
                InCombatMode = InCombatMode
                */
            };

            Reconciliation(reconcileData, true);
        }
    }

    /// <summary>
    /// Use player input to build move data that will be used in the move function.
    /// </summary>
    /// <param name="moveData"></param>
    private void BuildActions(out MoveData moveData)
    {
        moveData = default;

        //if (moveData.Horizontal == Input.HorizontalMovementInput) return;

        moveData.Horizontal = Input.HorizontalMovementInput;
        moveData.Sprint = Input.IsSprintKeyPressed;
        /*
        moveData.Jump = input.isJumpKeyPressed;

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

    /// <summary>
    /// Move the player using the provided moveData.
    /// This function is replicated on both the server and client.
    /// </summary>
    /// <param name="moveData"></param>
    /// <param name="asServer"></param>
    /// <param name="replaying"></param>
    [Replicate]
    private void Move(MoveData moveData, bool asServer, bool replaying = false)
    {
        //UpdateMode(moveData);

        UpdateRaycastOrigins();

        //if (movementProperties.timeSinceGrounded > minJumpTime)  {
            UpdateGrounded();
        //} else {
            //movementProperties.timeSinceGrounded += Time.fixedDeltaTime;
        //}

        UpdateVelocity(moveData);


        UpdatePosition();
            
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

    /// <summary>
    /// Reconcile the player's data.
    /// This function is only called on the client.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="asServer"></param>
    [Reconcile]
    private void Reconciliation(ReconcileData data, bool asServer)
    {
        transform.position = data.Position;
        _currentVelocity = data.Velocity;
        transform.rotation = data.Rotation;
        _currentAirborneVelocity = data.AirborneVelocity;
        _isGrounded = data.IsGrounded;
        /*
        InParkourMode = data.InParkourMode;
        InCombatMode = data.InCombatMode;
        */
    }

    /* Fixed Update is commented out for reference
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
		_raycastOrigins.bottomLeft = transform.position - (transform.right / 2) - (transform.up / 2);
		_raycastOrigins.bottomRight = transform.position + (transform.right / 2) - (transform.up / 2);
		_raycastOrigins.topLeft = transform.position - (transform.right / 2) + (transform.up / 2);
		_raycastOrigins.topRight = transform.position + (transform.right / 2) + (transform.up / 2);
    }

    void UpdateGrounded() {
        if (_isGrounded) {
            // I think I multiply by 1.25f to give the player a little bit of leeway for being grounded
            // TODO: Validate the statement above
            RaycastHit2D groundedHit = Physics2D.Raycast(transform.position, -transform.up, MovementProperties.GroundedHeight * 1.25f, MovementProperties.ObstacleMask);
            _isGrounded = groundedHit.collider != null;
            _groundDistance = groundedHit.distance;
        } else {
            RaycastHit2D groundedHit = Physics2D.Raycast(transform.position, -transform.up, MovementProperties.GroundedHeight, MovementProperties.ObstacleMask);
            _isGrounded = groundedHit.collider != null;
            _groundDistance = groundedHit.distance;
        }
    }

    public void Jump() {
        /*
        if (IsGrounded) {
            jumpNow = true;
        }
        */
    }

    private void UpdateVelocity(MoveData moveData = new MoveData())
    {

        // Add velo based on horizontal input, increase top speed when sprint key is pressed
        
        float sprintMultiplier = moveData.Sprint ? MovementProperties.SprintMultiplier : 1f;

        if (moveData.Horizontal != 0f)
        {
            _currentVelocity += new Vector3(moveData.Horizontal, 0f, 0f) * MovementProperties.Acceleration * sprintMultiplier;
        }
        else
        {
            _currentVelocity = Vector3.MoveTowards(_currentVelocity, Vector3.zero, MovementProperties.Friction);
        }

        if (_currentVelocity.magnitude > MovementProperties.MaxSpeed * sprintMultiplier) {
            _currentVelocity = _currentVelocity.normalized * MovementProperties.MaxSpeed * sprintMultiplier;
        }


        if (_isGrounded)
        {
            /*
            if (jumpNow)
            {
                jumpNow = false;
                IsGrounded = false;
                movementProperties.timeSinceGrounded = 0f;
                Velocity.y = movementProperties.jumpVelocity;
            }
            else
            */
            {
                if (_groundDistance < 0.95f) {
                    _currentVelocity.y += 1f;
                    //MovementProperties.Gravity; 
                } else if (_groundDistance > 1.05f) {
                    _currentVelocity.y -= 1f;
                    //MovementProperties.Gravity; 
                } else {
                    _currentVelocity.y = 0f;
                }
            }
            _currentAirborneVelocity = Vector3.zero;
        }
        else
        {
            _currentAirborneVelocity += (Vector3.down * MovementProperties.Gravity * (float) TimeManager.TickDelta);

            // This is where airborne movement forces can be applied

            // If forces were applied then we need to recalculate the landing
            //recalculateLanding = false;
        }

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

    private void UpdatePosition()
    {
        if (_isGrounded)
        {
            UpdatePositionRelativeToSelf();
        }
        else
        {
            UpdatePositionRelativeToWorld();
        }
    }

    private void UpdatePositionRelativeToSelf()
    {
        Vector2 velocity = new Vector2(_currentVelocity.x, _currentVelocity.y);

        Ray2D leftRay = new Ray2D(_raycastOrigins.bottomLeft + (velocity * (float) TimeManager.TickDelta), -transform.up);
        Ray2D rightRay = new Ray2D(_raycastOrigins.bottomRight + (velocity * (float) TimeManager.TickDelta), -transform.up);

        RaycastHit2D leftHit = Physics2D.Raycast(leftRay.origin, leftRay.direction, MovementProperties.GroundedHeight, MovementProperties.ObstacleMask);
        RaycastHit2D rightHit = Physics2D.Raycast(rightRay.origin, rightRay.direction, MovementProperties.GroundedHeight, MovementProperties.ObstacleMask);

        // Use override hit to prevent clipping
        RaycastHit2D overrideHit = Physics2D.Raycast((leftRay.origin + rightRay.origin) / 2, transform.right, MovementProperties.OverrideRayLength * Mathf.Sign(_currentVelocity.x), MovementProperties.ObstacleMask);
        if (_currentVelocity.x < 0f && overrideHit.collider != null) {
            leftHit = overrideHit;
        } else if (_currentVelocity.x > 0f && overrideHit.collider != null) {
            rightHit = overrideHit;
        }

        // Apply rotation to orient body to match ground
        Quaternion finalRotation = transform.rotation;
        if (leftHit && rightHit) {
            Vector2 avgNorm = (leftHit.normal + rightHit.normal) / 2;

            finalRotation = Quaternion.FromToRotation(Vector3.up, avgNorm);
        }

        Vector3 finalPosition = transform.position + (finalRotation * _currentVelocity) * (float) TimeManager.TickDelta;

        transform.SetPositionAndRotation(finalPosition, finalRotation);
    }

    private void UpdatePositionRelativeToWorld()
    {
        transform.position += _currentAirborneVelocity * (float) TimeManager.TickDelta;
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
