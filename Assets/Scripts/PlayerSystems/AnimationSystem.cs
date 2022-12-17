using FishNet.Object;
using UnityEngine;

[RequireComponent (typeof(PlayerVelocity))]
[RequireComponent (typeof(PlayerAnimation))]
[RequireComponent (typeof(PlayerMode))]
[RequireComponent (typeof(PlayerGrounded))]
public class AnimationSystem : NetworkBehaviour {

    PlayerPosition position;
    PlayerVelocity velocity;
    PlayerMode mode;
    PlayerGrounded grounded;
    PlayerAnimation playerAnimation;

    Animator _animator;

    // Child game objects
    GameObject staticAnimations;

    // Bones
    GameObject boneBody;

    // Child sprite
    //public Transform childSprite;

    public float lerpValue = 0.1f;

    private MovementSystem _movementSystem;
    private PlayerInputValues _playerInput;


    /* States */
    private static readonly int Idle_Parkour = Animator.StringToHash("Idle_Parkour");
    private static readonly int Idle_Combat = Animator.StringToHash("Idle_Combat");
    private static readonly int Walking_Parkour = Animator.StringToHash("Walking_Parkour");
    private static readonly int Walking_Combat = Animator.StringToHash("Walking_Combat");
    private static readonly int Running_Parkour = Animator.StringToHash("Running_Parkour");
    private static readonly int Jump_Parkour = Animator.StringToHash("Jump_Parkour");
    private static readonly int Jump_Combat = Animator.StringToHash("Jump_Combat");

    private bool _idleParkour;
    private bool _idleCombat;
    private bool _walkingParkour;
    private bool _walkingCombat;
    private bool _runningParkour;
    private bool _jumpParkour;
    private bool _jumpCombat;

    private int _currentState;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _movementSystem = GetComponent<MovementSystem>();
        _playerInput = GetComponent<PlayerInputValues>();
    }

    void Start() { // public void OnStart
        position = GetComponent<PlayerPosition>();
        velocity = GetComponent<PlayerVelocity>();
        playerAnimation = GetComponent<PlayerAnimation>();
        mode = GetComponent<PlayerMode>();
        grounded = GetComponent<PlayerGrounded>();

        //staticAnimations = GameObject.Find("PlayerAnimation");
        boneBody = GameObject.Find("Bone_Body");

        //animator = staticAnimations.GetComponent<Animator>();
        //animator = GetComponentInChildren<Animator>();

        //staticAnimations.transform.position = transform.position;
    }

    /*
    public void SetSpeed(float speed) {
        _animator.SetFloat("speed", speed);
    }

    public void SetInCombat(bool inCombat) {
        _animator.SetBool("inCombat", inCombat);
    }

    public void SetIsJumping(bool isJumping) {
        _animator.SetBool("isJumping", isJumping);
    }
    */

    private int GetState()
    {
        /*
        if (_movementSystem.InCombatMode)
        {
            if (!_movementSystem.IsGrounded) return Jump_Combat;

            if (_movementSystem.Velocity != Vector2.zero) return  Walking_Combat;

            return Idle_Combat;
        }
        else if (_movementSystem.InParkourMode)
        {
            if (!_movementSystem.IsGrounded) return Jump_Parkour;

            if (_movementSystem.Velocity != Vector2.zero) return _playerInput.isSprintKeyPressed ? Running_Parkour : Walking_Parkour;

            return Idle_Parkour;
        }
        */

        return _currentState;
    }

    private void Update()
    { // public void OnUpdate
        if (!base.IsOwner) return;

        /*
        if (_movementSystem.Velocity.x > 1f || _movementSystem.AirborneVelocity.x > 1f) {
            //_animator.CrossFade("Right", 0f, 0);
            _animator.SetBool("IsFacingRight", true);
        } else if (_movementSystem.Velocity.x < -1f || _movementSystem.AirborneVelocity.x < -1f) {
            //childSprite.localScale = new Vector3(-1f, 1f, 1f);
            //_animator.CrossFade("Left", 0f, 0);
            _animator.SetBool("IsFacingRight", false);
        }
        //childSprite.localScale = _movementSystem.Velocity.x > 1f ? new Vector3(1f, 1f, 1f) : new Vector3(-1f, 1f, 1f);


        var state = GetState();

        if (state == _currentState) return;

        //Debug.Log("State: " + state);

        _animator.CrossFade(state, 0f, 0);

        _currentState = state;
        */

        /*
        if (_movementSystem.InCombatMode)
        {
            if (_movementSystem.AirborneVelocity != Vector2.zero)
            {
                _animator.CrossFade("", );
            }

            if (_movementSystem.Velocity != Vector2.zero)
            {
                _animator.CrossFade("", );
            }

        }
        */

        /*
        SetSpeed(Mathf.Abs(velocity.x));

        SetInCombat(mode.inCombatMode);

        SetIsJumping(!grounded.isGrounded);

        if (velocity.x > 1f || velocity.veloOffGround.x > 1f) {
            childSprite.localScale = new Vector3(1f, 1f, 1f);
        } else if (velocity.x < -1f || velocity.veloOffGround.x < -1f) {
            childSprite.localScale = new Vector3(-1f, 1f, 1f);
        }
        */

        /*
        if (velocity.x > 1f || velocity.veloOffGround.x > 1f) {
            playerAnimation.isFacingLeft = false;
        } else if (velocity.x < -1f || velocity.veloOffGround.x < -1f) {
            playerAnimation.isFacingLeft = true;
        }

        // Update animation position based on player velocity
        Vector2 vectorInPixels = new Vector2(
            Mathf.RoundToInt(position.x * 32),
            Mathf.RoundToInt(position.y * 32)
        );

        //return vectorInPixels / 32;
        Vector2 posInUnits = vectorInPixels / 32;

        Vector2 lerpedPos = Vector2.Lerp(staticAnimations.transform.position, posInUnits, lerpValue);

        // Adjust the targeted rotation by 90 degrees if in combat mode
        Quaternion tRot = mode.inCombatMode ? transform.rotation * Quaternion.Euler(0f, 0f, 90f) : transform.rotation;
        Quaternion lerpedRot = Quaternion.Lerp(staticAnimations.transform.rotation, tRot, lerpValue);

        staticAnimations.transform.SetPositionAndRotation(new Vector3(lerpedPos.x, lerpedPos.y), lerpedRot);
            
        if (playerAnimation.isFacingLeft == true) {
            if (mode.inParkourMode) {
                staticAnimations.transform.localScale = new Vector3(-1f, 1f, 1f);
            } else if (mode.inCombatMode) {
                staticAnimations.transform.localScale = new Vector3(1f, -1f, 1f);
            }
        } else {
            staticAnimations.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        */
    }
}
