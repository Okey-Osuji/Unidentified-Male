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
        // Checks if the weapon is already broken before trying to use it
        if (isDepleted)
        {
            Debug.Log($"{weaponName} is broken!");
            return; // Stop the code here
        }

        // Calls the parent logic to handle the math
        ReduceDurability();

        // Triggers the firing logic (bullets)
        

        Debug.Log($"Using {weaponName}. Durability left: {durability}");

        // Checks if that last shot broke the gun
        if (durability <= 0)
        {
            isDepleted = true;
            // This is where the code is called to switch the player back to fists
            Debug.Log($"{weaponName} has broken!");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
