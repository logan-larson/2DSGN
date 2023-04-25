using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponSprites", menuName = "Weapons/WeaponSprites", order = 2)]
public class WeaponSprites : ScriptableObject
{
    public List<string> Names = new List<string>();
    public List<Sprite> Sprites = new List<Sprite>();
}
