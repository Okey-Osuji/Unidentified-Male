using System;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public enum EnemyType { Brawler, Guard, Soldier }// Enumeration to define different types of enemies in the game, which can be used to categorize enemy behavior and attributes

    [Header("Enemy Info")]
    public EnemyType enemyType;// Variable to store the type of enemy to differentiate between different enemy behaviors and attributes
    public string enemyName;// Variable to store the name of the enemy

    [Header("Stats")]
    public float maxHealth = 100f;// The maximum health of the enemy, which determines how much damage it can take before being defeated
    public float currentHealth;// The current health of the enemy, which is updated when the enemy takes damage and is used to determine if the enemy is still alive
    public float armorRating = 0f;// The armor rating of the enemy, which can reduce incoming damage based on its value
    public bool isDead = false;// Flag to indicate whether the enemy is dead or alive
    public float moveSpeed = 2f;//Enemy speed when moving around map/towards player
    public float detectionRange = 7f;//The radius in which enemies detect the player GameObject
    private Transform player;//Tracks and performs movement and scale of player

    [Header("Loot Drop Settings")]
    public GameObject weaponPickUpPrefab;
    public GameObject weaponToDrop;
    public float dropChance = 1.0f; // 1.0 = 100% chance for an enemy to drop an item

    [Header("Combat Settings")]
    public bool hasFirearm = false;// Flag to indicate whether the enemy is equipped with a firearm, which can affect its attack behavior and range
    public float attackDamage = 10f;// The base attack damage of the enemy, which can be modified by the enemy's type, equipment, and other factors to determine how much damage it deals to the player
    public float attackRange = 5.0f;// The range of the enemy attack in reference to player position
    public float attackRate = 1f;// The rate in seconds in which the enemy attacks
    private float nextAttackTime = 0f;// The time it takes to initiate the next attack
    public GameObject bulletPrefab;// Assign the new bullet here
    public Transform firePoint;// A child object at the tip of the gun
    public float bulletSpawnOffset = 1f;// Distance in front of the enemy where bullets spawn so each enemy does not need a perfect firePoint
    public bool isPlayerInRange = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;// Initializes the current health of the enemy to its maximum health at the start of the game
        // Automatically finds the player GameObject
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Enemy cannot find Player! Is the Triangle tagges 'Player'?");
        }


    }

    // Method to handle the enemy's death logic, which can include playing a death animation, dropping loot, and removing the enemy from the game world
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float finalDamage = damage * (1f - armorRating);// Calculate the final damage after applying armor reduction, which reduces the incoming damage based on the enemy's armor rating to provide a more realistic damage system
        currentHealth -= finalDamage;// Reduces the current health of the enemy by the calculated final damage after applying armor reduction

        Debug.Log($"{enemyName} (Tier {enemyType}) took {finalDamage} damage! Remaining health: {currentHealth}");// Log a message to indicate that the enemy has taken damage, including the enemy's name, type, the amount of damage taken, and the remaining health
        if (currentHealth <= 0) Die();// Checks if the enemy's health has dropped to zero or below, and if so, call the Die method to handle the enemy's death
    }

    void Update()
    {
        // If the enemy is dead, only listen for Consumption (E)
        if (isDead)
        {
            // Stop all movement completely
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            // Checks for consumption
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                ConsumeCorpse();
            }

            return; // Exits here so we don't run combat logic for a corpse
        }

        // If the enemy is alive, run Combat/Chase Logic
        if (player == null) return;

        FacePlayer();// Makes the enemy sprite face the player before chasing or attacking

        float currentEffectiveRange = attackRange;

        // Detection of weapon range
        if (weaponToDrop != null)
        {
            WeaponPickup pickup = weaponToDrop.GetComponent<WeaponPickup>();
            if (pickup != null && pickup.weaponPrefab != null)
            {
                Weapon weaponData = pickup.weaponPrefab.GetComponent<Weapon>();
                if (weaponData != null) currentEffectiveRange = weaponData.range;
            }
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= currentEffectiveRange)
        {
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackRate;
            }
        }
        else if (distance < detectionRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            GetComponent<Rigidbody2D>().linearVelocity = direction * moveSpeed;
        }
    }

    // Method to make sure soldier enemy always faces player when firing weapon
    void FacePlayer()
    {
        // Finds the direction from the enemy to the player
        Vector2 direction = player.position - transform.position;
        if (direction == Vector2.zero) return;

        // Rotates the enemy so the top of the sprite points towards the player
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, targetAngle - 90f);
    }

    
    private void ConsumeCorpse()
    {
        Debug.Log("Corpse consumed! Health restored, morale chipping away...");
        Destroy(gameObject);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        gameObject.tag = "Corpse";

        // Logic to spawn defeated enemy's weapon
        if (weaponToDrop != null)
        {
            Debug.Log("SUCCESS: Weapon variable found. Attempting to spawn: " + weaponToDrop.name);
            // Code to determine the distance from dropped weapons to corpse
            float offsetX = UnityEngine.Random.Range(-2.0f, 2.0f);
            float offsetY = UnityEngine.Random.Range(-2.0f, 2.0f);
            Vector3 dropPosition = new Vector3(transform.position.x + offsetX, transform.position.y + offsetY, transform.position.z);

            GameObject droppedItem = Instantiate(weaponToDrop, dropPosition, Quaternion.identity);

            // Final sanity check: make sure the dropped item is actually active
            if (droppedItem != null)
            {
                droppedItem.SetActive(true);
                Debug.Log("SUCCESS: Item instantiated at " + dropPosition);
            }
        }
        


        GetComponent<SpriteRenderer>().color = Color.gray;// Changes the color of the enemy's sprite to gray to visually indicate that the enemy is dead, which can help players easily identify defeated enemies in the game world
        transform.rotation = Quaternion.Euler(0, 0, 90);// Rotates the object to a fixed orientation of 90 degrees around the Z-axis(Tells player the state of the enemy, since it's 90 degrees, it lies down on the floor axis and looks like a corpse)



        // Turns over the solid collision so the player can walk over the corpse
        GetComponent<Collider2D>().isTrigger = true;


        Debug.Log(enemyName + " Is ready to be Consumed (E) !");// Logs a message to indicate that the enemy is ready to be consumed by the player, which can be used to prompt the player to interact with the defeated enemy for benefits such as health restoration or weapons

    }


    void AttackPlayer()
    {
        float damageToDeal = attackDamage;

        // Gets weapon damage if a weapon is assigned
        if (weaponToDrop != null)
        {
            WeaponPickup pickup = weaponToDrop.GetComponent<WeaponPickup>();
            if (pickup != null && pickup.weaponPrefab != null)
            {
                Weapon weaponData = pickup.weaponPrefab.GetComponent<Weapon>();
                if (weaponData != null) damageToDeal = weaponData.damageValue;
            }
        }

        // Checks if enemy uses ranged firearms or melee
        if (bulletPrefab != null)
        {
            // RANGED ATTACK (Soldier)
            // Spawns the bullet in front of the enemy using transform.up because the enemy now faces the player
            Vector3 spawnPosition = transform.position + transform.up * bulletSpawnOffset;

            // Calculates the direction from the spawn point to the player so the bullet does not shoot from the wrong angle
            Vector2 direction = (player.position - spawnPosition).normalized;

            // Converts direction into a rotation, with -90f because the bullet moves using transform.up
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.Euler(0, 0, angle - 90f));

            // Gives the bullet the enemy's damage value
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null) bulletScript.damage = damageToDeal;
        }
        else
        {
            // MELEE ATTACK (Brawler and Guard)
            float dist = Vector2.Distance(transform.position, player.position);


            if (dist <= attackRange + 0.5f)
            {
                StatusManager playerStatus = player.GetComponent<StatusManager>();
                if (playerStatus != null)
                {
                    playerStatus.TakeDamage(damageToDeal);
                    Debug.Log(enemyName + " dealt " + damageToDeal + " damage.");
                }
            }
        }
    }
}
