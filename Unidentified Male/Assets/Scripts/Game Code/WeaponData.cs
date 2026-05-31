using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Inventory/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;   // e.g., "Pistol", "Lead Pipe", "Rifle"
    public int weaponID;        // The integer ID the Animator uses (0, 1, 2, 4, etc.)
    public Sprite weaponIcon;   // To show icons
}