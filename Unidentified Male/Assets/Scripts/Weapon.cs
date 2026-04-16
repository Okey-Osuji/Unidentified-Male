using UnityEngine;

public abstract class Weapon : MonoBehaviour
{

    [Header("Identity")]
    public string weaponName;// The name of the weapon, which can be used to identify the weapon in the game and provide information to the player about its attributes and capabilities
    public bool isFirearm;// Flag to indicate whether the weapon is a firearm, which can affect its attack behavior and range compared to melee weapons
    
    
    [Header("Base Stats")]
    public float damageValue;// The damage value of the weapon, which determines how much damage it deals to enemies when used in combat
    public float attackRate = 0.5f;// The attack rate of the weapon, which determines how quickly the weapon can be used in combat, with lower values allowing for faster attacks
    public float range = 1.2f;// The range of the weapon, which determines how far the weapon can reach when used in combat, with higher values allowing for attacks from a greater distance
    
    [Header("Durability logic")]
    public float durability = 100f;// The durability of the weapon, which determines how long the weapon can be used before it breaks, with higher values allowing for longer use in combat 
    public float degradationRate = 5f;// The rate at which the weapon's durability degrades with use
    public bool isDepleted = false;// Flag to indicate whether the weapon's durability has been depleted, which can be used to prevent the weapon from being used in combat when it is broken



    
    // Start is called before the first frame update
    //Protected refers to methods only accesible by the parent class and its child classes
    protected virtual void Start()// Virtual Start method to allow child classes to override and implement their own initialization logic, which can include setting specific attributes for different types of weapons and preparing the weapon for use in combat
    {
        
    }

    // Method to reduce the durability of the weapon each time it is used in combat, which can lead to the weapon breaking when durability reaches zero
    public void ReduceDurability()
    {

        float amount = degradationRate;
        
        if (isDepleted) return;// If the weapon is already depleted, skip durability reduction

        durability -= degradationRate;// Reduce the durability of the weapon by the specified degradation rate each time it is used in combat
        durability = Mathf.Max(durability, 0f);// Ensure that durability does not go below zero

        if (durability <= 0f)
        {
            HandleDepletion();
        }
    }

    // Method to handle the logic when the weapon's durability is depleted
    protected virtual void HandleDepletion()
    {
        isDepleted = true;// Set the weapon's depleted status to true when durability reaches zero, which can be used to prevent the weapon from being used in combat 
        Debug.Log($"{weaponName} has broken!");// Log a message to indicate that the weapon has broken, 
    }

    // Abstract method to be implemented by child classes to define the specific behavior of using the weapon in combat, which can vary based on whether the weapon is a firearm or a melee weapon and can include logic for dealing damage, playing animations, and applying effects to enemies
    public abstract void Use();

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
