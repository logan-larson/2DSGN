using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeathIndicator : MonoBehaviour
{
    private float _lifetime = 1f;

    private Vector3 _velocity = new Vector3(0f, 2f);

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        var position = transform.position + _velocity * Time.deltaTime;
        transform.SetPositionAndRotation(position, Quaternion.identity);
    }
}
