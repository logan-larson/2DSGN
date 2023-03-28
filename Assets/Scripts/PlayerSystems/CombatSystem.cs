using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;

public class CombatSystem : NetworkBehaviour
{
    [SerializeField]
    private InputSystem _inputSystem;

    [SerializeField]
    private PlayerInputValues _input;

    [SerializeField]
    private WeaponHolder _weaponHolder;

    private MovementSystem _movementSystem;

    private float _shootTimer = 0f;
    private Vector3 _aimDirection = Vector3.zero;

    void Start()
    { // public void OnStart
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _movementSystem = GetComponent<MovementSystem>();

        _movementSystem.OnChangeToCombatMode += OnChangeToCombatMode;
        _movementSystem.OnChangeToParkourMode += OnChangeToParkourMode;

        // _weaponHolder.SetWeaponShow(false);

        _inputSystem = _inputSystem ?? GetComponent<InputSystem>();
        _input = _inputSystem.InputValues;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // _weaponHolder.SetWeaponShow(false);
    }

    private void OnChangeToCombatMode(bool inCombat)
    {
        _weaponHolder.SetWeaponShow(true);
    }

    private void OnChangeToParkourMode(bool inParkour)
    {
        _weaponHolder.SetWeaponShow(false);
    }

    private void Update()
    {
        if (!base.IsOwner) return;

        if (_weaponHolder.CurrentWeapon == null)
        {
            Debug.Log("No weapon");
            return;
        }

        if (_shootTimer < _weaponHolder.CurrentWeapon.FireRate)
        {
            _shootTimer += Time.deltaTime;
            return;
        }

        // Update aim direction.
        UpdateAimDirection();
    }

    private void UpdateAimDirection()
    {
        Vector3 screenMousePosition = Mouse.current.position.ReadValue();
        screenMousePosition.z = Camera.main.nearClipPlane;
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(screenMousePosition);

        Vector3 direction = new Vector3();

        if (_input.IsGamepad)
        {
            direction = _input.AimInput;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle += transform.eulerAngles.z;
            _aimDirection = new Vector3(0f, 0f, angle);
        }
        else
        {
            direction = (worldMousePosition - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _aimDirection = new Vector3(0f, 0f, angle);
        }
    }

    public void Shoot()
    {
        if (_weaponHolder.CurrentWeapon == null)
        {
            Debug.Log("No weapon");
            return;
        }

        if (_shootTimer < _weaponHolder.CurrentWeapon.FireRate) return;

        _shootTimer = 0f;

        Debug.Log("Shoot");
        Debug.DrawRay(transform.position, _aimDirection * _weaponHolder.CurrentWeapon.Range, Color.red, 1f);

        ShootServer(_weaponHolder.CurrentWeapon, transform.position, _aimDirection);
    }

    [ServerRpc]
    public void ShootServer(WeaponInfo weapon, Vector3 position, Vector3 direction)
    {
        if (weapon == null) return;

        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(position, direction, out hit, weapon.Range))
        {
            Debug.Log("Hit " + hit.collider.name);
        }

    }
}
