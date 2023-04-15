using System;
using FishNet.Object;
using UnityEngine;

/**
<summary>
FirstObjectNotifier provides an event that is invoked when the object it is attached to is spawned.
</summary>
*/
public class FirstObjectNotifier : NetworkBehaviour
{

    public static event Action<Transform, GameObject> OnFirstObjectSpawned;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            NetworkObject nob = base.LocalConnection.FirstObject;
            if (nob == base.NetworkObject)
                OnFirstObjectSpawned?.Invoke(transform, base.gameObject);
        }
    }
}
