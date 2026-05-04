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
    private int currentWeaponIndex = -1;// Index of items in inventory, with -1 meaning the player is using hand-to-hand combat
    public int maxInventory = 3;// Maximum amount of items allowed in inventory

    [Header("Visual Swapping")]
    public SpriteRenderer playerRenderer; // The "body" of the player
    public Sprite unarmedSprite;
    public Sprite pistolSprite;
    public Sprite rifleSprite;
    public Sprite pipeSprite;
    public Sprite knifeSprite;

    [Header("AttackPoint Offsets")]
    public Vector2 unarmedOffset = new Vector2(0.5f, 2f);
    public Vector2 pistolOffset = new Vector2(0f, 5.9f); 
    public Vector2 rifleOffset = new Vector2(7.2f, 5.2f);
    public Vector2 pipeOffset = new Vector2(0.5f, 2f);
    public Vector2 knifeOffset = new Vector2(0.5f, 2f);


    // Method to handle weapon and hand attack inputs from the player(N key on keyboard)
    void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;// Checks if the attack input is currently being pressed, and if so, returns early to prevent multiple attacks from being triggered while the button is held down

        if (currentWeapon != null && !currentWeapon.isDepleted)// Checks if there is a currently equipped weapon and if it is not depleted before allowing the player to perform a weapon attack
        {
            // Triggers the durability reduction logic and any Debug.Log messages
            currentWeapon.Use();

            // NEW LOGIC: Only perform the "Circle" hit if the weapon is NOT a firearm
            // This prevents guns from killing everything in a radius instantly
            if (!currentWeapon.isFirearm)
            {
                PerformHit(currentWeapon.damageValue, currentWeapon.range);// Call the PerformHit method with the damage value and range from the currently equipped weapon to execute the attack logic for the weapon attack
            }

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

    // Method to visualize the attack range in the Unity Editor using Gizmos
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
        unlockedWeapons.RemoveAll(item => item == null);

        // CAPACITY CHECK: Now checks the count based on surviving weapons
        if (unlockedWeapons.Count >= maxInventory)
        {
            Debug.Log("Inventory Full! Cannot add " + prefab.name);
            return false;
        }

        // INSTANTIATION: Creates the new weapon as a child of the player
        GameObject newWep = Instantiate(prefab, attackPoint.position, attackPoint.rotation, transform);

        // Hides the physical pickup sprite/collider because the player's main sprite handles weapon visuals
        SetInventoryWeaponState(newWep, true);

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

    public bool PickUpOrSwapWeapon(GameObject prefab, Vector3 pickupPosition)
    {
        // Safety check to make sure the pickup actually has a weapon prefab assigned in the Inspector
        if (prefab == null)
        {
            Debug.LogWarning("Cannot pick up weapon because the pickup has no weapon prefab assigned.");
            return false;
        }

        // Clears out any broken or missing weapons before checking the inventory size
        unlockedWeapons.RemoveAll(item => item == null);

        // If the player still has space, the weapon is added normally
        if (unlockedWeapons.Count < maxInventory)
        {
            return AddNewWeapon(prefab);
        }

        // If inventory is full and the player is using fists, there is no weapon in hand to swap out
        if (currentWeapon == null)
        {
            Debug.Log("Inventory Full! Equip a weapon with Q before swapping with " + prefab.name + ".");
            return false;
        }

        // If inventory is full and a weapon is equipped, swap the equipped weapon with the floor weapon
        return SwapCurrentWeapon(prefab, pickupPosition);
    }

    bool SwapCurrentWeapon(GameObject newWeaponPrefab, Vector3 pickupPosition)
    {
        // Finds the equipped weapon inside the inventory list so it can be replaced
        int equippedIndex = unlockedWeapons.IndexOf(currentWeapon.gameObject);
        if (equippedIndex < 0)
        {
            Debug.Log("Inventory Full! Could not find the equipped weapon to swap.");
            return false;
        }

        // Drops a copy of the current weapon onto the floor where the pickup was
        GameObject droppedWeapon = Instantiate(currentWeapon.gameObject, pickupPosition, Quaternion.identity);
        droppedWeapon.transform.SetParent(null);
        droppedWeapon.SetActive(true);
        SetInventoryWeaponState(droppedWeapon, false);// Restores the dropped weapon so it can be seen and picked up again

        // Stores the old weapon name before destroying it so it can be used in Debug.Log
        string oldWeaponName = currentWeapon.weaponName;

        // Removes the equipped weapon from the player before replacing it with the new weapon
        Destroy(currentWeapon.gameObject);

        // Creates the new weapon as a child of the player and equips it immediately
        GameObject newWeapon = Instantiate(newWeaponPrefab, attackPoint.position, attackPoint.rotation, transform);
        newWeapon.SetActive(true);
        SetInventoryWeaponState(newWeapon, true);// Hides the tiny weapon sprite because the player sprite already shows the equipped weapon

        // Replaces the old inventory slot with the new weapon so inventory size stays the same
        unlockedWeapons[equippedIndex] = newWeapon;
        currentWeaponIndex = equippedIndex;
        currentWeapon = newWeapon.GetComponent<Weapon>();

        UpdatePlayerAppearance();
        Debug.Log($"Swapped {oldWeaponName} for {currentWeapon.weaponName}.");
        return true;
    }

    void SetInventoryWeaponState(GameObject weaponObject, bool isInInventory)
    {
        // Inventory weapons are used for their script data, not for their small pickup sprite
        SpriteRenderer[] renderers = weaponObject.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer weaponRenderer in renderers)
        {
            weaponRenderer.enabled = !isInInventory;
        }

        // Disables pickup colliders while the weapon is held so it does not act like a floor item
        Collider2D[] colliders = weaponObject.GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D weaponCollider in colliders)
        {
            weaponCollider.enabled = !isInInventory;
        }

        // Disables the pickup script while held, but turns it back on when the weapon is dropped
        WeaponPickup pickup = weaponObject.GetComponent<WeaponPickup>();
        if (pickup != null)
        {
            pickup.enabled = !isInInventory;
        }
    }

    void CycleWeapon()
    {
        // Checks if the current weapon object still exists before deactivating
        if (currentWeapon != null && currentWeapon.gameObject != null)
        {
            currentWeapon.gameObject.SetActive(false);
        }

        unlockedWeapons.RemoveAll(item => item == null);

        // If there are no weapons in the inventory, Q should still put the player on fists
        if (unlockedWeapons.Count == 0)
        {
            EquipHands();
            return;
        }

        // Cycles through all weapons, then one extra hand-to-hand slot
        currentWeaponIndex++;

        // If the index passes the last weapon, switch to fists instead of counting fists as inventory
        if (currentWeaponIndex >= unlockedWeapons.Count)
        {
            EquipHands();
            return;
        }

        EquipWeaponFromIndex(currentWeaponIndex);
    }

    void EquipWeaponFromIndex(int index)
    {
        if (unlockedWeapons.Count == 0)
        {
            EquipHands();
            return;
        }

        // If the index is outside the list, the player is safely switched to fists
        if (index < 0 || index >= unlockedWeapons.Count)
        {
            EquipHands();
            return;
        }

        GameObject selection = unlockedWeapons[index];

        // Checks if the object actually exists in the game world
        if (selection != null)
        {
            selection.SetActive(true);
            SetInventoryWeaponState(selection, true);// Keeps the equipped inventory weapon invisible while its stats still work
            currentWeaponIndex = index;
            currentWeapon = selection.GetComponent<Weapon>();
            UpdatePlayerAppearance();// Updates the main player sprite after switching to this weapon
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

    void EquipHands()
    {
        // Sets the index to -1 so fists are treated as a toggle option, not an inventory item
        currentWeaponIndex = -1;
        currentWeapon = null;
        UpdatePlayerAppearance();
        Debug.Log("Switched to: Fists");
    }

    void Start()
    {
        unlockedWeapons.Clear();
        // If the list is empty but the player has a weapon equipped, add it to the count
        if (unlockedWeapons.Count == 0 && currentWeapon != null)
        {
            unlockedWeapons.Add(currentWeapon.gameObject);
            SetInventoryWeaponState(currentWeapon.gameObject, true);// Hides any starting weapon object so only the player sprite is visible
            currentWeaponIndex = 0;
            Debug.Log("Starting weapon registered. Inventory: 1/" + maxInventory);
        }

        // Visuals: Makes sure we don't start with a white square
        if (playerRenderer == null) playerRenderer = FindPlayerRenderer();// Finds the player body sprite if it was not assigned in the Inspector
        UpdatePlayerAppearance();
    }

    void Update()
    {
        // Handle Weapon Switching with Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleWeapon();
        }

    }

    // This handles the "Art" swap based on the current weapon's name
    public void UpdatePlayerAppearance()
    {
        // Safety cleanup for the list
        unlockedWeapons.RemoveAll(item => item == null);

        // If no weapon is equipped or it's depleted, show unarmed player
        if (currentWeapon == null || currentWeapon.isDepleted)
        {
            SetPlayerSprite(unarmedSprite, "Unarmed");// Shows the player without a weapon when using fists
            attackPoint.localPosition = unarmedOffset;//Moves attackPoint to the unarmed attack position, which is the default attackPoint
            return;
        }

        // We check the weaponName string from the Weapon class to determine the sprite
        string name = currentWeapon.weaponName.ToLower();

        if (name.Contains("pistol"))
        {
            SetPlayerSprite(pistolSprite, "Pistol");// Shows the player sprite that has the pistol built into it
            attackPoint.localPosition = pistolOffset;//Moves attackPoint to the pistol's gun barrel
        }
        else if (name.Contains("rifle"))
        {
            SetPlayerSprite(rifleSprite, "Rifle");// Shows the player sprite that has the rifle built into it
            attackPoint.localPosition = rifleOffset;//Moves attackPoint to the rifle's gun barrel
        }
        else if (name.Contains("pipe"))
        {
            SetPlayerSprite(pipeSprite, "Pipe");// Shows the player sprite that has the pipe built into it
            attackPoint.localPosition = pipeOffset;
        }
        else if (name.Contains("knife"))
        {
            SetPlayerSprite(knifeSprite, "Knife");// Shows the player sprite that has the knife built into it
            attackPoint.localPosition = knifeOffset;
        }
        else
        {
            SetPlayerSprite(unarmedSprite, "Unarmed");// Fallback sprite if the weapon name does not match any known weapon type
            attackPoint.localPosition = unarmedOffset;
        }
    }

    void SetPlayerSprite(Sprite newSprite, string spriteName)
    {
        // Safety check in case the playerRenderer was not assigned in the Inspector
        if (playerRenderer == null)
        {
            Debug.LogWarning("Player Renderer is missing, so the player sprite cannot be changed.");
            return;
        }

        // Safety check so missing weapon sprites do not make the player turn invisible
        if (newSprite == null)
        {
            Debug.LogWarning(spriteName + " sprite is not assigned in PlayerCombat.");
            return;
        }

        playerRenderer.sprite = newSprite;// Changes the main player sprite to match the equipped weapon
    }

    SpriteRenderer FindPlayerRenderer()
    {
        // First checks for a SpriteRenderer on the player object itself
        SpriteRenderer directRenderer = GetComponent<SpriteRenderer>();
        if (directRenderer != null) return directRenderer;

        // Then checks child SpriteRenderers, but skips weapon objects so we don't swap the tiny weapon sprite
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer childRenderer in childRenderers)
        {
            if (childRenderer.GetComponentInParent<Weapon>() == null)
            {
                return childRenderer;
            }
        }

        return null;
    }

}
