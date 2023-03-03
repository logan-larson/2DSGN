using UnityEngine;

public class CombatSystem : MonoBehaviour
{

    [SerializeField]
    private GameObject _weaponHolder;

    [SerializeField]
    private MovementSystem _movementSystem;

    void Start()
    { // public void OnStart
    }

    void Update()
    { // public void OnUpdate
        if (_movementSystem.PublicData.InCombatMode)
        {
            _weaponHolder.SetActive(true);
        }
        else
        {
            _weaponHolder.SetActive(false);
        }
    }

    public void ShootPrimary()
    {
    }
}
