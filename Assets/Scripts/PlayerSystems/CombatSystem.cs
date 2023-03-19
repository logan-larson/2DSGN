using UnityEngine;
using FishNet.Object;

public class CombatSystem : NetworkBehaviour
{

    [SerializeField]
    private WeaponHolder _weaponHolder;

    private MovementSystem _movementSystem;

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

    void Update()
    { // public void OnUpdate
    }

    public void ShootPrimary()
    {
    }
}
