using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class WeaponEquipManager : NetworkBehaviour
{

    private WeaponPickup _highlightedWeapon;

    [SerializeField]
    private WeaponHolder _weaponHolder;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _weaponHolder = _weaponHolder ?? GetComponentInChildren<WeaponHolder>();
    }


    private void Update()
    {
        if (!base.IsOwner) return;
        
        HighlightWeapon();
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
            if (weaponPickup != null && weaponPickup.Name != _weaponHolder.CurrentWeapon.WeaponInfo.Name)
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
    Try to equip the highlighted weapon. Called by the player's input system.
    </summary>
    */
    public void TryEquipWeapon()
    {
        if (!base.IsOwner) return;

        if (_highlightedWeapon == null) return;

        var weaponObj = _highlightedWeapon.gameObject;

        _weaponHolder.SwapWeapons(weaponObj);

        _highlightedWeapon = null;
    }
}
