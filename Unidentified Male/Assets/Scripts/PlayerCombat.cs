using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;// The point from which the attack will be initiated
    public float attackRange = 0.5f;// The range of the attack  
    public LayerMask enemyLayers;// The layers that define what is considered an enemy

    [Header("Weapon Logic Settings")]
    public Weapon currentWeapon;// Reference to the current weapon being used by the player, inheriting the Weapon class defined in the project
    public float fistDamage = 8f;// Base damage for unarmed attacks (fist attacks)

    // Method to handle melee attack input from the player(M key on keyboard)
    

    // Method to handle weapon attack input from the player(N key on keyboard)
    void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;// Checks if the attack input is currently being pressed, and if so, returns early to prevent multiple attacks from being triggered while the button is held down

        if (currentWeapon != null && !currentWeapon.isDepleted)// Checks if there is a currently equipped weapon and if it is not depleted before allowing the player to perform a weapon attack
        {
           currentWeapon.Use();// Calls the Use method on the current weapon to execute its specific attack behavior 
           PerformHit(currentWeapon.damageValue, currentWeapon.range);// Call the PerformHit method with the damage value and range from the currently equipped weapon to execute the attack logic for the weapon attack
        }
        else
        {
            Debug.Log("Using fists!");// Logs a message to indicate that the player is using unarmed attacks (fist attacks) when no weapon is equipped or if the equipped weapon is depleted
            PerformHit(fistDamage, attackRange);// Call the PerformHit method with the damage value for unarmed attacks (fist attacks) to execute the attack logic when no weapon is equipped or if the equipped weapon is depleted
        }
            
    }


    // Method to perform the hit logic for both melee and weapon attacks, which detects enemies within the attack range and applies damage to them
    void PerformHit(float damage, float weaponRange)
    {
        // Detect enemies in range of the attack using Physics2D.OverlapCircleAll, which checks for colliders within a circular area defined by the attackPoint position and attackRange, and filters them based on the enemyLayers
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, enemyLayers);
    
        
        foreach (Collider2D enemy in hitEnemies)// Loops through all detected enemies and applies damage to them
        {
            Enemy target = enemy.GetComponent<Enemy>();// Gets the Enemy component from the detected enemy collider to apply damage to it
            
            if(target != null)
            {
                target.TakeDamage(damage);
                Debug.Log($"Hit {enemy.name} with {currentWeapon.weaponName} for {damage} damage!");// Logs a message to indicate that an enemy has been hit and the amount of damage dealt
            }
            
        }
            
    }

    // Method to visualize the attack range in the Unity Editor using Gizmos, which will draw a red wireframe sphere around the attackPoint to represent the area of effect for the attack 
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;// If no attackPoint is assigned, skip drawing the Gizmos

        Gizmos.color = Color.red;// Sets the Gizmos color to red for better visibility
        float visualRange = (currentWeapon != null) ? currentWeapon.range : attackRange;// Determines the visual range to draw based on whether a weapon is equipped or not, using the weapon's range if available
        Gizmos.DrawWireSphere(attackPoint.position, visualRange);// Draws a wireframe sphere
    }

    // Method to swap the player's current weapon with a new weapon prefab
    public void SwapWeapon(GameObject newWeaponPrefab)
    {
        if (currentWeapon != null)// Checks if there is a currently equipped weapon before equipping the new one
        {
            Destroy(currentWeapon.gameObject);// Destroys the current weapon game object if it exists before equipping the new weapon
        }

        GameObject spawnedWeapon = Instantiate(newWeaponPrefab, transform.position, transform.rotation, transform);// Instantiates the new weapon prefab as a child of the player at the player's position and rotation, allowing the new weapon to be visually attached to the player character in the game world

        currentWeapon = spawnedWeapon.GetComponent<Weapon>();// Gets the Weapon component from the newly instantiated weapon and assigns it to the currentWeapon reference for use in attack logic

        Debug.Log($"Equipped: {currentWeapon.weaponName}");// Logs a message to indicate that a new weapon has been equipped, displaying the name of the weapon
    }
    
}
