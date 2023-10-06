using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxManager : MonoBehaviour
{
    [SerializeField]
    private ModeManager _modeManager;

    [SerializeField]
    private GameObject _parkourHitbox;

    [SerializeField]
    private GameObject _combatHitbox;

    [SerializeField]
    private GameObject _slidingHitbox;

    private void Start()
    {
        _modeManager ??= GetComponent<ModeManager>();

        _modeManager.OnChangeToCombat.AddListener(ChangeToCombat);
        _modeManager.OnChangeToParkour.AddListener(ChangeToParkour);
        _modeManager.OnChangeToSliding.AddListener(ChangeToSliding);
    }

    private void ChangeToCombat()
    {
        _combatHitbox.SetActive(true);
        _parkourHitbox.SetActive(false);
        _slidingHitbox.SetActive(false);
    }

    private void ChangeToParkour()
    {
        _combatHitbox.SetActive(false);
        _parkourHitbox.SetActive(true);
        _slidingHitbox.SetActive(false);
    }

    private void ChangeToSliding()
    {
        _combatHitbox.SetActive(false);
        _parkourHitbox.SetActive(false);
        _slidingHitbox.SetActive(true);
    }
}
