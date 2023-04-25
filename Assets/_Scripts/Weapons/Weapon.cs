using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponInfo WeaponInfo;
    public SpriteRenderer WeaponSprite;
    public SpriteRenderer MuzzleFlashSprite;

    public void Equip()
    {
        ShowWeapon();
        transform.localPosition = WeaponInfo.Position;
    }

    public void ShowWeapon()
    {
        WeaponSprite.enabled = true;
    }

    public void HideWeapon()
    {
        WeaponSprite.enabled = false;
    }

    public void FlipY(bool flipY)
    {
        WeaponSprite.flipY = flipY;
        MuzzleFlashSprite.flipY = flipY;
    }

    public void ShowMuzzleFlash()
    {
        MuzzleFlashSprite.enabled = true;
        StartCoroutine(DisableMuzzleFlash());
    }

    private IEnumerator DisableMuzzleFlash()
    {
        yield return new WaitForSeconds(0.05f);
        MuzzleFlashSprite.enabled = false;
    }

    public void ShowHighlight()
    {
        WeaponSprite.color = Color.yellow;
    }

    public void HideHighlight()
    {
        WeaponSprite.color = Color.white;
    }

    public void DropWeapon(Vector3 dropPosition)
    {
        transform.position = dropPosition;
        ShowWeapon();
    }
}