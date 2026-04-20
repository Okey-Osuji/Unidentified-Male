using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;// The point from which the attack will be initiated
    public float attackRange = 0.5f;// The range of the attack  
    public LayerMask enemyLayers;// The layers that define what is considered an enemy

    [Header("Weapon Logic Settings")]
    public Weapon currentWeapon;// Reference to the current weapon being used by the player, inheriting the Weapon class defined in the project
    public float fistDamage = 10f;// Base damage for unarmed attacks (fist attacks)

    [Header("Inventory")]
    public List<GameObject> unlockedWeapons = new List<GameObject>();
    private int currentWeaponIndex = 0;// Index of items in inventory
    public int maxInventory = 3;// Maximum amount of items allowed in invetory

    [Header("Visual Swapping")]
    public SpriteRenderer playerRenderer; // The "body" of the player
    public Sprite unarmedSprite;
    public Sprite pistolSprite;
    public Sprite rifleSprite;
    public Sprite pipeSprite;
    public Sprite knifeSprite;


    // Method to handle weapon and hand attack inputs from the player(N key on keyboard)
    void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;// Checks if the attack input is currently being pressed, and if so, returns early to prevent multiple attacks from being triggered while the button is held down

        if (currentWeapon != null && !currentWeapon.isDepleted)// Checks if there is a currently equipped weapon and if it is not depleted before allowing the player to perform a weapon attack
        {
            // Triggers the durability reduction logic and any Debug.Log messages
            currentWeapon.Use();

            PerformHit(currentWeapon.damageValue, currentWeapon.range);// Call the PerformHit method with the damage value and range from the currently equipped weapon to execute the attack logic for the weapon attack

            // Checks if the weapon broke on the last hit to update visuals immediately
            if (currentWeapon.isDepleted)
            {
                UpdatePlayerAppearance();
            }
        }
        else
        {
            UpdatePlayerAppearance(); // Syncs visuals if the weapon just broke
            Debug.Log("Using fists!");// Logs a message to indicate that the player is using unarmed attacks (fist attacks) when no weapon is equipped or if the equipped weapon is depleted
            PerformHit(fistDamage, attackRange);// Call the PerformHit method with the damage value for unarmed attacks (fist attacks) to execute the attack logic when no weapon is equipped or if the equipped weapon is depleted
        }

    }


    // Method to perform the hit logic for both melee and weapon attacks, which detects enemies within the attack range and applies damage to them
    void PerformHit(float damage, float weaponRange)
    {
        // Detects enemies in range of the attack using Physics2D.OverlapCircleAll, which checks for colliders within a circular area defined by the attackPoint position and attackRange, and filters them based on the enemyLayers
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, enemyLayers);


        foreach (Collider2D enemy in hitEnemies)// Loops through all detected enemies and applies damage to them
        {
            Enemy target = enemy.GetComponent<Enemy>();// Gets the Enemy component from the detected enemy collider to apply damage to it

            if (target != null)
            {
                target.TakeDamage(damage);

                // Checks if weapon exists before logging name
                string attackerName = (currentWeapon != null) ? currentWeapon.weaponName : "Fists";
                Debug.Log($"Hit {enemy.name} with {attackerName} for {damage} damage!");// Logs a message to indicate that an enemy has been hit and the amount of damage dealt
            }

        }

    }

    // Method to visualize the attack range in the Unity Editor using Gizmos, which will draw a red wireframe sphere around the attackPoint to represent the area of effect for the attack 
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;// If no attackPoint is assigned, skip drawing the Gizmos

        Gizmos.color = Color.red;// Sets the Gizmos color to red for better visibility
        // Uses weapon range if available, otherwise default to fist range
        float visualRange = (currentWeapon != null && !currentWeapon.isDepleted) ? currentWeapon.range : attackRange;
        Gizmos.DrawWireSphere(attackPoint.position, visualRange);// Draws a wireframe sphere
    }



    public bool AddNewWeapon(GameObject prefab)
{
    // CLEANUP : Remove any "Ghost" or "Missing" weapons from the list
    // This ensures that if a weapon broke, it doesn't take up a slot anymore
    unlockedWeapons.RemoveAll(item => item == null);

    // CAPACITY CHECK: Now checks the count based on surviving weapons
    if (unlockedWeapons.Count >= maxInventory)
    {
        Debug.Log("Inventory Full! Cannot add " + prefab.name);
        return false;
    }

    // INSTANTIATION: Creates the new weapon as a child of the player
    GameObject newWep = Instantiate(prefab, attackPoint.position, attackPoint.rotation, transform);

    // Hides it immediately so we don't hold multiple weapons at once
    newWep.SetActive(false);

    // Adds to our 'Q' list of weapons
    unlockedWeapons.Add(newWep);

    Debug.Log($"Added {newWep.name} to inventory. Current count: {unlockedWeapons.Count}/{maxInventory}");

    // AUTO-EQUIP: If it's our first weapon, equip it automatically
    if (unlockedWeapons.Count == 1)
    {
        EquipWeaponFromIndex(0);
        UpdatePlayerAppearance();
    }

    return true;
}

    void CycleWeapon()
    {
        // Checks if the current weapon object still exists before deactivating
        if (currentWeapon != null && currentWeapon.gameObject != null)
        {
            currentWeapon.gameObject.SetActive(false);
        }

        if (unlockedWeapons.Count == 0) return;

        // Moves to next weapon in index
        currentWeaponIndex = (currentWeaponIndex + 1) % unlockedWeapons.Count;

        // Equips new weapon from index
        EquipWeaponFromIndex(currentWeaponIndex);
    }

    void EquipWeaponFromIndex(int index)
    {
        GameObject selection = unlockedWeapons[index];

        // Checks if the object actually exists in the game world
        if (selection != null)
        {
            selection.SetActive(true);
            currentWeapon = selection.GetComponent<Weapon>();
            Debug.Log($"Switched to: {currentWeapon.weaponName}");
        }
        else
        {
            // If the weapon was destroyed (durability hit 0), remove it from the list
            unlockedWeapons.RemoveAt(index);

            // If the players still has weapons, try to equip the next one
            if (unlockedWeapons.Count > 0)
            {
                currentWeaponIndex = 0; // Reset to start
                EquipWeaponFromIndex(currentWeaponIndex);
            }
            else
            {
                currentWeapon = null;
                UpdatePlayerAppearance();
            }
        }
    }

    void Start()
    {
        unlockedWeapons.Clear();
        // If the list is empty but the player have a weapon equipped, add it to the count
        if (unlockedWeapons.Count == 0 && currentWeapon != null)
        {
            unlockedWeapons.Add(currentWeapon.gameObject);
            Debug.Log("Starting weapon registered. Inventory: 1/" + maxInventory);
        }

        // Visuals: Makes sure we don't start with a white square
        if (playerRenderer == null) playerRenderer = GetComponent<SpriteRenderer>();
        UpdatePlayerAppearance();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && unlockedWeapons.Count > 1)
        {
            CycleWeapon();
            UpdatePlayerAppearance(); // Sync visuals after cycle
        }
    }


    // This handles the "Art" swap based on the current weapon's name
    void UpdatePlayerAppearance()
    {
        // If no weapon is equipped or it's depleted, show unarmed player
        if (currentWeapon == null || currentWeapon.isDepleted)
        {
            playerRenderer.sprite = unarmedSprite;
            return;
        }

        // We check the weaponName string from the Weapon class
        string name = currentWeapon.weaponName.ToLower();

        if (name.Contains("pistol"))
        {
            playerRenderer.sprite = pistolSprite;
        }
        else if (name.Contains("rifle"))
        {
            playerRenderer.sprite = rifleSprite;
        }
        else if (name.Contains("pipe"))
        {
            playerRenderer.sprite = pipeSprite;
        }
        else if (name.Contains("knife"))
        {
            playerRenderer.sprite = knifeSprite;
        }
        else
        {
            // Default fallback
            playerRenderer.sprite = unarmedSprite;
        }
    }



}
