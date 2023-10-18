using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Linq;
using static ModeManager;
using UnityEngine.Events;
using FishNet.Connection;
using FishNet.Transporting;

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
    private AudioClip _changeWeapon;

    [SerializeField]
    public Weapon[] Weapons { get; private set; }

    public Weapon CurrentWeapon => Weapons[_currentWeaponIndex];

    public UnityEvent ChangeWeapon = new UnityEvent();

    private int _currentWeaponIndex = 0;

    [SyncVar]
    private int _currentWeaponPickupID;

    public override void OnStartClient()
    {
        base.OnStartClient();

        _weaponHolder ??= GetComponentInChildren<GameObject>();

        Weapons = _weaponHolder.GetComponentsInChildren<Weapon>(true);

        foreach (Weapon weapon in Weapons)
        {
            weapon.Initialize();
        }

        if (!base.IsOwner) return;

        _modeManager ??= GetComponentInParent<ModeManager>();
        _respawnManager ??= GetComponentInParent<RespawnManager>();
        _playerHealth ??= GetComponentInParent<PlayerHealth>();

        _modeManager.OnChangeToParkour.AddListener(OnChangeToParkourMode);
        _modeManager.OnChangeToCombat.AddListener(OnChangeToCombatMode);
        _modeManager.OnChangeToSliding.AddListener(OnChangeToSlidingMode);

        Weapons[_currentWeaponIndex].IsShown = _modeManager.CurrentMode == Mode.Combat || _modeManager.CurrentMode == Mode.Sliding;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _weaponHolder ??= GetComponentInChildren<GameObject>();
        _respawnManager ??= GetComponentInParent<RespawnManager>();

        Weapons = _weaponHolder.GetComponentsInChildren<Weapon>(true);

        _playerHealth.OnDeath.AddListener(OnDeath);
        _respawnManager.OnRespawn.AddListener(OnRespawn);

        foreach (Weapon weapon in Weapons)
        {
            weapon.Initialize();
        }

        Weapons[_currentWeaponIndex].IsShown = _modeManager.CurrentMode == Mode.Combat || _modeManager.CurrentMode == Mode.Sliding;

        ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    private void Update()
    {
        if (!base.IsOwner) return;
        
        HighlightWeapon();
    }

    // On disconnect, call the OnDeath method to drop the weapon
    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            OnDeath(false, Vector3.zero);
        }
    }

    private void OnDeath(bool _, Vector3 __)
    {
        // Drop the current weapon, if it isn't the default weapon
        if (_currentWeaponIndex != _defaultWeaponIndex)
        {
            GameObject[] weapons = GameObject.FindGameObjectsWithTag("WeaponPickup");

            var weaponPickup = weapons.Length > 0 ? weapons.FirstOrDefault(w => w.GetComponent<WeaponPickup>().WeaponID == _currentWeaponPickupID) : null;

            if (weaponPickup == null) return;

            weaponPickup.GetComponent<WeaponPickup>().Drop();
        }

        var weapon = Weapons[_currentWeaponIndex];

        if (weapon == null) return;

        weapon.IsShown = false;

        ChangeWeaponActivationObservers(_currentWeaponIndex, false, Owner.ClientId);

        _currentWeaponPickupID = 0;
    }

    private void OnRespawn()
    {
        var weapon = Weapons[_defaultWeaponIndex];

        if (weapon == null) return;

        if (_modeManager.CurrentMode == Mode.Combat || _modeManager.CurrentMode == Mode.Sliding)
            weapon.IsShown = true;
        else
            weapon.IsShown = false;

        ChangeWeaponActivationObservers(_defaultWeaponIndex, true, Owner.ClientId);
    }

    private void OnChangeToCombatMode()
    {
        ChangeWeaponActivationServer(_currentWeaponIndex, true, -1);
    }

    private void OnChangeToParkourMode()
    {
        ChangeWeaponActivationServer(_currentWeaponIndex, false, -1);
    }

    private void OnChangeToSlidingMode()
    {

        ChangeWeaponActivationServer(_currentWeaponIndex, true, -1);
    }

    /// <summary>
    /// Highlight the closest weapon that isn't equipped and within a certain threshold distance.
    /// </summary>
    private void HighlightWeapon()
    {
        GameObject[] weapons = GameObject.FindGameObjectsWithTag("WeaponPickup");
        GameObject closestWeaponPickup = null;
        float closestDistance = 5f;
        Vector3 referencePosition = transform.position;

        foreach (GameObject weapon in weapons)
        {
            var weaponPickup = weapon.GetComponent<WeaponPickup>();
            weaponPickup.HideHighlight();

            float distance = Vector3.Distance(weapon.transform.position, referencePosition);
            // If the distance is the closest and it is available to be picked up.
            if (!weaponPickup.IsPickedUp && distance < closestDistance)
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

    /// <summary>
    /// Try to equip the highlighted weapon. This is called by the player's input system.
    /// </summary>
    public void TryEquipWeapon()
    {
        if (!base.IsOwner) return;

        if (_highlightedWeapon == null) return;

        // 0. Get the highlighted weapons index in the weapons array.
        int highlightedWeaponIndex = Weapons.ToList().FindIndex(w => w.WeaponInfo.Name == _highlightedWeapon.Name);

        // 1. Check if the weapon is already equipped, if so, return.
        if (highlightedWeaponIndex == _currentWeaponIndex) return;

        // 2. Disable the current weapon in inventory.
        ChangeWeaponActivationServer(_currentWeaponIndex, false, Owner.ClientId, transform.position);

        // 3. Enable the new weapon in inventory.
        ChangeWeaponActivationServer(highlightedWeaponIndex, true, Owner.ClientId, transform.position);

        // 4. Pickup the weapon pickup.
        PickupWeaponPickupServer(_highlightedWeapon.WeaponID);

        // 5. Drop the current weapon pickup.
        DropWeaponPickupServer(_currentWeaponPickupID);

        SetCurrentWeaponPickupIDServerRPC(_highlightedWeapon.WeaponID);

        _highlightedWeapon = null;
    }

    [ServerRpc]
    private void SetCurrentWeaponPickupIDServerRPC(int id)
    {
        _currentWeaponPickupID = id;
    }


    [ServerRpc]
    private void ChangeWeaponActivationServer(int index, bool isActive, int clientId, Vector3? position = null)
    {
        var weapon = Weapons[index];

        if (weapon == null) return;

        if (_modeManager.CurrentMode == Mode.Combat || _modeManager.CurrentMode == Mode.Sliding)
            weapon.IsShown = isActive;
        else
            weapon.IsShown = false;

        if (Owner.ClientId == clientId)
        {
            _currentWeaponIndex = index;

            ChangeWeapon.Invoke();
        }

        ChangeWeaponActivationObservers(index, isActive, clientId, position);
    }

    [ObserversRpc]
    private void ChangeWeaponActivationObservers(int index, bool isActive, int clientId, Vector3? position = null)
    {
        var weapon = Weapons[index];

        if (weapon == null) return;

        if (_modeManager.CurrentMode == Mode.Combat || _modeManager.CurrentMode == Mode.Sliding)
            weapon.IsShown = isActive;
        else
            weapon.IsShown = false;

        if (Owner.ClientId == clientId)
        {
            _currentWeaponIndex = index;

            ChangeWeapon.Invoke();
        }

        if (isActive && position is not null)
        {
            var volume = base.IsOwner ? 3f : 5f;
            AudioSource.PlayClipAtPoint(_changeWeapon, position.Value, volume);
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
