using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Survival Stats")]
    public float maxEnergy = 100f;// Maximum energy the player can have
    public float currentEnergy;// Current energy level of the player
    public float energyDrainRate = 1.5f;// Rate at which energy drains over time

    [Header("Health and Lives")]
    public float maxHealth = 100f;// Maximum health the player can have
    public float currentHealth;// Current health level of the player
    public int lives = 3;// Number of lives the player has
    private Vector3 lastCheckpoint; // Position of the last checkpoint the player reached       

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentEnergy = maxEnergy;// Initializes current energy to maximum at the start
        currentHealth = maxHealth;// Initializes current health to maximum at the start
        lastCheckpoint = transform.position;// Sets the initial checkpoint to the player's starting position
    }

    // Update is called once per frame
    void Update()
    {
        if (currentEnergy > 0)
        {
            currentEnergy -= energyDrainRate * Time.deltaTime;// Drains energy over time
        }
        else
        {
            currentHealth -= 4f * Time.deltaTime;// If energy is depleted, start draining health
        }
        if (currentHealth <= 0)
        {
            HandleDeath();// Handles player death when health reaches zero
        }
    }
    
    // Method to handle player interaction input
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)// Checks if the interact action was performed
        {
            Consume();// Calls the Consume method when the interact action is performed
        }
    }

    // Method to consume an item and restore energy
    void Consume()
    {
       currentEnergy = Mathf.Min(currentEnergy + 25f, maxEnergy);// Restores energy by 25, but do not exceed max energy
       Debug.Log("Consumed, Energy Restored!");// Logs a message to the console when an item is consumed
    }

    // Method to handle player death and respawn logic
    void HandleDeath()
    {
        lives--; 
        if (lives > 0)
        { 
            currentHealth = maxHealth;// Resets health to maximum
            currentEnergy = maxEnergy;// Resets energy to maximum
            transform.position = lastCheckpoint;// Respawns the player at the last checkpoint
            Debug.Log("You Died! Lives Remaining: " + lives);// Logs a message to the console indicating the player has died and how many lives are remaining   
        }
        else
        {
            Time.timeScale = 0f;// Pauses the game when the player has no lives left
            Debug.Log("GAME OVER!");// Logs a game over message to the console when the player has no lives left
        }
    }
    
    // Method to update the last checkpoint position when the player reaches a new checkpoint
    public void SetCheckpoint(Vector3 newPos)
    {
        lastCheckpoint = newPos;// Updates the last checkpoint position when the player reaches a new checkpoint
        Debug.Log("Checkpoint Updated!");// Logs a message to the console when the checkpoint is updated
    }

}
