using System;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public enum EnemyType {Brawler, Guard, Soldier}// Enumeration to define different types of enemies in the game, which can be used to categorize enemy behavior and attributes
    
    [Header("Enemy Info")]
    public EnemyType enemyType;// Variable to store the type of enemy to differentiate between different enemy behaviors and attributes
    public string enemyName;// Variable to store the name of the enemy

    [Header("Stats")]
    public float maxHealth = 100f;// The maximum health of the enemy, which determines how much damage it can take before being defeated
    public float currentHealth;// The current health of the enemy, which is updated when the enemy takes damage and is used to determine if the enemy is still alive
    public float armorRating = 0f;// The armor rating of the enemy, which can reduce incoming damage based on its value
    public bool isDead = false;// Flag to indicate whether the enemy is dead or alive
    
    [Header("Equipment")]
    public bool hasFirearm = false;// Flag to indicate whether the enemy is equipped with a firearm, which can affect its attack behavior and range
    public float attackDamage = 10f;// The base attack damage of the enemy, which can be modified by the enemy's type, equipment, and other factors to determine how much damage it deals to the player
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;// Initializes the current health of the enemy to its maximum health at the start of the game
        if(enemyType == EnemyType.Soldier) GetComponent<SpriteRenderer>().color = Color.blue;// Set the color of the enemy's sprite to blue if it is of the Soldier type, which can be used to visually differentiate between different enemy types in the game
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

    void Die()
    {
        isDead = true;
        GetComponent<SpriteRenderer>().color = Color.gray;// Changes the color of the enemy's sprite to gray to visually indicate that the enemy is dead, which can help players easily identify defeated enemies in the game world
        GetComponent<Collider2D>().enabled = false;// Disables the enemy's collider to prevent further interactions with the player and other game objects after the enemy has died, which can help improve performance and prevent unintended behavior in the game world
        Debug.Log(enemyName + "Is ready to be Consumed (E) !");// Log a message to indicate that the enemy is ready to be consumed by the player, which can be used to prompt the player to interact with the defeated enemy for benefits such as health restoration or weapons
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
