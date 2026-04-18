using UnityEngine;

public class WeaponPickUp : MonoBehaviour
{
    [Header("Weapon Pick Up Settings")]
    public GameObject weaponPrefab; // The prefab of the weapon to be picked up
    public string pickUpMessage = "Press E to pick up"; // Message to display when player is near

    private bool isPlayerNearby = false; // Flag to check if player is nearby
    private PlayerCombat playerCombat; // Reference to the player's combat script

    // Method to etect when the player enters the trigger area
    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))// Checks if the player has entered the trigger area
        {
            isPlayerNearby = true;// Set the flag to true
            playerCombat = other.GetComponent<PlayerCombat>(); // Gets the player's combat script
            Debug.Log(pickUpMessage + weaponPrefab.name); // Displays the pick up message
        }
    }
 
    // Method to detect when the player exits the trigger area
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))// Checks if the player has exited the trigger area
        {
            isPlayerNearby = false;// Set the flag to false
            
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E)) // Checks if the player is nearby and presses the E key
        {
            EquipWeapon(); // Calls the method to equip the weapon
        }
    }

    void EquipWeapon()
    {
        playerCombat.SwapWeapon(weaponPrefab); // Calls the method to swap the player's weapon with the new one
        Destroy(gameObject);// Destroys the weapon pick up object after equipping the weapon
    }
}
