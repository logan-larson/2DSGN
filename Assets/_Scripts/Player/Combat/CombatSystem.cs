using UnityEngine;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/**
<summary>
CombatSystem is responsible for handling all combat related actions.
TODO: Break this up into the following classes:
- WeaponManager
- WeaponAnimator
- ShootManager
- AimManager
- DamageManager
- HitManager
- DeathManager
- RespawnManager
- IngameStatsManager
</summary>
*/
public class CombatSystem : NetworkBehaviour
{
    [SerializeField]
    private InputSystem _inputSystem;

    [SerializeField]
    private PlayerInputValues _input;

    [SerializeField]
    private WeaponHolder _weaponHolder;

    [SerializeField]
    private LineRenderer _bullet;

    private MovementSystem _movementSystem;

    private float _shootTimer = 0f;
    private bool _isShooting = false;

    private Vector3 _aimDirection = Vector3.zero;
    private Vector3 _worldMousePosition = Vector3.zero;

    public UserInfo UserInfo;

    public Vector3 AimDirection => _aimDirection;


    // This needs to get moved sometime
    [SerializeField]
    private List<Vector3> _spawnPositions = new List<Vector3>();



    void Start()
    { // public void OnStart
        _bullet.enabled = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _movementSystem = GetComponent<MovementSystem>();

        _movementSystem.OnChangeToCombatMode += OnChangeToCombatMode;
        _movementSystem.OnChangeToParkourMode += OnChangeToParkourMode;

        _inputSystem = _inputSystem ?? GetComponent<InputSystem>();
        _input = _inputSystem.InputValues;

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

        UpdateAimDirection();

        if (_shootTimer < _weaponHolder.CurrentWeapon.WeaponInfo.FireRate)
        {
            _shootTimer += Time.deltaTime;
            return;
        }

        CheckShoot();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.25f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_worldMousePosition, 0.25f);
    }

    private void UpdateAimDirection()
    {
        Vector3 screenMousePosition = Mouse.current.position.ReadValue();
        screenMousePosition.z = 10f;
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(screenMousePosition);

        _worldMousePosition = new Vector3(worldMousePosition.x, worldMousePosition.y, 0f);

        // Determine input type
        if (_input.IsGamepad)
        {
            // If player is actively aiming, use that direction
            if (_input.AimInput != Vector2.zero)
                _aimDirection = transform.localRotation * _input.AimInput.normalized;
        }
        else
        {
            _aimDirection = (new Vector3(worldMousePosition.x, worldMousePosition.y, 0f) - new Vector3(transform.position.x, transform.position.y, 0f)).normalized;
        }

    }

    public void SetIsShooting(bool isShooting)
    {
        _isShooting = isShooting;
    }

    private void CheckShoot()
    {
        if (_weaponHolder.CurrentWeapon == null)
        {
            Debug.Log("No weapon");
            return;
        }

        if (!_isShooting) return;

        if (_shootTimer < _weaponHolder.CurrentWeapon.WeaponInfo.FireRate) return;

        _shootTimer = 0f;

        Debug.DrawRay(transform.position, _aimDirection * _weaponHolder.CurrentWeapon.WeaponInfo.Range, Color.red, 1f);

        ShootServer(_weaponHolder.CurrentWeapon.WeaponInfo, _weaponHolder.transform.position, _aimDirection, UserInfo.Username);
    }

    [ServerRpc]
    public void ShootServer(WeaponInfo weapon, Vector3 position, Vector3 direction, string username)
    {
        if (weapon == null) return;

        RaycastHit2D[] hits = Physics2D.RaycastAll(position, direction, weapon.Range);

        bool hitSomething = false;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.transform.GetComponentInChildren<PlayerName>() != null)
                {
                    if (hit.transform.GetComponentInChildren<PlayerName>().Username != username)
                    {
                        Debug.Log("Hit user: " + hit.transform.GetComponentInChildren<PlayerName>().Username);

                        if (hit.transform.TryGetComponent(out PlayerHealth enemyHealth)) {
                            enemyHealth.Health -= weapon.Damage;

                            if (enemyHealth.Health <= 0)
                            {
                                // Debug.Log("Killed user: " + hit.transform.GetComponentInChildren<PlayerName>().Username);

                                enemyHealth.Health = 100;

                                int randomSpawnPosition = Random.Range(0, _spawnPositions.Count);

                                Debug.Log("Spawned at position: " + randomSpawnPosition);

                                hit.transform.position = _spawnPositions[randomSpawnPosition];
                                hit.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                            }
                        }

                        ShootObservers(position, direction, hit.distance);

                        hitSomething = true;
                    }
                }
                else if (hit.transform.GetComponentInChildren<Weapon>() != null)
                {
                    Debug.Log("Hit weapon");
                }
                else
                {
                    Debug.Log("Hit environment");
                    ShootObservers(position, direction, hit.distance);

                    hitSomething = true;
                    break;
                }
            }
        }

        if (!hitSomething)
        {
            Debug.Log("Miss");
            ShootObservers(position, direction, weapon.Range);
        }
    }

    [ObserversRpc]
    public void ShootObservers(Vector3 position, Vector3 direction, float distance)
    {
        StartCoroutine(ShootCoroutine(position, direction, distance));
    }

    private IEnumerator ShootCoroutine(Vector3 position, Vector3 direction, float distance)
    {
        _bullet.SetPosition(0, position + direction);
        _bullet.SetPosition(1, position + direction * distance);

        _bullet.enabled = true;

        _weaponHolder.CurrentWeapon.ShowMuzzleFlash();

        yield return new WaitForSeconds(0.1f);

        _bullet.enabled = false;
    }
}
