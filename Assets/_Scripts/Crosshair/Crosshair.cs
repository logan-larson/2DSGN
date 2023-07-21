using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class Crosshair : NetworkBehaviour
{
    private Camera _camera;
    private RectTransform _crosshair;
    private WeaponEquipManager _weaponEquipManager;
    private Weapon _weapon;
    private CameraController _cameraController;
    private InputSystem _input;

    [SerializeField]
    private float _minSize = 5f;

    [SerializeField]
    private float _sizeMultiplier = 15f;

    private void ChangeWeapon()
    {
        _weapon = _weaponEquipManager.CurrentWeapon;
    }

    private void Start()
    {
        _crosshair = GetComponent<RectTransform>();
        _input = GetComponentInParent<InputSystem>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _weaponEquipManager = _weaponEquipManager ?? GetComponentInParent<WeaponEquipManager>();

        _weaponEquipManager.ChangeWeapon.AddListener(ChangeWeapon);

        ChangeWeapon();

        _camera = _weaponEquipManager.transform.GetComponent<CameraManager>().Camera;

        _cameraController = _camera.GetComponent<CameraController>();
    }

    private void Update()
    {
        if (_weapon == null) return;

        var size = Mathf.Max(_minSize * _weapon.WeaponInfo.MaxBloomAngle, _weapon.CurrentBloom * _sizeMultiplier);

        _crosshair.sizeDelta = new Vector2(size, size);

        if (_input.InputValues.IsGamepad)
        {
            if (_input.InputValues.AimInput != Vector2.zero)
            {
                var aimDirection = transform.localRotation * _input.InputValues.AimInput.normalized;

                aimDirection *= 5f;

                _crosshair.parent.position = transform.parent.parent.position + aimDirection;
            }
        }
        else
        {
            var mousePosition = Input.mousePosition;

            if (_cameraController == null || _camera == null) return;

            mousePosition.z = _cameraController.CurrentZ * -1f;

            Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint(mousePosition);

            mouseWorldPosition.z = 0f;

            _crosshair.parent.position = mouseWorldPosition;
        }
    }
}
