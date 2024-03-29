using UnityEngine;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using FishNet.Connection;
using FishNet.Object.Synchronizing;

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
    private LineRenderer _bullet;

    private float _shootTimer = 0f;
    private float _bloomTimer = 0f;

    [SyncVar]
    private bool _isShooting = false;

    private Vector3 _aimDirection = Vector3.zero;
    private Vector3 _worldMousePosition = Vector3.zero;

    public UserInfo UserInfo;

    public Vector3 AimDirection => _aimDirection;


    void Start()
    { // public void OnStart
        _bullet.enabled = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _inputSystem = _inputSystem ?? GetComponent<InputSystem>();
        _weaponEquipManager = _weaponEquipManager ?? GetComponent<WeaponEquipManager>();

        _weaponEquipManager.ChangeWeapon.AddListener(OnWeaponChanged);

        _input = _inputSystem.InputValues;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _inputSystem = _inputSystem ?? GetComponent<InputSystem>();
        _weaponEquipManager = _weaponEquipManager ?? GetComponent<WeaponEquipManager>();

        _input = _inputSystem.InputValues;
    }

    private void OnWeaponChanged()
    {
        // When you switch weapons, allow the player to shoot right away
        _shootTimer = _weaponEquipManager.CurrentWeapon.WeaponInfo.FireRate;
    }

    private void Update()
    {
        if (base.IsOwner)
        {
            if (_weaponEquipManager.CurrentWeapon == null)
            {
                Debug.Log("No weapon");
                return;
            }

            UpdateAimDirection();

            if (_shootTimer < _weaponEquipManager.CurrentWeapon.WeaponInfo.FireRate)
            {
                _shootTimer += Time.deltaTime;
                return;
            }

            CheckShoot();
        }

        if (base.IsServer)
        {
            if (!_isShooting)
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
        _isShooting = isShooting;
    }

    private void CheckShoot()
    {
        if (_weaponEquipManager.CurrentWeapon == null)
        {
            Debug.Log("No weapon");
            return;
        }

        if (!_isShooting) return;

        if (_shootTimer < _weaponEquipManager.CurrentWeapon.WeaponInfo.FireRate) return;

        _shootTimer = 0f;

        // TODO: Change bullet spawn location to be related to WeaponInfo.MuzzlePosition
        ShootServer(_weaponEquipManager.CurrentWeapon.WeaponInfo, _weaponHolder.transform.position, _aimDirection, UserInfo.Username);
    }

    [ServerRpc]
    public void ShootServer(WeaponInfo weapon, Vector3 position, Vector3 direction, string username)
    {
        if (weapon == null) return;

        RaycastHit2D[][] allHits = GetHits(weapon, position, direction);

        foreach (RaycastHit2D[] hits in allHits) 
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    if (hit.transform.GetComponentInChildren<PlayerName>() != null)
                    {
                        if (hit.transform.GetComponentInChildren<PlayerName>().Username != username)
                        {
                            if (hit.transform.TryGetComponent(out PlayerHealth enemyHealth)) {
                                var nob = hit.transform.GetComponent<NetworkObject>();
                                DamagePlayerServer(hit.transform.gameObject, weapon.Damage, weapon.Name, nob.LocalConnection);
                            }
                            //var dir = (new Vector3(hit.point.x, hit.point.y, 0f) - transform.position).normalized;

                        }
                    }
                    else if (hit.transform.GetComponentInChildren<Weapon>() == null)
                    {
                        //var dir = (new Vector3(hit.point.x, hit.point.y, 0f) - transform.position).normalized;

                        break;
                    }
                }
            }
        }

        AddBloom(weapon);
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

    [Server]
    public void DrawShotServer(Vector3 position, Vector3 direction, float distance)
    {
        TrailRenderer bulletTrail = Instantiate(_weaponEquipManager.CurrentWeapon.BulletTrailRenderer, position, Quaternion.identity);
        
        StartCoroutine(ShootCoroutine(position, direction, distance, bulletTrail));
    }

    [ObserversRpc]
    public void DrawShotObservers(Vector3 position, Vector3 direction, float distance)
    {
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
