using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class WeaponEquipManager : NetworkBehaviour
{

    private Weapon _highlightedWeapon;

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
        GameObject[] weapons = GameObject.FindGameObjectsWithTag("Weapon");
        GameObject closestWeapon = null;
        float closestDistance = 5f;
        Vector3 referencePosition = transform.position;

        foreach (GameObject weapon in weapons)
        {
            weapon.GetComponent<Weapon>().HideHighlight();

            if (weapon.GetComponent<Weapon>().IsEquipped)
                continue;

            float distance = Vector3.Distance(weapon.transform.position, referencePosition);
            if (distance < closestDistance)
            {
                closestWeapon = weapon;
                closestDistance = distance;
            }
        }

        if (closestWeapon != null)
        {
            Weapon weapon = closestWeapon.GetComponent<Weapon>();
            if (weapon != null && weapon != _weaponHolder.CurrentWeapon)
            {
                _highlightedWeapon = weapon;
                weapon.ShowHighlight();
            }
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

        _weaponHolder.EquipWeapon(weaponObj, _highlightedWeapon.transform.position);

        _highlightedWeapon = null;
    }
}
