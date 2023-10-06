using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageMask : NetworkBehaviour
{
    [SerializeField]
    private ModeManager _modeManager;

    [SerializeField]
    private PlayerHealth _playerHealth;

    [SerializeField]
    private Canvas _canvas;

    [SerializeField]
    private Image _bipedalDamageMask;
    
    [SerializeField]
    private Image _quadripedalDamageMask;

    [SerializeField]
    private Image _slidingDamageMask;

    private void Awake()
    {
        _modeManager ??= GetComponentInParent<ModeManager>();
        _playerHealth ??= GetComponentInParent<PlayerHealth>();
        _canvas ??= GetComponent<Canvas>();

        _modeManager.OnChangeToCombat.AddListener(OnChangeToCombat);
        _modeManager.OnChangeToParkour.AddListener(OnChangeToParkour);
        _modeManager.OnChangeToSliding.AddListener(OnChangeToSliding);

        _bipedalDamageMask.gameObject.SetActive(true);
        _quadripedalDamageMask.gameObject.SetActive(true);
        _slidingDamageMask.gameObject.SetActive(true);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _canvas.worldCamera = Camera.main;
    }

    private void OnChangeToCombat()
    {
        _bipedalDamageMask.enabled = true;
        _quadripedalDamageMask.enabled = false;
        _slidingDamageMask.enabled = false;

        _bipedalDamageMask.fillAmount = 1 - (_playerHealth.Health / 100f);
    }

    private void OnChangeToParkour()
    {
        _bipedalDamageMask.enabled = false;
        _quadripedalDamageMask.enabled = true;
        _slidingDamageMask.enabled = false;

        _quadripedalDamageMask.fillAmount = 1 - (_playerHealth.Health / 100f);
    }

    private void OnChangeToSliding()
    {
        _bipedalDamageMask.enabled = false;
        _quadripedalDamageMask.enabled = false;
        _slidingDamageMask.enabled = true;

        _slidingDamageMask.fillAmount = 1 - (_playerHealth.Health / 100f);
    }

    private void Update()
    {
        _bipedalDamageMask.fillAmount = 1 - (_playerHealth.Health / 100f);
        _quadripedalDamageMask.fillAmount = 1 - (_playerHealth.Health / 100f);
        _slidingDamageMask.fillAmount = 1 - (_playerHealth.Health / 100f);
    }
}
