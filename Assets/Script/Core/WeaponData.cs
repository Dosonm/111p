using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Weapon Data", fileName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    public WeaponId weaponId;

    public WeaponRarity rarity;

    public Sprite icon;

    public string description;
}
