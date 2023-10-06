using UnityEngine;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using UnityEngine.Events;
using FishNet.Managing.Timing;
using FishNet.Component.ColliderRollback;
using FishNet.Transporting;

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
    private GameObject _weaponHolder;

    [SerializeField]
    private WeaponEquipManager _weaponEquipManager;

    [SerializeField]
    private ModeManager _modeManager;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private CameraController _cameraController;

    [SerializeField]
    private LineRenderer _bullet;

    [SerializeField]
    private bool _combatDisabled = false;

    public float ShootTimer = 0f;
    private float _bloomTimer = 0f;

    [SyncVar]
    public bool IsShooting = false;

    private Vector3 _aimDirection = Vector3.zero;
    private Vector3 _worldMousePosition = Vector3.zero;

    public Vector3 AimDirection => _aimDirection;

    public UnityEvent OnShoot = new UnityEvent();

    [SerializeField]
    private int _instanceID = -1;

    private void Start()
    {
        _bullet.enabled = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _inputSystem ??= GetComponent<InputSystem>();
        _weaponEquipManager ??= GetComponent<WeaponEquipManager>();

        _weaponEquipManager.ChangeWeapon.AddListener(OnWeaponChanged);

        _input = _inputSystem.InputValues;

        _camera = _weaponEquipManager.transform.GetComponent<CameraManager>().Camera;

        _cameraController = _camera.GetComponent<CameraController>();

        GetInstanceIDServer(base.Owner);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _inputSystem ??= GetComponent<InputSystem>();
        _weaponEquipManager ??= GetComponent<WeaponEquipManager>();

        _weaponEquipManager.ChangeWeapon.AddListener(OnWeaponChanged);

        _input = _inputSystem.InputValues;

        GameStateManager.Instance.OnGameStart.AddListener(OnGameStart);
        GameStateManager.Instance.OnGameEnd.AddListener(OnGameEnd);

        if (base.IsHost)
        {
            _instanceID = gameObject.GetInstanceID();
        }
    }


    [ServerRpc]
    private void GetInstanceIDServer(NetworkConnection conn)
    {
        if (conn == null) return;

        SetInstanceIDTarget(conn, gameObject.GetInstanceID());
    }

    [TargetRpc]
    private void SetInstanceIDTarget(NetworkConnection conn, int instanceID)
    {
        _instanceID = instanceID;
    }

    private void OnGameStart()
    {
        // Enable movement
        _combatDisabled = false;
    }

    private void OnGameEnd()
    {
        // Disable movement things
        _combatDisabled = true;
        IsShooting = false;
    }

    private void OnWeaponChanged()
    {
        // When you switch weapons, allow the player to shoot right away
        ShootTimer = _weaponEquipManager.CurrentWeapon.WeaponInfo.FireRate;
    }

    private void Update()
    {
        if (_combatDisabled) return;

        if (base.IsOwner)
        {
            if (_weaponEquipManager.CurrentWeapon == null)
            {
                Debug.Log("No weapon");
                return;
            }

            UpdateAimDirection();

            if (ShootTimer < _weaponEquipManager.CurrentWeapon.WeaponInfo.FireRate)
            {
                ShootTimer += Time.deltaTime;
                return;
            }

            CheckShoot();
        }

        if (!IsShooting)
        {
            if (_bloomTimer > _weaponEquipManager.CurrentWeapon.WeaponInfo.FireRate)
            {
                SubtractBloom(_weaponEquipManager.CurrentWeapon.WeaponInfo);
                _bloomTimer = 0f;
            }
            else
            {
                _bloomTimer += Time.deltaTime;
            }
        }
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
        // Determine input type
        if (_input.IsGamepad)
        {
            // If player is actively aiming, use that direction
            if (_input.AimInput != Vector2.zero)
                _aimDirection = transform.localRotation * _input.AimInput.normalized;
        }
        else
        {
            var mousePosition = Input.mousePosition;

            if (_cameraController == null || _camera == null) return;

            mousePosition.z = _cameraController.CurrentZ * -1f;

            Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint(mousePosition);

            mouseWorldPosition.z = 0f;

            _aimDirection = (mouseWorldPosition - transform.position).normalized;
        }

    }

    [Server]
    public void SetIsShooting(bool isShooting)
    {
        if (!base.IsOwner) return;

        IsShooting = isShooting;
    }

    [ServerRpc]
    public void SetIsShootingServerRpc(bool isShooting)
    {
        IsShooting = isShooting;
    }

    private void CheckShoot()
    {
        if (_weaponEquipManager.CurrentWeapon == null)
        {
            Debug.Log("No weapon");
            return;
        }

        if (!IsShooting) return;

        // Maybe breakpoint this because I don't think it will ever hit
        if (ShootTimer < _weaponEquipManager.CurrentWeapon.WeaponInfo.FireRate)
        {
            return;
        }

        // -- Shoot a bullet --
        Shoot();
    }

    private void Shoot()
    {
        ShootTimer = 0f;

        OnShoot.Invoke();

        // -- Setup --
        var currentWeapon = _weaponEquipManager.CurrentWeapon.WeaponInfo;
        var bulletSpawnPosition = _weaponHolder.transform.position + (_aimDirection * currentWeapon.MuzzleLength);

        // -- Calculate bullet direction(s) --
        Vector3[] bulletDirections = new Vector3[currentWeapon.BulletsPerShot];
        if (currentWeapon.BulletsPerShot == 1)
        {
            var currentBloom = _weaponEquipManager.CurrentWeapon.CurrentBloom;
            currentBloom = _modeManager.CurrentMode == ModeManager.Mode.Combat ? currentBloom / 2 : currentBloom * 2;
            Vector3 bloomDir = Quaternion.Euler(0f, 0f, Random.Range(-currentBloom, currentBloom)) * _aimDirection;

            bulletDirections[0] = bloomDir;
        }
        else
        {
            for (int i = 0; i < currentWeapon.BulletsPerShot; i++)
            {
                var angle = _modeManager.CurrentMode == ModeManager.Mode.Combat ? currentWeapon.SpreadAngle : currentWeapon.SpreadAngle * 2f;
                Vector3 randomDirection = Quaternion.Euler(0f, 0f, Random.Range(-angle, angle)) * _aimDirection;

                bulletDirections[i] = randomDirection;
            }
        }

        // -- Draw the shot for the shooter --
        LayerMask environment = LayerMask.GetMask("Obstacle");
        for (int i = 0; i < bulletDirections.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(bulletSpawnPosition, bulletDirections[i], currentWeapon.Range, environment);
            RaycastHit2D barrelStuff = Physics2D.Raycast(transform.position, bulletDirections[i], currentWeapon.MuzzleLength, LayerMask.GetMask("Obstacle"));

            if (barrelStuff.collider is not null) continue;

            if (hit.collider is not null)
            {
                // -- Hit the environment, so draw a line to the hit point --
                DrawShot(bulletSpawnPosition, bulletDirections[i], hit.distance);
            }
            else
            {
                // -- Didn't hit anything, so draw a line to the end of the range --
                DrawShot(bulletSpawnPosition, bulletDirections[i], currentWeapon.Range);
            }
        }

        PreciseTick pt = base.TimeManager.GetPreciseTick(base.TimeManager.LastPacketTick);

        // -- Shoot on server -- 
        ShootServer(pt, currentWeapon, transform.position, bulletSpawnPosition, bulletDirections, _instanceID);

        // -- Increase bloom --
        AddBloom(currentWeapon);
    }

    [ServerRpc]
    public void ShootServer(PreciseTick pt, WeaponInfo weapon, Vector3 playerPosition, Vector3 bulletSpawnPosition, Vector3[] bulletDirections, int instanceID)
    {
        if (weapon == null) return;

        // -- Rollback the colliders --
        base.RollbackManager.Rollback(pt, RollbackPhysicsType.Physics2D, base.IsOwner);

        // -- Get all the environment hits --
        LayerMask environment = LayerMask.GetMask("Obstacle");
        for (int i = 0; i < bulletDirections.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(bulletSpawnPosition, bulletDirections[i], weapon.Range, environment);
            RaycastHit2D barrelStuff = Physics2D.Raycast(playerPosition, bulletDirections[i], weapon.MuzzleLength, LayerMask.GetMask("Obstacle"));

            if (barrelStuff.collider is not null) continue;

            // -- Draw the shots for the other players --
            if (hit.collider is not null)
            {
                // -- Hit the environment, so draw a line to the hit point --
                DrawShot(bulletSpawnPosition, bulletDirections[i], hit.distance);
                DrawShotObservers(bulletSpawnPosition, bulletDirections[i], hit.distance);
            }
            else
            {
                // -- Didn't hit anything, so draw a line to the end of the range --
                DrawShot(bulletSpawnPosition, bulletDirections[i], weapon.Range);
                DrawShotObservers(bulletSpawnPosition, bulletDirections[i], weapon.Range);
            }
        }

        // -- Get all the player hits --
        LayerMask hitbox = LayerMask.GetMask("Hitbox", "Obstacle");
        for (int i = 0; i < bulletDirections.Length; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(bulletSpawnPosition, bulletDirections[i], weapon.Range, hitbox);
            RaycastHit2D barrelStuff = Physics2D.Raycast(playerPosition, bulletDirections[i], weapon.MuzzleLength, LayerMask.GetMask("Obstacle"));

            // -- Damage the players --
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null) // Maybe check if the username is the same as the shooter
                {
                    if (barrelStuff.collider is not null && hit.distance > barrelStuff.distance) break;

                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle")) break;

                    var player = hit.transform.parent;

                    if (player.gameObject.GetInstanceID() == instanceID) continue;

                    var nob = player.GetComponent<NetworkObject>();
                    PlayerManager.Instance.DamagePlayer(player.gameObject.GetInstanceID(), weapon.Damage, gameObject.GetInstanceID(), weapon.Name, nob.LocalConnection);
                }
            }
        }


        // -- Return the colliders --
        base.RollbackManager.Return();

        // -- Get all the hits for each bullet --
        /*
        RaycastHit2D[][] allHits = new RaycastHit2D[directions.Length][];

        LayerMask nonHittable = LayerMask.GetMask("WeaponPickup");
        for (int i = 0; i < directions.Length; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, directions[i], weapon.Range, ~nonHittable);

            //DrawShotWithHits(hits, position, directions[i], weapon.Range);

            allHits[i] = hits;
        }

        //RaycastHit2D[][] allHits = GetHits(weapon, position, directions);

        // -- Damage the players --
        foreach (RaycastHit2D[] hits in allHits) 
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    var player = hit.transform.parent;
                    if (player.GetComponentInChildren<PlayerName>() != null && player.GetComponentInChildren<PlayerName>().Username != username)
                    {
                        if (player.TryGetComponent(out PlayerHealth enemyHealth)) {
                            var nob = player.GetComponent<NetworkObject>();
                            DamagePlayerServer(player.gameObject, weapon.Damage, weapon.Name, nob.LocalConnection);
                        }
                        //var dir = (new Vector3(hit.point.x, hit.point.y, 0f) - transform.position).normalized;
                    }
                    else if (player.GetComponentInChildren<Weapon>() == null)
                    {
                        //var dir = (new Vector3(hit.point.x, hit.point.y, 0f) - transform.position).normalized;

                        break;
                    }
                }
            }
        }

        //AddBloom(weapon);
        */
    }

    private void DamagePlayerServer(GameObject playerHit, int damage, string weaponName, NetworkConnection playerConn)
    {
        PlayerManager.Instance.DamagePlayer(playerHit.GetInstanceID(), damage, gameObject.GetInstanceID(), weaponName, playerConn);
    }

    private void DrawShot(Vector3 origin, Vector3 direction, float distance)
    {
        TrailRenderer bulletTrail = Instantiate(_weaponEquipManager.CurrentWeapon.BulletTrailRenderer, origin, Quaternion.identity);
        
        StartCoroutine(ShootCoroutine(origin, direction, distance, bulletTrail));
    }

    [ObserversRpc]
    public void DrawShotObservers(Vector3 origin, Vector3 direction, float distance)
    {
        // -- Owner of shot check --
        if (base.IsOwner) return;

        TrailRenderer bulletTrail = Instantiate(_weaponEquipManager.CurrentWeapon.BulletTrailRenderer, origin, Quaternion.identity);
        
        StartCoroutine(ShootCoroutine(origin, direction, distance, bulletTrail));
    }

    private IEnumerator ShootCoroutine(Vector3 position, Vector3 direction, float distance, TrailRenderer bulletTrail)
    {
        _weaponEquipManager.CurrentWeapon.ShowMuzzleFlash();

        float time = 0;
        Vector3 startPosition = bulletTrail.transform.position;
        Vector3 endPosition = position + direction * distance;

        while (time < 1f)
        {
            bulletTrail.transform.position = Vector3.Lerp(startPosition, endPosition, time);
            time += Time.deltaTime / bulletTrail.time;

            yield return null;
        }

        Destroy(bulletTrail.gameObject, bulletTrail.time);
    }


    private void AddBloom(WeaponInfo weapon)
    {
        if (weapon.BulletsPerShot != 1) return;

        _weaponEquipManager.CurrentWeapon.CurrentBloom += weapon.BloomAngleIncreasePerShot;

        if (_weaponEquipManager.CurrentWeapon.CurrentBloom > weapon.MaxBloomAngle)
            _weaponEquipManager.CurrentWeapon.CurrentBloom = weapon.MaxBloomAngle;
    }

    private void SubtractBloom(WeaponInfo weapon)
    {
        if (weapon.BulletsPerShot != 1) return;

        _weaponEquipManager.CurrentWeapon.CurrentBloom -= weapon.BloomAngleIncreasePerShot * 1.5f;

        if (_weaponEquipManager.CurrentWeapon.CurrentBloom < 0f)
            _weaponEquipManager.CurrentWeapon.CurrentBloom = 0f;
    }
}
