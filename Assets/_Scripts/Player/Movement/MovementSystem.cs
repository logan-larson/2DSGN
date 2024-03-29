using System;
using System.Collections;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;

/**
<summary>
MovementSystem is responsible for handling all movement related actions.
</summary>
*/
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
        public bool IsRespawning; // Set by the RespawnManager
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
        public bool InParkourMode;
        public bool InCombatMode;
        public float TimeOnGround;
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
        public Vector3 Position;
        public Vector3 Velocity;
        //public Quaternion Rotation;
        public bool IsGrounded;
        public bool InParkourMode;
        public bool InCombatMode;
    }

    public PublicMovementData PublicData;

    #endregion


    #region Script References

    [SerializeField]
    private InputSystem InputSystem;

    [SerializeField]
    private PlayerInputValues _input;

    [SerializeField]
    private PlayerHealth _playerHealth;

    [SerializeField]
    private RespawnManager _respawnManager;

    [SerializeField]
    private PlayerMovementProperties MovementProperties;

    #endregion


    #region Private Variables

    /// <summary>
    /// The current airborne velocity of the player.
    /// </summary>
    public Vector3 _currentVelocity = new Vector3();

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
    [SerializeField]
    private float _timeSinceGrounded = 0f;

    /// <summary>
    /// Time since player has been grounded, used for jumping and re-enabling grounded.
    /// </summary>
    [SerializeField]
    private float _timeOnGround = 0f;

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

    private IEnumerator _recalculateLandingCoroutine;
    private bool _recalculateLandingCoroutineIsRunning;

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

    /// <summary>
    /// Set to true when the player is respawning.
    /// </summary>
    [SerializeField]
    private bool _isRespawning = false;


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
        InputSystem = InputSystem ?? GetComponent<InputSystem>();
        _playerHealth = _playerHealth ?? GetComponent<PlayerHealth>();
        _respawnManager = _respawnManager ?? GetComponent<RespawnManager>();
    }

    private void Start()
    {
        _input = InputSystem.InputValues;

        if (_input == null)
            Debug.LogError("InputValues not found on InputSystem.");
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
                /* Copy by value */
                Position = new Vector3(transform.position.x, transform.position.y, transform.position.z),
                Velocity = new Vector3(_currentVelocity.x, _currentVelocity.y, _currentVelocity.z),
                Rotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w),
                IsGrounded = _isGrounded,
                InParkourMode = _inParkourMode,
                InCombatMode = _inCombatMode,
                TimeOnGround = _timeOnGround
            };

            Reconciliation(reconcileData, true);
        }


        SetPublicMovementData();
    }

    /// <summary>
    /// Use player input to build move data that will be used in the move function.
    /// </summary>
    /// <param name="moveData"></param>
    private void BuildActions(out MoveData moveData)
    {
        moveData = default;

        moveData.Horizontal = _input.HorizontalMovementInput;
        moveData.Sprint = _input.IsSprintKeyPressed;
        moveData.Jump = _input.IsJumpKeyPressed;
        moveData.ChangeToCombat = _input.IsFirePressed;
        if (!moveData.ChangeToCombat)
            moveData.ChangeToParkour = _input.IsSprintKeyPressed;

        moveData.IsRespawning = _isRespawning;
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

        UpdateGrounded();

        UpdateVelocity(moveData, asServer);

        UpdatePosition(moveData);
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
        transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
        transform.rotation = new Quaternion(data.Rotation.x, data.Rotation.y, data.Rotation.z, data.Rotation.w);
        _currentVelocity = new Vector3(data.Velocity.x, data.Velocity.y, data.Velocity.z);
        _isGrounded = data.IsGrounded;
        _inParkourMode = data.InParkourMode;
        _inCombatMode = data.InCombatMode;
        _timeOnGround = data.TimeOnGround;

        if (_isRespawning)
        {
            transform.rotation = Quaternion.identity;
            _currentVelocity = Vector3.zero;
        }
    }

    // TODO: this is a fucking hacky way to do this
    public void SetIsRespawning(bool isRespawning)
    {
        if (!isRespawning)
        {
            StartCoroutine(DelayRespawnCoroutine());
        }
        else
        {
            _isRespawning = true;
        }
    }

    private IEnumerator DelayRespawnCoroutine()
    {
        yield return new WaitForSeconds(0.25f);
        _isRespawning = false;
    }

    private void UpdateMode(MoveData moveData = new MoveData())
    {
        if (moveData.ChangeToParkour)
        {
            if (_inCombatMode)
            {
                _inCombatMode = false;
                _inParkourMode = true;
            }
        }

        if (moveData.ChangeToCombat)
        {
            if (_inParkourMode)
            {
                _inParkourMode = false;
                _inCombatMode = true;
            }
        }
    }

    private void UpdateRaycastOrigins()
    {
        _raycastOrigins.bottomLeft = transform.position - (transform.right / 2) - (transform.up / 2);
        _raycastOrigins.bottomRight = transform.position + (transform.right / 2) - (transform.up / 2);
        _raycastOrigins.topLeft = transform.position - (transform.right / 2) + (transform.up / 2);
        _raycastOrigins.topRight = transform.position + (transform.right / 2) + (transform.up / 2);
    }

    private void UpdateGrounded()
    {
        if (_isGrounded && _timeOnGround <= MovementProperties.MinimumJumpTime * 2f)
            _timeOnGround += (float)TimeManager.TickDelta;
        else if (!_isGrounded)
            _timeOnGround = 0f;

        // If the player is currently grounded, check if they are still grounded and set ground distance
        if (_isGrounded || _timeSinceGrounded > MovementProperties.MinimumJumpTime)
        {
            RaycastHit2D groundedHit = Physics2D.Raycast(transform.position, -transform.up, MovementProperties.GroundedHeight, MovementProperties.ObstacleMask);
            _isGrounded = groundedHit.collider != null;
            _groundDistance = groundedHit.distance;
            if (_isGrounded && _timeOnGround > MovementProperties.MinimumJumpTime * 2f)
                _canJump = true;
        }
        // Otherwise, increase the time since grounded
        else
        {
            _timeSinceGrounded += (float)TimeManager.TickDelta;
        }
    }

    private void UpdateVelocity(MoveData moveData, bool asServer)
    {


        // Increase top speed when sprint key is pressed
        float sprintMultiplier = moveData.Sprint && !_inCombatMode ? MovementProperties.SprintMultiplier : 1f;
        // Set top speed based on mode
        float modeMultiplier = _inParkourMode ? MovementProperties.ParkourMultiplier : MovementProperties.CombatMultiplier;

        // If grounded, change velocity
        if (_isGrounded)
        {
            // If horizontal input is given, add velocity
            if (moveData.Horizontal != 0f)
                _currentVelocity += transform.right * moveData.Horizontal * MovementProperties.Acceleration * sprintMultiplier * modeMultiplier;
            // If no horizontal input is given, decrease velocity by friction
            else
                _currentVelocity = Vector3.MoveTowards(_currentVelocity, Vector3.zero, MovementProperties.Friction);
        }

        // Limit top speed
        float maxSpeed = _isGrounded ? MovementProperties.MaxSpeed : MovementProperties.MaxAirborneSpeed;
        if (_isGrounded && _currentVelocity.magnitude > maxSpeed * sprintMultiplier * modeMultiplier)
        {
            _currentVelocity = _currentVelocity.normalized * maxSpeed * sprintMultiplier * modeMultiplier;
        }

        if (_isGrounded)
        {
            // Jump
            if (moveData.Jump && _canJump)
            {
                _canJump = false;
                _isGrounded = false;
                _timeSinceGrounded = 0f;

                _currentVelocity += transform.up * MovementProperties.JumpVelocity;

                RecalculateLandingPosition();

                _recalculateLandingCoroutine = RecalculateNextLandingCoroutine();

                _recalculateLandingCoroutineIsRunning = true;

                StartCoroutine(_recalculateLandingCoroutine);
            }
            // Set height relative to ground
            else
            {
                if (_recalculateLandingCoroutineIsRunning)
                {
                    StopCoroutine(_recalculateLandingCoroutine);
                    _recalculateLandingCoroutineIsRunning = false;
                }
            }
        }
        else
        {
            // Apply gravity
            _currentVelocity += (Vector3.down * MovementProperties.Gravity * (float)TimeManager.TickDelta);

            // This is where airborne movement forces can be applied

            // If forces were applied then we need to recalculate the landing
            //recalculateLanding = false;
        }

        //CheckNeedRecalc();

        // If we are not grounded and we have not predicted a landing position then we need to calculate it
        // Or if we manually trigger a recalculation
        if ((!_isGrounded && _predictedNormal == Vector3.zero && _predictedPosition == Vector3.zero) || _recalculateLanding)
        {
            RecalculateLandingPosition();
        }
    }

    private IEnumerator RecalculateNextLandingCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            Vector2? normal = RecalculateNextLandingNormal();

            if (normal != null)
            {
                Vector2 diff = (Vector2)_predictedNormal - (Vector2)normal;

                if (diff.magnitude > 0.1f)
                {
                    _predictedNormal = (Vector2)normal;
                }
                else
                {
                    yield break;
                }
            }
        }

    }

    private void UpdatePosition(MoveData moveData)
    {
        Vector3 changeGround = Vector3.zero;

        if (_isGrounded)
        {
            changeGround = transform.up * (1f - _groundDistance);
        }

        Vector3 finalPosition = (transform.position + changeGround) + _currentVelocity * (float)TimeManager.TickDelta;


        if (_isGrounded)
        {
            Vector2 velocity = new Vector2(_currentVelocity.x, _currentVelocity.y);

            Ray2D leftRay = new Ray2D(_raycastOrigins.bottomLeft + (velocity * (float)TimeManager.TickDelta), -transform.up);
            Ray2D rightRay = new Ray2D(_raycastOrigins.bottomRight + (velocity * (float)TimeManager.TickDelta), -transform.up);

            RaycastHit2D leftHit = Physics2D.Raycast(leftRay.origin, leftRay.direction, MovementProperties.GroundedHeight, MovementProperties.ObstacleMask);
            RaycastHit2D rightHit = Physics2D.Raycast(rightRay.origin, rightRay.direction, MovementProperties.GroundedHeight, MovementProperties.ObstacleMask);

            // Use override hit to prevent clipping
            RaycastHit2D overrideHit = Physics2D.Raycast((leftRay.origin + rightRay.origin) / 2, transform.right, MovementProperties.OverrideRayLength * Mathf.Sign(_currentVelocity.x), MovementProperties.ObstacleMask);
            if (_currentVelocity.x < 0f && overrideHit.collider != null)
            {
                leftHit = overrideHit;
            }
            else if (_currentVelocity.x > 0f && overrideHit.collider != null)
            {
                rightHit = overrideHit;
            }

            // Apply rotation to orient body to match ground
            Quaternion finalRotation = transform.rotation;
            if (leftHit && rightHit)
            {
                Vector2 avgNorm = (leftHit.normal + rightHit.normal) / 2;

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, avgNorm);
                finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, MovementProperties.MaxRotationDegrees);
            }

            transform.SetPositionAndRotation(finalPosition, finalRotation);
        }
        else
        {
            // And rotate to the predicted landing spots normal
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, _predictedNormal);
            Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, MovementProperties.MaxRotationDegrees);

            transform.SetPositionAndRotation(finalPosition, finalRotation);
        }

        if (moveData.IsRespawning)
        {
            transform.SetPositionAndRotation(finalPosition, Quaternion.identity);
        }
    }

    private void RecalculateLandingPosition()
    {

        _recalculateLanding = false;

        // Predict the landing spot 
        RaycastHit2D predictHit = new RaycastHit2D();
        RaycastHit2D predictHit2 = new RaycastHit2D();
        Vector2 pos = transform.position - (transform.up * 0.7f);
        Vector2 pos2 = transform.position + (transform.up * 0.7f);

        Vector2 velo = new Vector2(_currentVelocity.x, _currentVelocity.y) * Time.fixedDeltaTime * MovementProperties.FFactor;

        int count = 0;
        while ((predictHit.collider == null && predictHit2.collider == null) && count < 100)
        {

            // Generate new ray
            Ray2D ray = new Ray2D(pos, velo.normalized);
            Ray2D ray2 = new Ray2D(pos2, velo.normalized);

            Color randColor = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);

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

        // Set the predicted landing position and normal
        // By nature this will get the closer of the two landing positions
        if (predictHit.collider != null)
        {
            _predictedPosition = predictHit.point;

            _predictedNormal = predictHit.normal;
        }
        else if (predictHit2.collider != null)
        {
            _predictedPosition = predictHit2.point;

            _predictedNormal = predictHit2.normal;
        }
    }

    private Vector2? RecalculateNextLandingNormal()
    {

        _recalculateLanding = false;

        // Predict the landing spot 
        RaycastHit2D predictHit = new RaycastHit2D();
        RaycastHit2D predictHit2 = new RaycastHit2D();
        Vector2 pos = transform.position - (transform.up * 0.7f);
        Vector2 pos2 = transform.position + (transform.up * 0.7f);

        Vector2 velo = new Vector2(_currentVelocity.x, _currentVelocity.y) * Time.fixedDeltaTime * MovementProperties.FFactor;

        int count = 0;
        while ((predictHit.collider == null && predictHit2.collider == null) && count < 100)
        {

            // Generate new ray
            Ray2D ray = new Ray2D(pos, velo.normalized);
            Ray2D ray2 = new Ray2D(pos2, velo.normalized);

            Color randColor = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);

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

        // Set the predicted landing position and normal
        // By nature this will get the closer of the two landing positions
        if (predictHit.collider != null)
        {
            //_predictedPosition = predictHit.point;

            return predictHit.normal;
        }
        else if (predictHit2.collider != null)
        {
            //_predictedPosition = predictHit2.point;

            return predictHit2.normal;
        }
        
        return null;
    }

    private void SetPublicMovementData()
    {
        PublicData.Position = transform.position;
        PublicData.Velocity = _currentVelocity;
        PublicData.IsGrounded = _isGrounded;
        PublicData.InParkourMode = _inParkourMode;
        PublicData.InCombatMode = _inCombatMode;
    }

    #endregion

}
