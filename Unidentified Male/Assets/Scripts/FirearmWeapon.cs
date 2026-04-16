using UnityEngine;

public class FirearmWeapon : Weapon
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        isFirearm = true;// Set the isFirearm property to true for firearm weapons,
    }

    public override void Use()
    {
        ReduceDurability();// Reduce the durability of the firearm weapon by the specified degradation rate each time it is used
        Debug.Log($"Using {weaponName}. Range: {range}, Durability left : {durability}");// Log a message to indicate that the firearm weapon is being used, along with its name, range, and remaining durability for debugging purposes
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
