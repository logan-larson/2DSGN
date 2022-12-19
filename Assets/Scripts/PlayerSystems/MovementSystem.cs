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
        public bool Jump;
        public bool ChangeToCombat; // Fire key
        public bool ChangeToParkour; // Sprint key
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
        public bool InParkourMode;
        public bool InCombatMode;
    }

    /// <summary>
    /// The origins of the raycasts used to determine if the player is grounded.
    /// </summary>
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    public struct PublicMovementData
    {
        //public Vector3 Position;
        public Vector3 Velocity;
        //public Quaternion Rotation;
        public bool IsGrounded;
        public Vector2 AirborneVelocity;
    }

    public PublicMovementData PublicData;

    #endregion


    #region Script References

    [HideInInspector]
    public PlayerInputValues Input;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private PlayerMovementProperties MovementProperties;

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

    /// <summary>
    /// Time since player has been grounded, used for jumping and re-enabling grounded.
    /// </summary>
    private float _timeSinceGrounded = 0f;

    /// <summary>
    /// True if the player is allowed to jump.
    /// </summary>
    private bool _canJump = true;

    /// <summary>
    /// The predicted landing position for airborne player.
    /// </summary>
    private Vector3 _predictedPosition = new Vector3();

    /// <summary>
    /// The predicted normal of surface of landing position for airborne player.
    /// </summary>
    private Vector3 _predictedNormal = new Vector3();

    /// <summary>
    /// True if the predicted landing position and normal should be recalculated.
    /// </summary>
    private bool _recalculateLanding = false;

    /// <summary>
    /// True if the player is in parkour mode.
    /// </summary>
    [SerializeField]
    private bool _inParkourMode = true;

    /// <summary>
    /// True if the player is in combat mode.
    /// </summary>
    [SerializeField]
    private bool _inCombatMode = false;



    /// TODO: Move to combat when ready

    /// <summary>
    /// True if the player is firing.
    /// </summary>
    [SerializeField]
    private bool _isFiring = false;


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

    private void Awake()
    {
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
                AirborneVelocity = _currentAirborneVelocity,
                IsGrounded = _isGrounded,
                InParkourMode = _inParkourMode,
                InCombatMode = _inCombatMode
            };

            Reconciliation(reconcileData, true);
        }

        PerformAnimation();

        SetPublicMovementData();
    }

    public void Fire()
    {
        _isFiring = true;
    }

    /// <summary>
    /// Use player input to build move data that will be used in the move function.
    /// </summary>
    /// <param name="moveData"></param>
    private void BuildActions(out MoveData moveData)
    {
        moveData = default;

        moveData.Horizontal = Input.HorizontalMovementInput;
        moveData.Sprint = Input.IsSprintKeyPressed;
        moveData.Jump = Input.IsJumpKeyPressed;
        moveData.ChangeToCombat = Input.IsFirePressed;
        if (!moveData.ChangeToCombat)
            moveData.ChangeToParkour = Input.IsSprintKeyPressed;
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
        UpdateMode(moveData);

        UpdateRaycastOrigins();

        if (_timeSinceGrounded > MovementProperties.MinimumJumpTime)  {
            UpdateGrounded();
        } else {
            _timeSinceGrounded += (float) TimeManager.TickDelta;
        }

        UpdateVelocity(moveData, asServer, replaying);

        UpdatePosition();
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
        _inParkourMode = data.InParkourMode;
        _inCombatMode = data.InCombatMode;
    }

    private void UpdateMode(MoveData moveData = new MoveData())
    {
        if (moveData.ChangeToParkour)
        {
            _inCombatMode = false;
            _inParkourMode = true;
        }
        
        if (moveData.ChangeToCombat)
        {
            _inCombatMode = true;
            _inParkourMode = false;
        }
    }

    private void UpdateRaycastOrigins() {
		_raycastOrigins.bottomLeft = transform.position - (transform.right / 2) - (transform.up / 2);
		_raycastOrigins.bottomRight = transform.position + (transform.right / 2) - (transform.up / 2);
		_raycastOrigins.topLeft = transform.position - (transform.right / 2) + (transform.up / 2);
		_raycastOrigins.topRight = transform.position + (transform.right / 2) + (transform.up / 2);
    }

    private void UpdateGrounded() {
        if (_isGrounded) {
            // I think I multiply by 1.25f to give the player a little bit of leeway for being grounded
            // TODO: Validate the statement above
            RaycastHit2D groundedHit = Physics2D.Raycast(transform.position, -transform.up, MovementProperties.GroundedHeight * 1.25f, MovementProperties.ObstacleMask);
            _isGrounded = groundedHit.collider != null;
            _groundDistance = groundedHit.distance;
            _canJump = true;
        } else {
            RaycastHit2D groundedHit = Physics2D.Raycast(transform.position, -transform.up, MovementProperties.GroundedHeight, MovementProperties.ObstacleMask);
            _isGrounded = groundedHit.collider != null;
            _groundDistance = groundedHit.distance;
        }
    }

    private void UpdateVelocity(MoveData moveData, bool asServer, bool replaying = false)
    {

        // Add velo based on horizontal input, increase top speed when sprint key is pressed
        
        float sprintMultiplier = moveData.Sprint && !_inCombatMode ? MovementProperties.SprintMultiplier : 1f;
        float modeMultiplier = _inParkourMode ? MovementProperties.ParkourMultiplier : MovementProperties.CombatMultiplier;

        if (moveData.Horizontal != 0f)
        {
            _currentVelocity += new Vector3(moveData.Horizontal, 0f, 0f) * MovementProperties.Acceleration * sprintMultiplier * modeMultiplier;
        }
        else
        {
            _currentVelocity = Vector3.MoveTowards(_currentVelocity, Vector3.zero, MovementProperties.Friction);
        }

        if (_currentVelocity.magnitude > MovementProperties.MaxSpeed * sprintMultiplier * modeMultiplier) {
            _currentVelocity = _currentVelocity.normalized * MovementProperties.MaxSpeed * sprintMultiplier * modeMultiplier;
        }


        if (_isGrounded)
        {
            if (moveData.Jump && _canJump && !replaying)
            {
                _canJump = false;
                _isGrounded = false;
                _timeSinceGrounded = 0f;
                _currentVelocity.y = MovementProperties.JumpVelocity;
            }
            else
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
        

        // This is triggered directly after the jump is initialized
        if (!_isGrounded && _timeSinceGrounded == 0f)
        {
            SetupAirborneMovement(asServer, replaying);
        }
        else if ((!_isGrounded && _predictedNormal == Vector3.zero && _predictedPosition == Vector3.zero) || _recalculateLanding)
        {
            //if (asServer)
                RecalculateLandingPosition();
        }
    }

    private void SetupAirborneMovement(bool asServer, bool replaying = false)
    {
        // Get the velocity directly after the jump because we don't allow
        // airborne movement controls besides using a weapon to move

        // Theta = yAngleFromRight
        float theta = Vector2.SignedAngle(Vector2.right, transform.up) * Mathf.Deg2Rad;
        // Gamma = xAngleFromRight
        float gamma = Vector2.SignedAngle(Vector2.right, transform.right) * Mathf.Deg2Rad;

        float xComponentOfYVelo = Mathf.Cos(theta) * _currentVelocity.y;
        float yComponentOfYVelo = Mathf.Sin(theta) * _currentVelocity.y;

        float xComponentOfXVelo = Mathf.Cos(gamma) * _currentVelocity.x;
        float yComponentOfXVelo = Mathf.Sin(gamma) * _currentVelocity.x;

        _currentAirborneVelocity = new Vector2(xComponentOfXVelo + xComponentOfYVelo, yComponentOfXVelo + yComponentOfYVelo);

        // Reset ground velocity
        _currentVelocity = Vector3.zero;

        //if (asServer)
        if (!replaying)
            RecalculateLandingPosition();
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

            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, avgNorm);
            finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, MovementProperties.MaxRotationDegrees);
        }

        Vector3 finalPosition = transform.position + (finalRotation * _currentVelocity) * (float) TimeManager.TickDelta;

        transform.SetPositionAndRotation(finalPosition, finalRotation);
    }

    private void UpdatePositionRelativeToWorld()
    {
        Vector3 finalPosition = transform.position + _currentAirborneVelocity * (float) TimeManager.TickDelta;

        // And rotate to the predicted landing spots normal
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, _predictedNormal);
        Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, MovementProperties.MaxRotationDegrees);

        transform.SetPositionAndRotation(finalPosition, finalRotation);
        //transform.rotation = Quaternion.Euler(0f, 0f, finalRotation.eulerAngles.z);
        //Rotation = finalRotation.eulerAngles.z;
    }

    private void RecalculateLandingPosition() {
        // Predict the landing spot 
        RaycastHit2D predictHit = new RaycastHit2D();
        RaycastHit2D predictHit2 = new RaycastHit2D();
        Vector2 pos = transform.position - (transform.up * 0.7f);
        Vector2 pos2 = transform.position + (transform.up * 0.7f);

        Vector2 velo = new Vector2(_currentAirborneVelocity.x, _currentAirborneVelocity.y) * Time.fixedDeltaTime * MovementProperties.FFactor;

        int count = 0;
        while ((predictHit.collider == null && predictHit2.collider == null) && count < 100) {

            // Generate new ray
            Ray2D ray = new Ray2D(pos, velo.normalized);
            Ray2D ray2 = new Ray2D(pos2, velo.normalized);

            Color randColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);

            Debug.DrawRay(ray.origin, ray.direction * velo.magnitude, randColor, 2f);
            Debug.DrawRay(ray2.origin, ray2.direction * velo.magnitude, randColor, 2f);

            // Update predictHit
            predictHit = Physics2D.Raycast(ray.origin, ray.direction, velo.magnitude, MovementProperties.ObstacleMask);
            predictHit2 = Physics2D.Raycast(ray2.origin, ray2.direction, velo.magnitude, MovementProperties.ObstacleMask);

            // Update position to end of predictHit ray
            pos += (ray.direction * velo.magnitude);
            pos2 += (ray2.direction * velo.magnitude);

            velo += (Vector2.down * MovementProperties.Gravity * Time.fixedDeltaTime);

            count++;
        }

        // If landing position is spotted
        if (predictHit.collider != null) {
            // Set the predicted landing position
            _predictedPosition = predictHit.point;

            // And the predicted landing normal
            _predictedNormal = predictHit.normal;
        } else if (predictHit2.collider != null) {
            // Set the predicted landing position
            _predictedPosition = predictHit2.point;

            // And the predicted landing normal
            _predictedNormal = predictHit2.normal;
        } else {
            //Debug.Log("Hit the max count");
        }
    }

    private void PerformAnimation()
    {
        if (_currentVelocity.x > 0f)
            _animator.SetBool("IsFacingRight", true);
        else if (_currentVelocity.x < 0f)
            _animator.SetBool("IsFacingRight", false);

        _animator.SetBool("InCombatMode", _inCombatMode);
    }

    private void SetPublicMovementData()
    {
        PublicData.Velocity = _currentVelocity;
        PublicData.AirborneVelocity = _currentAirborneVelocity;
        PublicData.IsGrounded = _isGrounded;
    }

    #endregion
}
