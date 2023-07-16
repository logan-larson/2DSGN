using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    private TMP_Text _damageValue;

    private float _initialYVelocity = 3f;
    private float _initialXVelocityRange = 1.5f;
    private float _lifetime = 0.5f;

    private Vector3 _velocity;

    private void Awake()
    {
        _damageValue = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        _velocity = new Vector3(Random.Range(-_initialXVelocityRange, _initialXVelocityRange), _initialYVelocity);

        transform.SetPositionAndRotation(transform.position + new Vector3(0f, 1f), Quaternion.identity);

        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        var position = transform.position + _velocity * Time.deltaTime;
        transform.SetPositionAndRotation(position, Quaternion.identity);
    }

    public void SetDamageValue(int damage)
    {
        _damageValue.text = damage.ToString();
    }

}
