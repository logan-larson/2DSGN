using UnityEngine;
using FishNet.Object;

public class CombatSystem : NetworkBehaviour
{

    [SerializeField]
    private WeaponHolder _weaponHolder;

    private MovementSystem _movementSystem;

    private float _shootTimer = 0f;

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

        _weaponHolder.SetWeaponShow(false);
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

        ShootServer(_weaponHolder.CurrentWeapon, transform.position, transform.forward);
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
