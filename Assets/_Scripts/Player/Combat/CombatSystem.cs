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
    private PlayerName _playerName;

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


    private void Start()
    {
        _bullet.enabled = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _inputSystem ??= GetComponent<InputSystem>();
        _playerName ??= GetComponentInChildren<PlayerName>();
        _weaponEquipManager ??= GetComponent<WeaponEquipManager>();

        _weaponEquipManager.ChangeWeapon.AddListener(OnWeaponChanged);

        _input = _inputSystem.InputValues;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _inputSystem ??= GetComponent<InputSystem>();
        _weaponEquipManager ??= GetComponent<WeaponEquipManager>();
        _playerName ??= GetComponentInChildren<PlayerName>();

        _weaponEquipManager.ChangeWeapon.AddListener(OnWeaponChanged);

        _input = _inputSystem.InputValues;

        GameStateManager.Instance.OnGameStart.AddListener(OnGameStart);
        GameStateManager.Instance.OnGameEnd.AddListener(OnGameEnd);
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
        Vector3 screenMousePosition = Mouse.current.position.ReadValue();
        screenMousePosition.z = 10f;

        if (Camera.main == null) return;

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

        if (ShootTimer < _weaponEquipManager.CurrentWeapon.WeaponInfo.FireRate) return;

        ShootTimer = 0f;

        OnShoot.Invoke();

        var currentWeapon = _weaponEquipManager.CurrentWeapon.WeaponInfo;

        var bulletSpawnPosition = _weaponHolder.transform.position + (_aimDirection * currentWeapon.MuzzleLength);

        var slidingMultiplier = _input.IsSlideKeyPressed ? 2f : 1f;

        Vector3[] bulletDirections = new Vector3[currentWeapon.BulletsPerShot];
        if (currentWeapon.BulletsPerShot == 1)
        {
            var currentBloom = _weaponEquipManager.CurrentWeapon.CurrentBloom * slidingMultiplier;
            Vector3 bloomDir = Quaternion.Euler(0f, 0f, Random.Range(-currentBloom, currentBloom)) * _aimDirection;

            bulletDirections[0] = bloomDir;
        }
        else
        {
            for (int i = 0; i < currentWeapon.BulletsPerShot; i++)
            {
                var angle = currentWeapon.SpreadAngle * slidingMultiplier;
                Vector3 randomDirection = Quaternion.Euler(0f, 0f, Random.Range(-angle, angle)) * _aimDirection;

                bulletDirections[i] = randomDirection;
            }
        }

        // Shoot on client
        for (int i = 0; i < bulletDirections.Length; i++)
        {
            DrawShotOwner(bulletSpawnPosition, bulletDirections[i], currentWeapon.Range);
        }

        PreciseTick pt = base.TimeManager.GetPreciseTick(base.TimeManager.LastPacketTick);

        // Shoot on server and enable collider rollback
        ShootServer(pt, _weaponEquipManager.CurrentWeapon.WeaponInfo, bulletSpawnPosition, bulletDirections, _playerName.Username);

        AddBloom(currentWeapon);
    }

    [ServerRpc]
    public void ShootServer(PreciseTick pt, WeaponInfo weapon, Vector3 position, Vector3[] directions, string username)
    {
        if (weapon == null) return;

        if (!base.IsOwner)
        {
            OnShoot.Invoke();
        }

        base.RollbackManager.Rollback(pt, RollbackPhysicsType.Physics2D, base.IsOwner);

        RaycastHit2D[][] allHits = new RaycastHit2D[directions.Length][];

        LayerMask nonHittable = LayerMask.GetMask("WeaponPickup");
        for (int i = 0; i < directions.Length; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, directions[i], weapon.Range, ~nonHittable);

            DrawShot(hits, position, directions[i], weapon.Range);

            allHits[i] = hits;
        }

        //RaycastHit2D[][] allHits = GetHits(weapon, position, directions);

        base.RollbackManager.Return();

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
    }

    private void DamagePlayerServer(GameObject playerHit, int damage, string weaponName, NetworkConnection playerConn)
    {
        PlayerManager.Instance.DamagePlayer(playerHit.GetInstanceID(), damage, gameObject.GetInstanceID(), weaponName, playerConn);
    }

    private RaycastHit2D[][] GetHits(WeaponInfo weapon, Vector3 position, Vector3 direction)
    {
        LayerMask nonHittable = LayerMask.GetMask("WeaponPickup");
        RaycastHit2D[][] hits = new RaycastHit2D[0][];
        if (weapon.BulletsPerShot == 1)
        {
            hits = new RaycastHit2D[1][];

            var currentBloom = _weaponEquipManager.CurrentWeapon.CurrentBloom;
            Vector3 bloomDir= Quaternion.Euler(0f, 0f, Random.Range(-currentBloom, currentBloom)) * direction;

            hits[0] = Physics2D.RaycastAll(position, bloomDir, weapon.Range, ~nonHittable);

            DrawShot(hits[0], position, bloomDir, weapon.Range);
        }
        else
        {
            hits = new RaycastHit2D[weapon.BulletsPerShot][];
            for (int i = 0; i < weapon.BulletsPerShot; i++)
            {
                Vector3 randomDirection = Quaternion.Euler(0f, 0f, Random.Range(-weapon.SpreadAngle, weapon.SpreadAngle)) * direction;

                Debug.DrawRay(position, randomDirection * weapon.Range, Color.green, 0.5f);

                RaycastHit2D[] bulletHits = Physics2D.RaycastAll(position, randomDirection, weapon.Range, ~nonHittable);

                hits[i] = bulletHits;

                DrawShot(hits[i], position, randomDirection, weapon.Range);
            }
        }

        return hits;
    }

    private void DrawShot(RaycastHit2D[] hits, Vector3 position, Vector3 direction, float distance)
    {
        bool hitSomething = false;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.transform.GetComponentInChildren<Weapon>() == null)
            {
                DrawShotServer(position, direction, hits[System.Array.IndexOf(hits, hit)].distance);
                DrawShotObservers(position, direction, hits[System.Array.IndexOf(hits, hit)].distance);

                hitSomething = true;
                break;
            }
        }

        if (!hitSomething)
        {
            DrawShotServer(position, direction, distance);
            DrawShotObservers(position, direction, distance);
        }
    }

    public void DrawShotOwner(Vector3 position, Vector3 direction, float distance)
    {
        TrailRenderer bulletTrail = Instantiate(_weaponEquipManager.CurrentWeapon.BulletTrailRenderer, position, Quaternion.identity);
        
        StartCoroutine(ShootCoroutine(position, direction, distance, bulletTrail));
    }

    [Server]
    public void DrawShotServer(Vector3 position, Vector3 direction, float distance)
    {
        TrailRenderer bulletTrail = Instantiate(_weaponEquipManager.CurrentWeapon.BulletTrailRenderer, position, Quaternion.identity);
        
        StartCoroutine(ShootCoroutine(position, direction, distance, bulletTrail));
    }

    [ObserversRpc]
    public void DrawShotObservers(Vector3 position, Vector3 direction, float distance)
    {
        if (base.IsOwner || base.IsServer) return;

        TrailRenderer bulletTrail = Instantiate(_weaponEquipManager.CurrentWeapon.BulletTrailRenderer, position, Quaternion.identity);
        
        StartCoroutine(ShootCoroutine(position, direction, distance, bulletTrail));
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
