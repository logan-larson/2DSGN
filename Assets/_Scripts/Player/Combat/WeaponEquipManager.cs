using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEquipManager : MonoBehaviour
{

    private Weapon _highlightedWeapon;

    [SerializeField]
    private CombatSystem _combatSystem;

    [SerializeField]
    private WeaponHolder _weaponHolder;

    private Vector3 _aimDirection = Vector3.zero;

    private void Update()
    {
        _aimDirection = _combatSystem.AimDirection;
        
        HighlightWeapon();
    }

    private void HighlightWeapon()
    {
        // See if the player is aiming at a weapon
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _aimDirection, 3f, LayerMask.GetMask("Weapon"));
        Debug.DrawRay(transform.position, _aimDirection * 3f, Color.red);

        // If the player is aiming at a weapon, highlight it
        if (hit.collider != null)
        {
            Weapon weapon = hit.collider.GetComponent<Weapon>();

            if (weapon != null)
            {
                if (_highlightedWeapon != weapon)
                {
                    if (_highlightedWeapon != null)
                    {
                        _highlightedWeapon.HideHighlight();
                    }

                    _highlightedWeapon = weapon;
                    _highlightedWeapon.ShowHighlight();
                }
            }
        }
        else // If the player is not aiming at a weapon, remove the highlight from the currently highlighted weapon
        {
            if (_highlightedWeapon != null)
            {
                _highlightedWeapon.HideHighlight();
                _highlightedWeapon = null;
            }
        }
    }

    public void TryEquipWeapon()
    {
        if (_highlightedWeapon == null) return;
        
        _weaponHolder.EquipWeapon(_highlightedWeapon, _highlightedWeapon.transform.position);
    }
}
