using UnityEngine;

public class MeleeWeapon : Weapon
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()// Override the Start method to perform initialization specific to melee weapons
    {
        base.Start();// Call the base Start method to ensure any initialization in the Weapon class is executed
        isFirearm = false;// Set the isFirearm property to false for melee weapons, indicating that this weapon does not use ammunition and is not a firearm
    }


    public override void Use()
    {
        ReduceDurability();// Reduce the durability of the melee weapon by the specified degradation rate each time it is used
        Debug.Log($"Using {weaponName}. Range: {range}, Durability left : {durability}");// Log a message to indicate that the melee weapon is being used, along with its name, range, and remaining durability for debugging purposes
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
