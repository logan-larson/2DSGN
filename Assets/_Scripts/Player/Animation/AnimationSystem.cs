using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

/**
<summary>
AnimationSystem is used to handle the player's animation state.
</summary>
*/
public class AnimationSystem : NetworkBehaviour
{

    Animator _animator;

    // Child game objects
    GameObject staticAnimations;

    // Bones
    GameObject boneBody;

    // Child sprite
    //public Transform childSprite;

    public float lerpValue = 0.1f;

    [SerializeField]
    private SpriteRenderer _quad;
    [SerializeField]
    private SpriteRenderer _bi;

    [SerializeField]
    private Sprite _quadSprite;
    [SerializeField]
    private Sprite _biSprite;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private int _currentMode = 0;



    private PlayerInputValues _playerInput;
    private ModeManager _modeManager;


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


    [SyncVar(OnChange = nameof(OnChangeMode))]
    public int Mode;

    private void OnChangeMode(int oldValue, int newValue, bool isServer)
    {
        if (newValue == 0) // Parkour
        {
            _spriteRenderer.sprite = _quadSprite;
            // _quad.enabled = true;
            // _bi.enabled = false;
        }
        else if (newValue == 1) // Combat
        {
            _spriteRenderer.sprite = _biSprite;
            // _quad.enabled = false;
            // _bi.enabled = true;
        }
    }

    /*
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _movementSystem = GetComponent<MovementSystem>();
    }
    */

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _modeManager = GetComponent<ModeManager>();

        _modeManager.OnChangeToParkour.AddListener(OnChangeToParkourMode);
        _modeManager.OnChangeToCombat.AddListener(OnChangeToCombatMode);

        SetMode(0);
    }

    private void OnChangeToCombatMode()
    {
        SetMode(1);
    }

    private void OnChangeToParkourMode()
    {
        SetMode(0);
    }

    void Start()
    { // public void OnStart

        //staticAnimations = GameObject.Find("PlayerAnimation");

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

    private int GetMode()
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

        return _currentMode;
    }

    [ServerRpc]
    public void SetMode(int mode)
    {
        Mode = mode;
    }

    private void Update()
    { // public void OnUpdate
        if (!base.IsOwner) return;

        /*
        if (_movementSystem.PublicData.InCombatMode)
        {
            // _bi.enabled = true;
            // _quad.enabled = false;
        }
        else if (_movementSystem.PublicData.InParkourMode)
        {
            // _bi.enabled = false;
            // _quad.enabled = true;
        }
        */
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
