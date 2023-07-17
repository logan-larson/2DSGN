using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using static ModeManager;

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

    public float lerpValue = 0.1f;

    [SerializeField]
    private Sprite _quadSprite;
    [SerializeField]
    private Sprite _biSprite;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private SpriteRenderer _damagedSpriteRenderer;



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

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _modeManager = GetComponent<ModeManager>();

        _modeManager.OnChangeToParkour.AddListener(OnChangeToParkourMode);
        _modeManager.OnChangeToCombat.AddListener(OnChangeToCombatMode);

        _spriteRenderer.sprite = _quadSprite;
        _damagedSpriteRenderer.sprite = _quadSprite;
    }

    private void OnChangeToCombatMode()
    {
        _spriteRenderer.sprite = _biSprite;
        _damagedSpriteRenderer.sprite = _biSprite;
        OnChangeToCombatModeServer();
    }

    [ServerRpc]
    private void OnChangeToCombatModeServer()
    {
        _spriteRenderer.sprite = _biSprite;
        _damagedSpriteRenderer.sprite = _biSprite;
        OnChangeToCombatModeObservers();
    }

    [ObserversRpc]
    private void OnChangeToCombatModeObservers()
    {
        _spriteRenderer.sprite = _biSprite;
        _damagedSpriteRenderer.sprite = _biSprite;
    }

    private void OnChangeToParkourMode()
    {
        _spriteRenderer.sprite = _quadSprite;
        _damagedSpriteRenderer.sprite = _quadSprite;
        OnChangeToParkourModeServer();
    }

    [ServerRpc]
    private void OnChangeToParkourModeServer()
    {
        _spriteRenderer.sprite = _quadSprite;
        _damagedSpriteRenderer.sprite = _quadSprite;
        OnChangeToParkourModeObservers();
    }

    [ObserversRpc]
    private void OnChangeToParkourModeObservers()
    {
        _spriteRenderer.sprite = _quadSprite;
        _damagedSpriteRenderer.sprite = _quadSprite;
    }
}
