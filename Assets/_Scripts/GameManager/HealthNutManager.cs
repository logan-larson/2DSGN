using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using System.Linq;
using FishNet.Connection;

public class HealthNutManager : NetworkBehaviour
{

    private HealthNut _highlightedHealthNut = null;

    [SerializeField]
    private PlayerHealth _playerHealth;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner) return;

        _playerHealth ??= GetComponent<PlayerHealth>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _playerHealth ??= GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (!base.IsOwner) return;

        HighlightNut();
    }

    private void HighlightNut()
    {
        HealthNut[] nuts = GameObject
            .FindGameObjectsWithTag("HealthNut")
            .Select(g => g.GetComponent<HealthNut>())
            .ToArray();

        HealthNut closestNut = null;
        float closestDistance = 5f;
        Vector3 referencePosition = transform.position;

        foreach (HealthNut nut in nuts)
        {
            nut.HideHighlight();

            float distance = Vector3.Distance(nut.transform.position, referencePosition);

            if (nut.IsAvailable && distance < closestDistance)
            {
                closestNut = nut;
                closestDistance = distance;
            }
        }

        if (closestNut != null && _playerHealth.Health != 100)
        {
            _highlightedHealthNut = closestNut;
            closestNut.ShowHighlight();
        }
        else
        {
            _highlightedHealthNut = null;
        }
    }

    public void TryPickupHealthNut()
    {
        if (!base.IsOwner) return;

        if (_highlightedHealthNut == null) return;

        PickupHealthNutServerRpc(_highlightedHealthNut.HealthNutID);
    }

    [ServerRpc]
    private void PickupHealthNutServerRpc(int healthNutID)
    {
        Debug.Log("Got to server");

        HealthNut nut = GameObject
            .FindGameObjectsWithTag("HealthNut")
            .Select(g => g.GetComponent<HealthNut>())
            .Where(n => n.HealthNutID == healthNutID)
            .FirstOrDefault();

        if (nut == null) return;

        if (!nut.IsAvailable) return;

        nut.Pickup();

        _playerHealth.ResetHealth();
    }
}

