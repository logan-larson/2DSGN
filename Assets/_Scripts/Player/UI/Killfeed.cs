using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killfeed : NetworkBehaviour
{
    [SerializeField]
    private GameObject _killfeedItemPrefab;

    private void Start()
    {
        if (PlayerManager.Instance is null) return;

        PlayerManager.Instance.OnPlayerKilled.AddListener(OnPlayerKilled);
    }

    private void OnPlayerKilled(string playerKilledUsername, string killerUsername, string weaponName)
    {
        GameObject go = Instantiate(_killfeedItemPrefab, transform);
        go.GetComponent<KillfeedItem>().Setup(playerKilledUsername, killerUsername, weaponName);

        Destroy(go, 4f);

        OnPlayerKilledObserversRpc(playerKilledUsername, killerUsername, weaponName);
    }

    [ObserversRpc]
    private void OnPlayerKilledObserversRpc(string playerKilledUsername, string killerUsername, string weaponName)
    {
        if (base.IsHost) return;

        GameObject go = Instantiate(_killfeedItemPrefab, transform);
        go.GetComponent<KillfeedItem>().Setup(playerKilledUsername, killerUsername, weaponName);

        Destroy(go, 4f);
    }
}