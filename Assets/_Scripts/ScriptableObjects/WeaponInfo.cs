using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInfo", menuName = "Weapons/WeaponInfo", order = 1)]
public class WeaponInfo : ScriptableObject
{
    public string Name;
    public int Damage;
    public float FireRate;
    public float Range;

    public bool IsAutomatic = true;
    public int BulletsPerShot = 1;
    public float SpreadAngle = 0f;
    public float MaxBloomAngle = 0f;
    public float BloomAngleIncreasePerShot = 0f;

    public float Knockback = 0f;
}
