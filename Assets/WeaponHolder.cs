using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.InputSystem;

public class WeaponHolder : NetworkBehaviour
{
    [SerializeField]
    private int _currentWeaponIndex = 0;

    [SerializeField]
    private float _weaponDistanceThreshold = 1.5f;

    [SerializeField]
    private List<WeaponInfo> _weapons = new List<WeaponInfo>();

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private MovementSystem _movementSystem;

    [SerializeField]
    private InputSystem _inputSystem;

    [SerializeField]
    private PlayerInputValues _input;

    [SerializeField]
    private WeaponSprites _weaponSprites;

    public WeaponInfo CurrentWeapon => _weapons[_currentWeaponIndex];

    [SyncVar(OnChange = nameof(OnChangeWeaponShow))]
    public bool WeaponShow = false;

    private void OnChangeWeaponShow(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            _spriteRenderer.enabled = true;
        }
        else
        {
            _spriteRenderer.enabled = false;
        }
    }

    [SyncVar(OnChange = nameof(OnChangeFlipY))]
    public bool FlipY = false;

    private void OnChangeFlipY(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            _spriteRenderer.flipY = true;
        }
        else
        {
            _spriteRenderer.flipY = false;
        }
    }

    private void Start() {
        _spriteRenderer = _spriteRenderer ?? GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();


        int index = _weaponSprites.Names.IndexOf(CurrentWeapon.Name);

        if (index == -1)
        {
            Debug.LogError($"Weapon {CurrentWeapon.Name} not found in WeaponSprites.");
        }
        else
        {
            _spriteRenderer.sprite = _weaponSprites.Sprites[index];
        }

        if (base.IsOwner)
        {
            _inputSystem = _inputSystem ?? GetComponent<InputSystem>();
            _input = _inputSystem.InputValues;
        }
    }

    public void Update()
    {
        if (!base.IsOwner) return;

        Vector3 screenMousePosition = Mouse.current.position.ReadValue();
        screenMousePosition.z = Camera.main.nearClipPlane;
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(screenMousePosition);
        // Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(_input.AimInput);

        Vector3 direction = new Vector3();

        if (_input.IsGamepad)
        {
            direction = _input.AimInput;
        }
        else
        {
            direction = (new Vector3(worldMousePosition.x, worldMousePosition.y, 0f) - transform.position).normalized;
        }


        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.localEulerAngles = new Vector3(0f, 0f, angle);

        // If past vertical, flip sprite.
        float angleDifference = Mathf.DeltaAngle(transform.parent.rotation.eulerAngles.z, transform.rotation.eulerAngles.z);
        if (angleDifference > 90f || angleDifference < -90f)
        {
            SetFlipY(true);
        }
        else
        {
            SetFlipY(false);
        }
    }

    [ServerRpc]
    public void SetWeaponShow(bool show)
    {
        WeaponShow = show;
    }

    [ServerRpc]
    public void SetFlipY(bool flipY)
    {
        FlipY = flipY;
    }

}
