using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
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
    public List<GameObject> unlockedWeapons = new List<GameObject>();// Stores the weapon objects that the player has picked up and can cycle through
    private int currentWeaponIndex = -1;// Index of items in inventory, with -1 meaning the player is using hand-to-hand combat
    public int maxInventory = 3;// Maximum amount of items allowed in inventory

    [Header("Visual Swapping")]
    public SpriteRenderer playerRenderer; // The "body" of the player
    public Animator playerAnimator;// Movement animations can overwrite playerRenderer.sprite, so this is paused while weapon sprites are shown
    public Sprite unarmedSprite;// The sprite shown when the player has no weapon equipped
    public Sprite pistolSprite;// The idle sprite shown when the player has a pistol equipped
    public Sprite rifleSprite;// The idle sprite shown when the player has a rifle equipped
    public Sprite pipeSprite;// The idle sprite shown when the player has a pipe equipped
    public Sprite knifeSprite;// The idle sprite shown when the player has a knife equipped
    [SerializeField] private AudioSource audioSource;// Reference to the AudioSource component for playing sound effects
    [SerializeField] private AudioClip pickupSound; // Sound effect for picking up a weapon
    [SerializeField] private AudioClip attackSound; // Sound effect for switching weapons
    [SerializeField] private AudioClip hitSound; // Sound effect for switching weapons
    [Header("Animation Parameters")]
    public bool driveAnimatorWeaponStates = true;// Allows the script to control the player's Animator weapon states when this is turned on
    public string weaponTypeParameter = "WeaponType";// The Animator integer parameter used to tell animations which weapon is equipped
    public string movingParameter = "IsMoving";// The Animator bool parameter used to tell animations whether the player is moving
    public string attackTriggerParameter = "Attack";// The Animator trigger parameter used to play attack animations
    public float idleVisualRestoreDelay = 0.05f;// Small delay before returning to the idle weapon sprite after attacking while standing still

    [Header("Walk Animation Clips")]
    public AnimationClip unarmedWalkClip;// The normal player walking animation, used as the base clip that weapon walks replace
    public AnimationClip pistolWalkClip;// The walking animation used when the player has a pistol equipped
    public AnimationClip rifleWalkClip;// The walking animation used when the player has a rifle equipped
    public AnimationClip pipeWalkClip;// The walking animation used when the player has a pipe equipped
    public AnimationClip knifeWalkClip;// The walking animation used when the player has a knife equipped

    [Header("AttackPoint Offsets")]
    public Vector2 unarmedOffset = new Vector2(0.5f, 2f);// Local attackPoint position used for fist attacks
    public Vector2 pistolOffset = new Vector2(0f, 5.9f);// Local attackPoint position used for pistol shots
    public Vector2 rifleOffset = new Vector2(-2.8f, 7.9f);// Local attackPoint position used for rifle shots
    public Vector2 pipeOffset = new Vector2(0.5f, 2f);// Local attackPoint position used for pipe attacks
    public Vector2 knifeOffset = new Vector2(0.5f, 2f);// Local attackPoint position used for knife attacks

    private bool isOverridingAnimatorSprite;// Tracks whether the Animator is currently disabled so an idle weapon sprite can be shown
    private bool isPlayerMoving;// Tracks whether the movement script says the player is currently moving
    private Coroutine idleVisualRestoreRoutine;// Stores the delayed idle restore coroutine so it can be cancelled when switching weapons
    private AnimatorOverrideController walkOverrideController;// Runtime copy of the Animator Controller that lets us swap only the walking clip without editing the controller asset
    private AnimationClip baseWalkClip;// The original walk clip inside the Animator Controller that gets replaced by the current weapon's walk clip
    private Weapon.WeaponAnimationType appliedWalkAnimationType = Weapon.WeaponAnimationType.Auto;// Tracks which weapon walk clip is already applied so we do not keep replacing it every frame


    // Method to handle weapon and hand attack inputs from the player(N key on keyboard)
    void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;// Checks if the attack input is currently being pressed, and if so, returns early to prevent multiple attacks from being triggered while the button is held down

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);// Plays the attack sound effect when performing an attack
        }

        TriggerAttackAnimation();// Plays the attack animation trigger before running the weapon's damage logic

        if (currentWeapon != null && !currentWeapon.isDepleted)// Checks if there is a currently equipped weapon and if it is not depleted before allowing the player to perform a weapon attack
        {
            // Triggers the durability reduction logic and any Debug.Log messages
            currentWeapon.Use();// Uses the equipped weapon, which handles durability loss and weapon-specific attack behavior

            // NEW LOGIC: Only perform the "Circle" hit if the weapon is NOT a firearm
            // This prevents guns from killing everything in a radius instantly
            if (!currentWeapon.isFirearm)
            {
                PerformHit(currentWeapon.damageValue, currentWeapon.range);// Call the PerformHit method with the damage value and range from the currently equipped weapon to execute the attack logic for the weapon attack
            }

            // Checks if the weapon broke on the last hit to update visuals immediately
            if (currentWeapon.isDepleted)
            {
                UpdatePlayerAppearance();// Refreshes the player visuals if the weapon broke during this attack
            }
        }
        else
        {
            UpdatePlayerAppearance(); // Syncs visuals if the weapon just broke
            Debug.Log("Using fists!");// Logs a message to indicate that the player is using unarmed attacks (fist attacks) when no weapon is equipped or if the equipped weapon is depleted
            PerformHit(fistDamage, attackRange);// Call the PerformHit method with the damage value for unarmed attacks (fist attacks) to execute the attack logic when no weapon is equipped or if the equipped weapon is depleted
        }

        RestoreIdleVisualAfterAttack();// Returns to the correct idle weapon sprite after attacking while standing still
    }


    // Method to perform the hit logic for both melee and weapon attacks, which detects enemies within the attack range and applies damage to them
    void PerformHit(float damage, float weaponRange)
    {
        // Detects enemies in range of the attack using Physics2D.OverlapCircleAll, which checks for colliders within a circular area defined by the attackPoint position and attackRange, and filters them based on the enemyLayers
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, enemyLayers);// Stores every enemy collider found inside the attack range


        foreach (Collider2D enemy in hitEnemies)// Loops through all detected enemies and applies damage to them
        {
            Enemy target = enemy.GetComponent<Enemy>();// Gets the Enemy component from the detected enemy collider to apply damage to it

            if (target != null)
            {
                target.TakeDamage(damage);// Applies the final damage amount to the enemy that was hit

                // Checks if weapon exists before logging name
                string attackerName = (currentWeapon != null) ? currentWeapon.weaponName : "Fists";// Chooses the equipped weapon name, or "Fists" if the player is unarmed
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
        float visualRange = (currentWeapon != null && !currentWeapon.isDepleted) ? currentWeapon.range : attackRange;// Chooses the weapon range for the Gizmo when a valid weapon is equipped
        Gizmos.DrawWireSphere(attackPoint.position, visualRange);// Draws a wireframe sphere
    }

    public bool AddNewWeapon(GameObject prefab)
    {
        // CLEANUP : Remove any "Ghost" or "Missing" weapons from the list
        unlockedWeapons.RemoveAll(item => item == null);// Removes destroyed or missing weapon references before adding a new weapon

        // CAPACITY CHECK: Now checks the count based on surviving weapons
        if (unlockedWeapons.Count >= maxInventory)
        {
            Debug.Log("Inventory Full! Cannot add " + prefab.name);// Logs a message when the inventory is already at maximum capacity
            return false;// Stops the pickup because there is no space for another weapon
        }

        // INSTANTIATION: Creates the new weapon as a child of the player
        GameObject newWep = Instantiate(prefab, attackPoint.position, attackPoint.rotation, transform);// Creates the picked-up weapon as a child object of the player

        // Hides the physical pickup sprite/collider because the player's main sprite handles weapon visuals
        SetInventoryWeaponState(newWep, true);// Converts the new weapon into an inventory item instead of a visible floor pickup

        // Hides it immediately so we don't hold multiple weapons at once
        newWep.SetActive(false);// Keeps the weapon object inactive until it is selected with Q

        // Adds to our 'Q' list of weapons
        unlockedWeapons.Add(newWep);// Adds the new weapon object to the player's inventory list
        audioSource.PlayOneShot(pickupSound);// Plays the pickup sound effect when a new weapon is added to the inventory
        Debug.Log($"Added {newWep.name} to inventory. Current count: {unlockedWeapons.Count}/{maxInventory}");// Logs the updated inventory count after the pickup

        // Picking up stores the weapon, but the player keeps holding whatever is currently equipped.
        UpdatePlayerAppearance();// Refreshes visuals so the current equipped weapon still appears correctly after pickup
        return true;// Tells the pickup script that the weapon was successfully added to the inventory
    }

    public bool PickUpOrSwapWeapon(GameObject prefab, Vector3 pickupPosition)
    {
        // Safety check to make sure the pickup actually has a weapon prefab assigned in the Inspector
        if (prefab == null)
        {
            Debug.LogWarning("Cannot pick up weapon because the pickup has no weapon prefab assigned.");// Warns if the pickup object was not set up correctly in the Inspector
            return false;// Stops the pickup because there is no weapon prefab to add
        }

        // Clears out any broken or missing weapons before checking the inventory size
        unlockedWeapons.RemoveAll(item => item == null);// Removes broken or missing weapons before deciding whether the inventory has space

        // If the player still has space, the weapon is added normally
        if (unlockedWeapons.Count < maxInventory)
        {
            return AddNewWeapon(prefab);// Adds the weapon normally when there is still inventory space
        }

        // If inventory is full and the player is using fists, there is no weapon in hand to swap out
        if (currentWeapon == null)
        {
            Debug.Log("Inventory Full! Equip a weapon with Q before swapping with " + prefab.name + ".");// Tells the player why the full-inventory pickup failed
            return false;// Stops the swap because fists are not an inventory weapon that can be replaced
        }

        // If inventory is full and a weapon is equipped, swap the equipped weapon with the floor weapon
        return SwapCurrentWeapon(prefab, pickupPosition);// Swaps the equipped weapon with the weapon on the floor
    }

    bool SwapCurrentWeapon(GameObject newWeaponPrefab, Vector3 pickupPosition)
    {
        // Finds the equipped weapon inside the inventory list so it can be replaced
        int equippedIndex = unlockedWeapons.IndexOf(currentWeapon.gameObject);// Finds the inventory slot that contains the currently equipped weapon
        if (equippedIndex < 0)
        {
            Debug.Log("Inventory Full! Could not find the equipped weapon to swap.");// Logs a safety message if the current weapon was not found in the inventory list
            return false;// Stops the swap because there is no valid inventory slot to replace
        }

        // Drops a copy of the current weapon onto the floor where the pickup was
        GameObject droppedWeapon = Instantiate(currentWeapon.gameObject, pickupPosition, Quaternion.identity);// Creates a floor copy of the weapon being swapped out
        droppedWeapon.transform.SetParent(null);// Removes the dropped weapon from the player so it becomes a world object
        droppedWeapon.SetActive(true);// Makes sure the dropped weapon is active in the scene
        SetInventoryWeaponState(droppedWeapon, false);// Restores the dropped weapon so it can be seen and picked up again

        // Stores the old weapon name before destroying it so it can be used in Debug.Log
        string oldWeaponName = currentWeapon.weaponName;// Stores the old weapon name before destroying the equipped weapon object

        // Removes the equipped weapon from the player before replacing it with the new weapon
        Destroy(currentWeapon.gameObject);// Removes the old equipped weapon object from the player

        // Creates the new weapon as a child of the player and equips it immediately
        GameObject newWeapon = Instantiate(newWeaponPrefab, attackPoint.position, attackPoint.rotation, transform);// Creates the new swapped weapon as a child object of the player
        newWeapon.SetActive(true);// Activates the new weapon object so its weapon script can be used by the player
        SetInventoryWeaponState(newWeapon, true);// Hides the tiny weapon sprite because the player sprite already shows the equipped weapon

        // Replaces the old inventory slot with the new weapon so inventory size stays the same
        unlockedWeapons[equippedIndex] = newWeapon;// Replaces the old inventory slot with the new weapon object
        currentWeaponIndex = equippedIndex;// Keeps the current inventory slot selected after the swap
        currentWeapon = newWeapon.GetComponent<Weapon>();// Stores the Weapon component from the new equipped weapon

        UpdatePlayerAppearance();// Refreshes the player's idle sprite, animation weapon type, and attackPoint offset after the swap
        Debug.Log($"Swapped {oldWeaponName} for {currentWeapon.weaponName}.");// Logs which weapon was replaced
        return true;// Tells the pickup object that the swap succeeded
    }

    void SetInventoryWeaponState(GameObject weaponObject, bool isInInventory)
    {
        // Inventory weapons are used for their script data, not for their small pickup sprite
        SpriteRenderer[] renderers = weaponObject.GetComponentsInChildren<SpriteRenderer>(true);// Gets every SpriteRenderer on the weapon and its children
        foreach (SpriteRenderer weaponRenderer in renderers)
        {
            weaponRenderer.enabled = !isInInventory;// Hides held inventory weapon renderers and shows dropped floor weapon renderers
        }

        // Disables pickup colliders while the weapon is held so it does not act like a floor item
        Collider2D[] colliders = weaponObject.GetComponentsInChildren<Collider2D>(true);// Gets every Collider2D on the weapon and its children
        foreach (Collider2D weaponCollider in colliders)
        {
            weaponCollider.enabled = !isInInventory;// Disables held weapon colliders and enables dropped floor weapon colliders
        }

        // Disables the pickup script while held, but turns it back on when the weapon is dropped
        WeaponPickup pickup = weaponObject.GetComponent<WeaponPickup>();// Gets the pickup script so it can be disabled while the weapon is held
        if (pickup != null)
        {
            pickup.enabled = !isInInventory;// Prevents held weapons from behaving like pickup objects while they are in the inventory
        }
    }

    void CycleWeapon()
    {
        // Checks if the current weapon object still exists before deactivating
        if (currentWeapon != null && currentWeapon.gameObject != null)
        {
            currentWeapon.gameObject.SetActive(false);// Deactivates the previously equipped weapon before switching to the next inventory option
        }

        unlockedWeapons.RemoveAll(item => item == null);// Removes destroyed or missing weapons before cycling

        // If there are no weapons in the inventory, Q should still put the player on fists
        if (unlockedWeapons.Count == 0)
        {
            EquipHands();// Switches to fists when there are no weapons in the inventory
            return;// Stops cycling because there is no weapon to equip
        }

        // Cycles through all weapons, then one extra hand-to-hand slot
        currentWeaponIndex++;// Moves the selection to the next weapon slot

        // If the index passes the last weapon, switch to fists instead of counting fists as inventory
        if (currentWeaponIndex >= unlockedWeapons.Count)
        {
            EquipHands();// Switches to fists after cycling past the last weapon
            return;// Stops cycling because fists have now been selected
        }

        EquipWeaponFromIndex(currentWeaponIndex);// Equips the weapon at the newly selected inventory index
    }

    void EquipWeaponFromIndex(int index)
    {
        if (unlockedWeapons.Count == 0)
        {
            EquipHands();// Switches to fists if there are no weapons to equip
            return;// Stops the equip logic because the inventory is empty
        }

        // If the index is outside the list, the player is safely switched to fists
        if (index < 0 || index >= unlockedWeapons.Count)
        {
            EquipHands();// Switches to fists when the requested inventory index is not valid
            return;// Stops the equip logic because the index cannot be used
        }

        GameObject selection = unlockedWeapons[index];// Gets the weapon object stored at the selected inventory index

        // Checks if the object actually exists in the game world
        if (selection != null)
        {
            selection.SetActive(true);// Activates the selected weapon object so its Weapon component can be used
            SetInventoryWeaponState(selection, true);// Keeps the equipped inventory weapon invisible while its stats still work
            currentWeaponIndex = index;// Stores the selected inventory index as the current weapon slot
            currentWeapon = selection.GetComponent<Weapon>();// Gets the Weapon component from the selected inventory object
            CancelIdleVisualRestore();// Cancels any delayed idle visual refresh from a previous attack or weapon switch
            RefreshMovementState();// Checks whether the player is moving before deciding between idle sprite and walk animation
            UpdatePlayerAppearance();// Updates the main player sprite after switching to this weapon
            Debug.Log($"Switched to: {currentWeapon.weaponName}");// Logs the weapon that is now equipped
        }
        else
        {
            // If the weapon was destroyed (durability hit 0), remove it from the list
            unlockedWeapons.RemoveAt(index);// Removes the missing weapon slot from the inventory list

            // If the players still has weapons, try to equip the next one
            if (unlockedWeapons.Count > 0)
            {
                currentWeaponIndex = 0; // Reset to start
                EquipWeaponFromIndex(currentWeaponIndex);// Equips the first available weapon after removing the missing one
            }
            else
            {
                currentWeapon = null;// Clears the current weapon when no inventory weapons remain
                UpdatePlayerAppearance();// Refreshes visuals back to the unarmed state
            }
        }
    }

    void EquipHands()
    {
        // Sets the index to -1 so fists are treated as a toggle option, not an inventory item
        currentWeaponIndex = -1;// Sets the inventory index to -1 to represent fists
        currentWeapon = null;// Clears the equipped weapon so attacks use fists
        CancelIdleVisualRestore();// Cancels any delayed visual refresh before switching to fists
        RefreshMovementState();// Checks whether the player is currently moving before updating visuals
        UpdatePlayerAppearance();// Refreshes the player sprite and attackPoint to the unarmed setup
        Debug.Log("Switched to: Fists");// Logs that the player is now unarmed
    }

    void Start()
    {
        unlockedWeapons.Clear();// Clears any Inspector-filled inventory list so the runtime inventory starts clean
        // If the list is empty but the player has a weapon equipped, add it to the count
        if (unlockedWeapons.Count == 0 && currentWeapon != null)
        {
            unlockedWeapons.Add(currentWeapon.gameObject);// Adds the starting weapon to the inventory list if the player begins with one equipped
            SetInventoryWeaponState(currentWeapon.gameObject, true);// Hides any starting weapon object so only the player sprite is visible
            currentWeaponIndex = 0;// Sets the starting weapon as the selected inventory slot
            Debug.Log("Starting weapon registered. Inventory: 1/" + maxInventory);// Logs that the starting weapon was added to the inventory count
        }

        // Visuals: Makes sure we don't start with a white square
        if (playerRenderer == null) playerRenderer = FindPlayerRenderer();// Finds the player body sprite if it was not assigned in the Inspector
        InitializeWalkOverrideController();// Sets up the runtime walk animation override controller before visuals are updated
        UpdatePlayerAppearance();// Applies the correct starting sprite, weapon animation, and attackPoint offset
    }

    void Update()
    {
        // Handle Weapon Switching with Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleWeapon();// Switches to the next inventory weapon or fists when Q is pressed
            audioSource.PlayOneShot(pickupSound);// Plays the weapon switch sound effect when cycling weapons
        }


    }

    public void SetPlayerMoving(bool moving)
    {
        if (isPlayerMoving == moving) return;// Avoids refreshing visuals when the movement state has not changed

        isPlayerMoving = moving;// Stores the current movement state reported by PlayerMovement
        UpdatePlayerAppearance();// Updates the player visuals when switching between idle and moving
    }

    void RefreshMovementState()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();// Gets the PlayerMovement script so this class can read its current movement state
        if (movement != null)
        {
            isPlayerMoving = movement.isMoving;// Uses the movement script's isMoving value as the most reliable movement check
            return;// Stops here because the movement state was found from PlayerMovement
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();// Falls back to checking the Rigidbody2D velocity if PlayerMovement is missing
        isPlayerMoving = rb != null && rb.linearVelocity.sqrMagnitude > 0.01f;// Treats the player as moving if the Rigidbody2D has noticeable velocity
    }

    // This handles the "Art" swap based on the current weapon's name
    public void UpdatePlayerAppearance()
    {
        // Safety cleanup for the list
        unlockedWeapons.RemoveAll(item => item == null);// Removes destroyed or missing weapons before deciding what visual state to show

        // If no weapon is equipped or it's depleted, show unarmed player
        if (currentWeapon == null || currentWeapon.isDepleted)
        {
            ApplyWeaponVisual(unarmedSprite, "Unarmed", unarmedOffset, Weapon.WeaponAnimationType.Unarmed);// Shows the player without a weapon when using fists
            attackPoint.localPosition = unarmedOffset;//Moves attackPoint to the unarmed attack position, which is the default attackPoint
            return;
        }

        // We check the weaponName string from the Weapon class to determine the sprite
        Weapon.WeaponAnimationType animationType = GetWeaponAnimationType(currentWeapon);// Converts the equipped weapon into the animation type used by sprites and Animator logic

        if (animationType == Weapon.WeaponAnimationType.Pistol)
        {
            ApplyWeaponVisual(pistolSprite, "Pistol", pistolOffset, animationType);// Shows the player sprite that has the pistol built into it
        }
        else if (animationType == Weapon.WeaponAnimationType.Rifle)
        {
            ApplyWeaponVisual(rifleSprite, "Rifle", rifleOffset, animationType);// Shows the player sprite that has the rifle built into it
        }
        else if (animationType == Weapon.WeaponAnimationType.Pipe)
        {
            ApplyWeaponVisual(pipeSprite, "Pipe", pipeOffset, animationType);// Shows the player sprite and attack offset for the pipe
        }
        else if (animationType == Weapon.WeaponAnimationType.Knife)
        {
            ApplyWeaponVisual(knifeSprite, "Knife", knifeOffset, animationType);// Shows the player sprite that has the knife built into it
        }
        else
        {
            ApplyWeaponVisual(unarmedSprite, "Unarmed", unarmedOffset, Weapon.WeaponAnimationType.Unarmed);// Fallback sprite if the weapon name does not match any known weapon type
        }
    }

    void ApplyWeaponVisual(Sprite fallbackSprite, string visualName, Vector2 attackOffset, Weapon.WeaponAnimationType animationType)
    {
        attackPoint.localPosition = attackOffset;// Moves the attackPoint to the correct place for the equipped weapon
        SetAnimatorWalkClip(animationType);// Swaps the walk animation clip so movement uses the equipped weapon's walking animation

        if (driveAnimatorWeaponStates)
        {
            if (playerAnimator == null) playerAnimator = GetComponent<Animator>();// Finds the Animator if it was not assigned in the Inspector

            if (playerAnimator != null)
            {
                if (isPlayerMoving)
                {
                    SetAnimatorWeaponType(animationType);// Enables the Animator and sets weapon movement parameters when the player is walking
                    return;// Stops here because moving visuals are handled by the Animator instead of the idle sprite
                }

                playerAnimator.SetInteger(weaponTypeParameter, (int)animationType);// Stores the equipped weapon type in the Animator even while idle
                playerAnimator.SetBool(movingParameter, false);// Tells the Animator the player is not moving before it is disabled for the idle sprite
                SetAnimatorSpriteOverride(true);// Disables the Animator so the manual idle weapon sprite can stay visible
            }
        }

        // Standing still uses the assigned held-weapon sprite as the idle pose.
        if (!isPlayerMoving || !driveAnimatorWeaponStates || playerAnimator == null)
        {
            SetPlayerSprite(fallbackSprite, visualName);// Shows the correct idle sprite when the player is standing still
        }
    }

    Weapon.WeaponAnimationType GetWeaponAnimationType(Weapon weapon)
    {
        if (weapon == null) return Weapon.WeaponAnimationType.Unarmed;// Uses the unarmed animation type when there is no equipped weapon
        if (weapon.animationType != Weapon.WeaponAnimationType.Auto) return weapon.animationType;// Uses the manually assigned weapon animation type if the weapon prefab has one

        string name = weapon.weaponName.ToLower();// Converts the weapon name to lowercase so the text checks are not case-sensitive
        if (name.Contains("pistol")) return Weapon.WeaponAnimationType.Pistol;// Uses the pistol animation type if the weapon name contains "pistol"
        if (name.Contains("rifle")) return Weapon.WeaponAnimationType.Rifle;// Uses the rifle animation type if the weapon name contains "rifle"
        if (name.Contains("pipe")) return Weapon.WeaponAnimationType.Pipe;// Uses the pipe animation type if the weapon name contains "pipe"
        if (name.Contains("knife")) return Weapon.WeaponAnimationType.Knife;// Uses the knife animation type if the weapon name contains "knife"

        return Weapon.WeaponAnimationType.Unarmed;// Falls back to unarmed animations if the weapon name does not match a known type
    }

    void SetAnimatorWeaponType(Weapon.WeaponAnimationType animationType)
    {
        if (!driveAnimatorWeaponStates) return;// Stops if this script is not meant to control Animator weapon states

        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();// Finds the Animator if it was not assigned in the Inspector
        if (playerAnimator == null) return;// Stops if there is no Animator on the player

        // Updates the Animator parameters first so any normal transitions still know which weapon is equipped
        isOverridingAnimatorSprite = false;// Records that the Animator is no longer being overridden by a static idle sprite
        playerAnimator.SetInteger(weaponTypeParameter, (int)animationType);// Sends the equipped weapon type to the Animator
        playerAnimator.SetBool(movingParameter, true);// Tells the Animator the player is moving so walk animations can play
        SetAnimatorWalkClip(animationType);// Replaces the base walk clip with the equipped weapon's walk clip
        playerAnimator.enabled = true;// Enables the Animator so movement animation can control the SpriteRenderer
    }

    void InitializeWalkOverrideController()
    {
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();// Finds the Animator if it was not assigned in the Inspector
        if (playerAnimator == null || playerAnimator.runtimeAnimatorController == null) return;// Stops if the player has no Animator or no Animator Controller

        // Creates a runtime override controller so the original Animator Controller asset is not permanently changed
        if (walkOverrideController == null)
        {
            walkOverrideController = new AnimatorOverrideController(playerAnimator.runtimeAnimatorController);// Creates a copy of the current Animator Controller that can override clips at runtime
            playerAnimator.runtimeAnimatorController = walkOverrideController;// Assigns the override controller back to the player Animator
        }

        // Uses the assigned unarmed walk as the base clip if it was set in the Inspector
        if (baseWalkClip == null)
        {
            baseWalkClip = unarmedWalkClip;// Stores the assigned unarmed walk clip as the animation clip that will be replaced
        }

        // Fallback: finds the first animation clip with "walk" in its name if the base walk clip was not assigned
        if (baseWalkClip == null)
        {
            foreach (AnimationClip clip in walkOverrideController.animationClips)
            {
                string clipName = clip.name.ToLower();// Converts the clip name to lowercase so the walk-name check is not case-sensitive
                if (clipName.Contains("walk"))
                {
                    baseWalkClip = clip;// Stores the first walk clip found as the clip that weapon walks will replace
                    break;// Stops searching once a suitable walk clip has been found
                }
            }
        }
    }

    void SetAnimatorWalkClip(Weapon.WeaponAnimationType animationType)
    {
        if (!driveAnimatorWeaponStates) return;// Stops if Animator weapon-state control is turned off
        if (appliedWalkAnimationType == animationType) return;// Stops if the correct weapon walk clip is already applied

        InitializeWalkOverrideController();// Makes sure the runtime override controller and base walk clip are ready
        if (walkOverrideController == null || baseWalkClip == null) return;// Stops if there is no controller or base walk clip to override

        // Chooses the matching walking clip for the equipped weapon, falling back to normal walking if one is missing
        AnimationClip replacementClip = GetWalkClip(animationType);// Gets the weapon-specific walk clip that should replace the base walk clip
        if (replacementClip == null)
        {
            replacementClip = baseWalkClip;// Uses the normal walking clip if the weapon-specific walk clip was not assigned
        }

        // Replaces the normal walk clip with the equipped weapon's walk clip at runtime
        walkOverrideController[baseWalkClip] = replacementClip;// Applies the chosen replacement clip to the runtime override controller
        appliedWalkAnimationType = animationType;// Records which weapon walk clip is now active
    }

    AnimationClip GetWalkClip(Weapon.WeaponAnimationType animationType)
    {
        if (animationType == Weapon.WeaponAnimationType.Pistol) return pistolWalkClip;// Returns the pistol walk clip when the pistol is equipped
        if (animationType == Weapon.WeaponAnimationType.Rifle) return rifleWalkClip;// Returns the rifle walk clip when the rifle is equipped
        if (animationType == Weapon.WeaponAnimationType.Pipe) return pipeWalkClip;// Returns the pipe walk clip when the pipe is equipped
        if (animationType == Weapon.WeaponAnimationType.Knife) return knifeWalkClip;// Returns the knife walk clip when the knife is equipped
        return unarmedWalkClip != null ? unarmedWalkClip : baseWalkClip;// Returns the unarmed walk clip, or the detected base walk clip if no unarmed clip was assigned
    }

    void TriggerAttackAnimation()
    {
        if (!driveAnimatorWeaponStates) return;// Stops if this script is not controlling Animator weapon states

        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();// Finds the Animator if it was not assigned in the Inspector
        if (playerAnimator == null) return;// Stops if the player has no Animator

        playerAnimator.enabled = true;// Enables the Animator so the attack trigger can be played
        isOverridingAnimatorSprite = false;// Records that the Animator is controlling sprites during the attack animation
        playerAnimator.SetTrigger(attackTriggerParameter);// Triggers the attack animation parameter on the Animator
    }

    void RestoreIdleVisualAfterAttack()
    {
        if (isPlayerMoving) return;// Does not restore a static idle sprite if the player is currently moving

        CancelIdleVisualRestore();// Cancels any previous delayed idle restore so only the newest attack controls it
        idleVisualRestoreRoutine = StartCoroutine(RestoreIdleVisualAfterAttackRoutine());// Starts the delayed restore back to the held-weapon idle sprite
    }

    void CancelIdleVisualRestore()
    {
        if (idleVisualRestoreRoutine == null) return;// Stops if there is no delayed idle restore currently running

        StopCoroutine(idleVisualRestoreRoutine);// Cancels the delayed idle restore coroutine
        idleVisualRestoreRoutine = null;// Clears the coroutine reference after cancelling it
    }

    IEnumerator RestoreIdleVisualAfterAttackRoutine()
    {
        yield return new WaitForSeconds(idleVisualRestoreDelay);// Waits briefly so the attack animation trigger has time to run before restoring the idle sprite
        RefreshMovementState();// Checks whether the player started moving during the delay

        if (!isPlayerMoving)
        {
            UpdatePlayerAppearance();// Restores the correct held-weapon idle sprite if the player is still standing still
        }

        idleVisualRestoreRoutine = null;// Clears the coroutine reference after the restore finishes
    }

    void SetPlayerSprite(Sprite newSprite, string spriteName)
    {
        // Safety check in case the playerRenderer was not assigned in the Inspector
        if (playerRenderer == null)
        {
            Debug.LogWarning("Player Renderer is missing, so the player sprite cannot be changed.");// Warns that the player SpriteRenderer reference is missing
            return;
        }

        // Safety check so missing weapon sprites do not make the player turn invisible
        if (newSprite == null)
        {
            Debug.LogWarning(spriteName + " sprite is not assigned in PlayerCombat.");// Warns that the requested idle weapon sprite was not assigned in the Inspector
            return;
        }

        playerRenderer.enabled = true;// Makes sure the player SpriteRenderer is visible before changing the sprite
        playerRenderer.color = Color.white;// Resets the player color so animation clips cannot leave the sprite transparent or tinted
        playerRenderer.sprite = newSprite;// Changes the main player sprite to match the equipped weapon
    }

    void SetAnimatorSpriteOverride(bool shouldOverride)
    {
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();// Finds the Animator if it was not assigned in the Inspector
        if (playerAnimator == null) return;// Stops if the player has no Animator

        isOverridingAnimatorSprite = shouldOverride;// Records whether manual sprite swapping is currently overriding the Animator
        playerAnimator.enabled = !shouldOverride;// Disables the Animator for idle sprites and enables it for animated movement/attacks
    }

    SpriteRenderer FindPlayerRenderer()
    {
        // First checks for a SpriteRenderer on the player object itself
        SpriteRenderer directRenderer = GetComponent<SpriteRenderer>();// Looks for a SpriteRenderer directly on the player object
        if (directRenderer != null) return directRenderer;

        // Then checks child SpriteRenderers, but skips weapon objects so we don't swap the tiny weapon sprite
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>(true);// Gets all child SpriteRenderers so the player body renderer can be found
        foreach (SpriteRenderer childRenderer in childRenderers)
        {
            if (childRenderer.GetComponentInParent<Weapon>() == null)
            {
                return childRenderer;// Returns the first child SpriteRenderer that does not belong to a weapon object
            }
        }

        return null;// Returns null if no valid player SpriteRenderer could be found
    }

}
