using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField]
    private GameObject _weapon;

    private void Awake()
    {
        _weapon = Instantiate(_weapon, transform);
    }

    public void SetWeapon(GameObject weapon)
    {
        Destroy(_weapon);
        _weapon = Instantiate(weapon, transform);
    }

    public GameObject GetWeapon()
    {
        return _weapon;
    }

}
