using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StatusManager : MonoBehaviour
{

    [Header("Survival Stats")]
    public float health = 100f;// Starting health value
    public float maxHealth = 100f;// Maximum health value
    public float morale = 100f;// Starting morale value
    public float hunger = 100f;// Starting hunger value

    [Header("UI Sliders (Placeholders)")]
    public Slider healthSlider;// Slider for health value
    public Slider hungerSlider;// Slider for hunger value
    public Slider moraleSlider;// Slider for moraleSlider

    [Header("Lives System")]
    public int lives = 3;// Starting number of lives
    public GameObject[] lifeIcons;// UI icons representing lives (assigned in inspector)

    

    void Start()
    {
        UpdateUI();
    }
    // Update is called once per frame
    void Update()
    {
        if(hunger > 0)// If the player is not starving, decrease hunger over time
        {
            hunger -= Time.deltaTime * 0.8f;// Hunger decreases slowly over time
        }
        else
        {
            // Starvation : Health and Morale(Sanity) begin to drop
            TakeDamage(Time.deltaTime * 1.5f);// If hunger reaches zero, start taking damage from starvation
            morale = Mathf.Max(morale-(Time.deltaTime * 2f), 0);
        }

        
        UpdateUI();// Keeps the UI updated every frame
    }

    // Method to handle changes in UI Sliders
    void UpdateUI()
    {
        if(healthSlider != null) healthSlider.value = health / maxHealth;
        if(hungerSlider != null) hungerSlider.value = hunger / 100f;
        if(moraleSlider != null) moraleSlider.value = morale / 100f;
    }

    // Method to apply damage to the player, reducing health and checking for death
    public void TakeDamage(float amount)
    {
        health -= amount;// Reduce health by the specified amount
        if (health <= 0)// If health drops to zero or below, handle player death
         {
            HandleDeath();// Calls the method to handle player death and life loss
        }

    }

    // Method to consume a fallen enemy, restoring health and hunger but reducing morale
    public void Consume()
    {
        health = Mathf.Clamp(health + 30f, 0, maxHealth);// Restores health by 30, but does not exceed maxHealth
        hunger = Mathf.Clamp(hunger + 40f, 0, 100f);// Restores hunger by 40, but does not exceed 100
        morale = Mathf.Clamp(morale - 10f, 0, 100f);// Reduces morale by 10, but does not go below 0
        Debug.Log("Consumed fallen enemy. Morale is chipping away...");// Logs a message to the console indicating that the player has consumed an enemy and morale is decreasing
    }


    // Method to handle player death, reducing lives and resetting stats or restarting the game if no lives remain
    void HandleDeath()
{
    lives--; // Reduces the number of lives by one

    // Update the UI Icons
    // If the player just died and has 2 lives left, index 2 (the 3rd heart) turns off.
    if (lives >= 0 && lives < lifeIcons.Length)
    {
        lifeIcons[lives].SetActive(false);
        Debug.Log("Life Lost! Remaining: " + lives);
    }

    // Checks if the Game is over
    if (lives > 0)
    {
        // RESET STATS (The "Respawn")
        health = maxHealth;
        hunger = 100f;
        
        
        
    }
    else
    {
        // GAME OVER
        Debug.Log("Triggering Game Over Screen...");
        
        // Use the Manager instead of just restarting the scene
        // This makes your "GAME OVER!" text panel pop up.
        GameManager manager = FindFirstObjectByType<GameManager>();
        if (manager != null)
        {
            manager.TriggerGameOver();
        }
        
    }
}
}
