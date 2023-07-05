using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
using FishNet.Object;
using System.Linq;

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

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _weaponHolder = _weaponHolder ?? GetComponentInChildren<GameObject>();

        _playerHealth.OnDeath.AddListener(OnDeath);
        _respawnManager.OnRespawn.AddListener(OnRespawn);

        _modeManager = _modeManager ?? GetComponentInParent<ModeManager>();

        _modeManager.OnChangeToParkour.AddListener(OnChangeToParkourMode);
        _modeManager.OnChangeToCombat.AddListener(OnChangeToCombatMode);


        Weapons = _weaponHolder
            .GetComponentsInChildren<Weapon>(true);

        for (int i = 0; i < Weapons.Length; i++)
        {
            SetWeaponShowServer(false, i);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _weaponHolder = _weaponHolder ?? GetComponentInChildren<GameObject>();

        Weapons = _weaponHolder
            .GetComponentsInChildren<Weapon>(true);
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
            //DropWeaponServer(CurrentWeapon.gameObject, transform.position);
        }
        else
        {
            // If the current weapon is the default weapon, destroy it
            //Destroy(CurrentWeapon.gameObject);
        }

        //CurrentWeapon = null;
    }

    private void OnRespawn()
    {
        // Equip the default weapon
        //EquipDefaultWeapon();
    }

    private void OnChangeToCombatMode()
    {
        SetWeaponShowServer(true, _currentWeaponIndex);
    }

    private void OnChangeToParkourMode()
    {
        SetWeaponShowServer(false, _currentWeaponIndex);
    }

    [ServerRpc]
    public void SetWeaponShowServer(bool show, int index)
    {
        Weapons[index].IsShown = show;
        SetWeaponShowObservers(show, index);
    }

    [ObserversRpc]
    public void SetWeaponShowObservers(bool show, int index)
    {
        Weapons[index].IsShown = show;
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

        // 2. Disable the current weapon.
        ChangeWeaponActivationServer(_currentWeaponIndex, false, Owner.ClientId);

        // 3. Enable the new weapon.
        ChangeWeaponActivationServer(highlightedWeaponIndex, true, Owner.ClientId);

        /*
        // 4. Destroy the weapon pickup.
        DespawnWeaponPickupServer(_highlightedWeapon.gameObject);

        // 5. Spawn the old weapon pickup in place of the new one.
        SpawnWeaponPickupServer(oldWeaponPickup, _highlightedWeapon.transform.position);
        */

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
    private void DespawnWeaponPickupServer(GameObject weaponPickup)
    {
        ServerManager.Despawn(weaponPickup);
    }

    [ServerRpc]
    private void SpawnWeaponPickupServer(GameObject weaponPickup, Vector3 position)
    {
        GameObject weaponPickups = GameObject.FindWithTag("WeaponPickups");
        GameObject spawnedPickup = Instantiate(weaponPickup, position, Quaternion.identity, weaponPickups.transform);
        spawnedPickup.tag = "WeaponPickup";

        ServerManager.Spawn(spawnedPickup);
    }
}
