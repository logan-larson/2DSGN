using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
using FishNet.Object;
using System.Linq;
using static ModeManager;

public class WeaponEquipManager : NetworkBehaviour
{

    private WeaponPickup _highlightedWeapon;

    [SerializeField]
    private GameObject _weaponHolder;

    [SerializeField]
    private PlayerHealth _playerHealth;

    [SerializeField]
    private RespawnManager _respawnManager;

    [SerializeField]
    private ModeManager _modeManager;

    [SerializeField]
    private int _defaultWeaponIndex;

    [SerializeField]
    public Weapon[] Weapons { get; private set; }

    public Weapon CurrentWeapon => Weapons[_currentWeaponIndex];

    private int _currentWeaponIndex = 0;

    private int _currentWeaponPickupID;

    public override void OnStartClient()
    {
        base.OnStartClient();

        _weaponHolder = _weaponHolder ?? GetComponentInChildren<GameObject>();

        Weapons = _weaponHolder
            .GetComponentsInChildren<Weapon>(true);

        if (!base.IsOwner) return;

        _modeManager = _modeManager ?? GetComponentInParent<ModeManager>();

        _modeManager.OnChangeToParkour.AddListener(OnChangeToParkourMode);
        _modeManager.OnChangeToCombat.AddListener(OnChangeToCombatMode);

        foreach (Weapon weapon in Weapons)
        {
            weapon.Initialize();
        }

        Weapons[_currentWeaponIndex].IsShown = _modeManager.CurrentMode == Mode.Combat;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _weaponHolder = _weaponHolder ?? GetComponentInChildren<GameObject>();

        Weapons = _weaponHolder
            .GetComponentsInChildren<Weapon>(true);

        _playerHealth.OnDeath.AddListener(OnDeath);
        _respawnManager.OnRespawn.AddListener(OnRespawn);

        foreach (Weapon weapon in Weapons)
        {
            weapon.Initialize();
        }

        Weapons[_currentWeaponIndex].IsShown = _modeManager.CurrentMode == Mode.Combat;
    }

    private void Update()
    {
        if (!base.IsOwner) return;
        
        HighlightWeapon();
    }

    private void OnDeath()
    {
        // Drop the current weapon, if it isn't the default weapon
        if (_currentWeaponIndex != _defaultWeaponIndex)
        {
            DropWeaponPickupServer(_currentWeaponPickupID);
        }

        ChangeWeaponActivationServer(_currentWeaponIndex, false, -1);
    }

    private void OnRespawn()
    {
        ChangeWeaponActivationServer(_defaultWeaponIndex, true, Owner.ClientId);
    }

    private void OnChangeToCombatMode()
    {
        ChangeWeaponActivationServer(_currentWeaponIndex, true, -1);
    }

    private void OnChangeToParkourMode()
    {
        ChangeWeaponActivationServer(_currentWeaponIndex, false, -1);
    }

    /**
    <summary>
    Highlight the closest weapon that isn't equipped and within a certain threshold distance.
    </summary>
    */
    private void HighlightWeapon()
    {
        GameObject[] weapons = GameObject.FindGameObjectsWithTag("WeaponPickup");
        GameObject closestWeaponPickup = null;
        float closestDistance = 5f;
        Vector3 referencePosition = transform.position;

        foreach (GameObject weapon in weapons)
        {
            weapon.GetComponent<WeaponPickup>().HideHighlight();

            float distance = Vector3.Distance(weapon.transform.position, referencePosition);
            if (distance < closestDistance)
            {
                closestWeaponPickup = weapon;
                closestDistance = distance;
            }
        }

        if (closestWeaponPickup != null)
        {
            WeaponPickup weaponPickup = closestWeaponPickup.GetComponent<WeaponPickup>();
            if (weaponPickup != null)
            {
                _highlightedWeapon = weaponPickup;
                weaponPickup.ShowHighlight();
            }
        } else
        {
            _highlightedWeapon = null;
        }
    }

    /**
    <summary>
    Try to equip the highlighted weapon. This is called by the player's input system.
    </summary>
    */
    public void TryEquipWeapon()
    {
        if (!base.IsOwner) return;

        if (_highlightedWeapon == null) return;

        // 0. Get the highlighted weapons index in the weapons array.
        int highlightedWeaponIndex = Weapons.ToList().FindIndex(w => w.WeaponInfo.Name == _highlightedWeapon.Name);

        // 1. Check if the weapon is already equipped, if so, return.
        if (highlightedWeaponIndex == _currentWeaponIndex) return;

        // 2. Disable the current weapon in inventory.
        ChangeWeaponActivationServer(_currentWeaponIndex, false, Owner.ClientId);

        // 3. Enable the new weapon in inventory.
        ChangeWeaponActivationServer(highlightedWeaponIndex, true, Owner.ClientId);

        // 4. Pickup the weapon pickup.
        PickupWeaponPickupServer(_highlightedWeapon.WeaponID);

        // 5. Drop the current weapon pickup.
        DropWeaponPickupServer(_currentWeaponPickupID);


        _currentWeaponPickupID = _highlightedWeapon.WeaponID;

        _highlightedWeapon = null;
    }

    [ServerRpc]
    private void ChangeWeaponActivationServer(int index, bool isActive, int clientId)
    {
        var weapon = Weapons[index];

        if (weapon == null) return;

        if (_modeManager.CurrentMode == Mode.Combat)
            weapon.IsShown = isActive;
        else
            weapon.IsShown = false;

        ChangeWeaponActivationObservers(index, isActive, clientId);
    }

    [ObserversRpc]
    private void ChangeWeaponActivationObservers(int index, bool isActive, int clientId)
    {
        var weapon = Weapons[index];

        if (weapon == null) return;

        if (_modeManager.CurrentMode == Mode.Combat)
            weapon.IsShown = isActive;
        else
            weapon.IsShown = false;

        if (Owner.ClientId == clientId)
        {
            _currentWeaponIndex = index;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickupWeaponPickupServer(int weaponPickupID)
    {
        GameObject[] weapons = GameObject.FindGameObjectsWithTag("WeaponPickup");

        var weaponPickup = weapons.Length > 0 ? weapons.FirstOrDefault(w => w.GetComponent<WeaponPickup>().WeaponID == weaponPickupID) : null;

        if (weaponPickup == null) return;

        weaponPickup.GetComponent<WeaponPickup>().Pickup();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropWeaponPickupServer(int weaponPickupID)
    {
        GameObject[] weapons = GameObject.FindGameObjectsWithTag("WeaponPickup");

        var weaponPickup = weapons.Length > 0 ? weapons.FirstOrDefault(w => w.GetComponent<WeaponPickup>().WeaponID == weaponPickupID) : null;

        if (weaponPickup == null) return;

        weaponPickup.GetComponent<WeaponPickup>().Drop();
    }
}
