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

    private float _shootTimer = 0f;
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
        _input = _inputSystem.InputValues;

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

        if (!_isShooting)
        {
            SubtractBloom(_weaponHolder.CurrentWeapon.WeaponInfo);
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

        RaycastHit2D[][] allHits = GetHits(weapon, position, direction);

        bool hitSomething = false;

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
                            Debug.Log("Hit user: " + hit.transform.GetComponentInChildren<PlayerName>().Username);

                            if (hit.transform.TryGetComponent(out PlayerHealth enemyHealth)) {
                                enemyHealth.Health -= weapon.Damage;
                            }
                            var dir = (new Vector3(hit.point.x, hit.point.y, 0f) - transform.position).normalized;

                            hitSomething = true;
                        }
                    }
                    else if (hit.transform.GetComponentInChildren<Weapon>() == null)
                    {
                        Debug.Log("Hit environment");
                        var dir = (new Vector3(hit.point.x, hit.point.y, 0f) - transform.position).normalized;
                        // ShootObservers(position, dir, hit.distance);

                        hitSomething = true;
                        break;
                    }
                }
            }
        }

        AddBloom(weapon);
    }

    private RaycastHit2D[][] GetHits(WeaponInfo weapon, Vector3 position, Vector3 direction)
    {
        RaycastHit2D[][] hits = new RaycastHit2D[0][];
        if (weapon.BulletsPerShot == 1)
        {
            hits = new RaycastHit2D[1][];

            var currentBloom = _weaponHolder.CurrentWeapon.CurrentBloom;
            Vector3 bloomDir= Quaternion.Euler(0f, 0f, Random.Range(-currentBloom, currentBloom)) * direction;

            hits[0] = Physics2D.RaycastAll(position, bloomDir, weapon.Range);

            DrawShot(hits[0], position, bloomDir, weapon.Range);
        }
        else
        {
            hits = new RaycastHit2D[weapon.BulletsPerShot][];
            for (int i = 0; i < weapon.BulletsPerShot; i++)
            {
                Vector3 randomDirection = Quaternion.Euler(0f, 0f, Random.Range(-weapon.SpreadAngle, weapon.SpreadAngle)) * direction;

                Debug.DrawRay(position, randomDirection * weapon.Range, Color.green, 0.5f);

                RaycastHit2D[] bulletHits = Physics2D.RaycastAll(position, randomDirection, weapon.Range);

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
                ShootObservers(position, direction, hits[System.Array.IndexOf(hits, hit)].distance);

                hitSomething = true;
                break;
            }
        }

        if (!hitSomething)
        {
            ShootObservers(position, direction, distance);
        }
    }

    [ObserversRpc]
    public void ShootObservers(Vector3 position, Vector3 direction, float distance)
    {
        TrailRenderer bulletTrail = Instantiate(_weaponHolder.CurrentWeapon.BulletTrailRenderer, position, Quaternion.identity);
        
        StartCoroutine(ShootCoroutine(position, direction, distance, bulletTrail));
    }

    private IEnumerator ShootCoroutine(Vector3 position, Vector3 direction, float distance, TrailRenderer bulletTrail)
    {
        _weaponHolder.CurrentWeapon.ShowMuzzleFlash();

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

        _weaponHolder.CurrentWeapon.CurrentBloom += weapon.BloomAngleIncreasePerShot;

        if (_weaponHolder.CurrentWeapon.CurrentBloom > weapon.MaxBloomAngle)
            _weaponHolder.CurrentWeapon.CurrentBloom = weapon.MaxBloomAngle;
    }

    private void SubtractBloom(WeaponInfo weapon)
    {
        if (weapon.BulletsPerShot != 1) return;

        _weaponHolder.CurrentWeapon.CurrentBloom -= weapon.MaxBloomAngle * Time.deltaTime;

        if (_weaponHolder.CurrentWeapon.CurrentBloom < 0f)
            _weaponHolder.CurrentWeapon.CurrentBloom = 0f;
    }
}
