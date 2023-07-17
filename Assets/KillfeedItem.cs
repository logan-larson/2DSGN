using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillfeedItem : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI _playerKilled;
    [SerializeField]
    private TMPro.TextMeshProUGUI _killer;
    [SerializeField]
    private List<GameObject> _weaponObjects;


    public void Setup(string playerKilledUsername, string killerUsername, string weaponName)
    {
        Debug.Log("KillfeedItem Setup");

        _playerKilled.text = playerKilledUsername;

        foreach (var weaponObject in _weaponObjects)
        {
            if (weaponObject.name == weaponName)
            {
                weaponObject.SetActive(true);
                continue;
            }

            weaponObject.SetActive(false);
        }

        _killer.text = killerUsername;
    }
}
