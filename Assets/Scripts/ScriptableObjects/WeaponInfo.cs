using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInfo", menuName = "Weapons/WeaponInfo", order = 1)]
public class WeaponInfo : ScriptableObject
{
    public string WeaponName;
    public Sprite WeaponSprite;
    public int Damage;
    public float FireRate;
    public float Range;
    public float ReloadTime;
    public float ClipSize;

}
