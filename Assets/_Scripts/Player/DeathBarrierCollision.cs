using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class DeathBarrierCollision : NetworkBehaviour
{
    [SerializeField]
    private PlayerHealth _playerHealth;

    public override void OnStartClient()
    {
        base.OnStartClient();

        _playerHealth ??= GetComponent<PlayerHealth>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!base.IsOwner) return;

        if (collision.gameObject.CompareTag("DeathBarrier"))
        {
            _playerHealth.TakeDamageServerRpc(100); 
        }
    }
}
