using UnityEngine;

public class MeleeWeapon : Weapon
{
    protected override void Start()
    {
        base.Start();
        isFirearm = false; // Confirming this is a melee weapon
    }

    public override void Use()
    {
        // Safety Check: If it's already broken, don't let player use the weapon
        if (isDepleted)
        {
            Debug.Log($"{weaponName} is broken and cannot be used!");
            return;
        }

        // Performs the actual durability math from the Parent class
        ReduceDurability();

        // Melee-Specific Logic 
        Debug.Log($"Swinging {weaponName}. Durability: {durability}");

        // Checks if this hit was the final blow for the weapon
        if (durability <= 0)
        {
            isDepleted = true;
            Debug.Log($"{weaponName} is broken!");
            
        }
    }
}
