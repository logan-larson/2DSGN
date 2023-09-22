using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputValues))]
[RequireComponent(typeof(MovementSystem))]
public class InputSystem : NetworkBehaviour
{


    // TODO: At some point I think I would like to make these events
    // So other systems can subscribe to them without having
    // to know about the input system

    [HideInInspector]
    public PlayerInputValues InputValues;

    private MovementSystem _movement;
    private CombatSystem _combatSystem;
    private WeaponEquipManager _weaponEquipManager;
    private ModeManager _modeManager;
    private PlayerHealth _playerHealth;
    private RespawnManager _respawnManager;
    private LobbyManager _lobbyManager;
    private ScoreboardManager _scoreboardManager;
    private HealthNutManager _healthNutManager;


    [SerializeField]
    [SyncVar]
    private bool _isEnabled = true;

    /*
    [SyncVar (OnChange = nameof(ChangeIsCursorVisible))]
    private bool _isCursorVisible = false;

    private void ChangeIsCursorVisible(bool oldValue, bool newValue, bool asServer)
    {
        if (!asServer)
            Cursor.visible = newValue;
    }
    */

    private PlayerInput _playerInput;

    private void Awake()
    {
        _movement ??= GetComponent<MovementSystem>();
        _playerInput ??= GetComponent<PlayerInput>();

        _modeManager ??= GetComponent<ModeManager>();
        _playerHealth ??= GetComponent<PlayerHealth>();
        _combatSystem ??= GetComponent<CombatSystem>();

        _playerHealth.OnDeath.AddListener((bool _) => _isEnabled = false);



        // Just uncommented these
        _weaponEquipManager ??= GetComponent<WeaponEquipManager>();
        _healthNutManager ??= GetComponent<HealthNutManager>();
        _lobbyManager ??= GetComponent<LobbyManager>();
        _scoreboardManager ??= GetComponentInChildren<ScoreboardManager>();
        _respawnManager ??= GetComponent<RespawnManager>();

        _respawnManager.OnRespawn.AddListener(() =>
        {
            if (GameStateManager.Instance.CurrentGameState == GameStateManager.GameState.Game)
                _isEnabled = true;
        });




        InputValues = (PlayerInputValues)ScriptableObject.CreateInstance(typeof(PlayerInputValues));
        Cursor.visible = true;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _isEnabled = true;

        GameStateManager.Instance.OnGameEnd.AddListener(() =>
        {
            _isEnabled = false;
            //Cursor.visible = true;
        });
        GameStateManager.Instance.OnGameStart.AddListener(() =>
        {
            _isEnabled = true;
            //Cursor.visible = false;
        });
        GameStateManager.Instance.OnLobbyStart.AddListener(() =>
        {
            _isEnabled = false;
            //Cursor.visible = true;
        });

        if (GameStateManager.Instance.CurrentGameState == GameStateManager.GameState.Game)
            _isEnabled = true;
        else
            _isEnabled = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        //if (!base.IsOwner) enabled = false;
    }

    private void Update()
    {
        if (!_isEnabled) return;

        InputValues.AimInput = _playerInput.actions["Aim"].ReadValue<Vector2>();
    }

    public void OnMove(InputValue value)
    {
        if (!_isEnabled) return;

        InputValues.HorizontalMovementInput = value.Get<Vector2>().x;
    }

    public void OnSprint(InputValue value)
    {
        if (!_isEnabled) return;

        InputValues.IsSprintKeyPressed = value.Get<float>() == 1f;

        if (InputValues.IsSprintKeyPressed && !InputValues.IsFirePressed)
            _modeManager.ChangeToParkourMode();
        else if (InputValues.IsFirePressed)
            _modeManager.ChangeToCombatMode();
    }

    // TODO: Move these events to triggers
    public void OnJump(InputValue value)
    {
        if (!_isEnabled) return;

        InputValues.IsJumpKeyPressed = value.Get<float>() == 1f;
    }

    public void OnSlide(InputValue value)
    {
        if (!_isEnabled) return;

        InputValues.IsSlideKeyPressed = value.Get<float>() == 1f;

        if (InputValues.IsSlideKeyPressed)
            _modeManager.ChangeToSlidingMode();
    }

    public void OnFire(InputValue value)
    {
        if (!_isEnabled) return;

        InputValues.IsFirePressed = value.Get<float>() == 1f;

        _combatSystem.SetIsShootingServerRpc(value.Get<float>() == 1f);

        if (InputValues.IsFirePressed && _modeManager.CurrentMode == ModeManager.Mode.Parkour)
            _modeManager.ChangeToCombatMode();
        else if (InputValues.IsSprintKeyPressed)
            _modeManager.ChangeToParkourMode();
    }

    public void OnInteract(InputValue value)
    {
        if (!_isEnabled) return;

        _weaponEquipManager.TryEquipWeapon();
        _healthNutManager.TryPickupHealthNut();
    }

    public void OnSwitchMode(InputValue value)
    {
        if (!_isEnabled) return;

        _modeManager.ChangeMode();
    }

    public void OnToggleLeaderboard(InputValue _)
    {
        _scoreboardManager.ToggleScoreboard();
    }

    public void OnToggleReady(InputValue _)
    {
        _lobbyManager.ToggleReady();
    }

    public void OnForceRespawn(InputValue _)
    {
        _respawnManager.ForceRespawn();
    }

    public void OnControlsChanged()
    {
        if (_playerInput == null)
            return;

        InputValues.IsGamepad = _playerInput.currentControlScheme == "Gamepad";
        Debug.Log("Controls changed to " + _playerInput.currentControlScheme);
    }
}
