using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageMask : MonoBehaviour
{
    private Vector3 _quadPos = new Vector3(-0.261f, -0.711f, 0);
    private Vector3 _quadScale = new Vector3(3f, 0f, 0);
    private float _quadScaleYMax = 1.14f;

    private Vector3 _biPos = new Vector3(-0.261f, -0.75f, 0);
    private Vector3 _biScale = new Vector3(3f, 0f, 0);
    private float _biScaleYMax = 2.10f;

    private float _currentScaleY = 0f;

    private ModeManager _modeManager;
    private PlayerHealth _playerHealth;

    [SerializeField]
    private Transform _damageMask;

    private void Awake()
    {
        _modeManager = GetComponentInParent<ModeManager>();
        _playerHealth = GetComponentInParent<PlayerHealth>();

        _modeManager.OnChangeToCombat.AddListener(OnChangeToCombat);
        _modeManager.OnChangeToParkour.AddListener(OnChangeToParkour);
    }

    private void OnChangeToCombat()
    {
        _damageMask.localPosition = _biPos;
        _damageMask.localScale = _biScale;
    }

    private void OnChangeToParkour()
    {
        _damageMask.localPosition = _quadPos;
        _damageMask.localScale = _quadScale;
    }

    void Update()
    {
        if (_modeManager.CurrentMode == ModeManager.Mode.Combat)
        {
            _currentScaleY = ((100f - _playerHealth.Health) / 100f) * _biScaleYMax;
        }
        else if (_modeManager.CurrentMode == ModeManager.Mode.Parkour)
        {
            _currentScaleY = ((100f - _playerHealth.Health) / 100f) * _quadScaleYMax;
        }

        _damageMask.localScale = new Vector3(_damageMask.localScale.x, _currentScaleY, _damageMask.localScale.z);
    }
}
