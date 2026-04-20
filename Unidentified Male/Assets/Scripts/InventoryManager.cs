using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{

    [Header("Inventory Settings")]
    public int maxSlots = 3;// Maximum number of weapon slots available in the inventory
    public List<Weapon> weaponSlots = new List<Weapon>();// List to hold the weapons currently in the inventory, initialized as an empty list
    public int currentWeaponIndex = 0;// Index to track the currently equipped weapon in the inventory, starting at 0 (the first slot)

    private PlayerCombat playerCombat;// Reference to the PlayerCombat script to allow the InventoryManager to interact with the player's combat system


    // Start is called before the first frame update, which initializes the inventory by clearing any existing weapons and setting the current weapon index to 0 to ensure a clean starting state for the player's inventory when the game begins
    void Start()
    {
        weaponSlots.Clear();// Clears the weaponSlots list at the start of the game to ensure it starts empty, allowing the player to pick up weapons and add them to the inventory as they progress through the game
        currentWeaponIndex = 0;// Resets the currentWeaponIndex to 0 at the start of the game to ensure that the first weapon slot is selected by default when the player begins, allowing for a consistent starting point for equipping weapons from the inventory
    }

    // Method to call Awake when the script instance is being loaded, which initializes the reference to the PlayerCombat component attached to the same GameObject
    void Awake()
    {
        playerCombat = GetComponent<PlayerCombat>();// Gets the PlayerCombat component attached to the same GameObject to allow the InventoryManager to access and modify the player's combat behavior based on the equipped weapon
    }

    // Method to add a new weapon to the inventory, which takes a weapon prefab as an argument and instantiates it in the game world, then adds it to the weaponSlots list if there is available space
    public bool AddWeapon(GameObject weaponPrefab)
    {

        Debug.Log($"Attemptimg pickup. Inventory: {weaponSlots.Count}/Max: {maxSlots}");

        if (weaponSlots.Count < maxSlots)
        {
            GameObject newWeaponObj = Instantiate(weaponPrefab, transform.position, transform.rotation, transform);
            Weapon newWeapon = newWeaponObj.GetComponent<Weapon>();

            if (newWeapon == null)
            {
                Debug.LogError("The weapon prefab does not have a Weapon component attached.");
                Destroy(newWeaponObj);
                return false;
            }

            newWeaponObj.SetActive(false);
            weaponSlots.Add(newWeapon);

            if (weaponSlots.Count == 1)
            {
                EquipWeapon(0);
            }

            Debug.Log("Success! Item Added.");
            return true;
        }

        Debug.Log("Inventory Full! Cannot add more weapons.");// Logs a message to indicate that the inventory is full and cannot accept more weapons when the maximum number of weapon slots has been reached
        return false;


    }

    // Method to equip a weapon from the inventory based on the provided index, which checks if the index is valid and then equips the corresponding weapon from the weaponSlots list
    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Count) return;// Checks if the provided index is within the valid range of the weaponSlots list, and if not, returns early to prevent errors

        if (playerCombat.currentWeapon != null)// Checks if there is currently an equipped weapon in the PlayerCombat script, and if so, deactivates it to prepare for equipping the new weapon
        {
            playerCombat.currentWeapon.gameObject.SetActive(false);// Deactivates the currently equipped weapon GameObject to hide it from the game world before equipping the new weapon
        }

        currentWeaponIndex = index;// Updates the currentWeaponIndex to the new index provided, which will be used to track the currently equipped weapon in the inventory
        Weapon selectedWeapon = weaponSlots[currentWeaponIndex];// Gets the weapon from the weaponSlots list at the currentWeaponIndex to be equipped by the player
        selectedWeapon.gameObject.SetActive(true);// Activates the selected weapon GameObject to make it visible and interactable in the game world as the currently equipped weapon

        playerCombat.currentWeapon = selectedWeapon;// Updates the currentWeapon reference in the PlayerCombat script to the newly equipped weapon, allowing the player's combat system to use the properties and methods of the new weapon for attacks and other combat interactions
        Debug.Log($"Equipped {selectedWeapon.weaponName}.");// Logs a message to indicate that a new weapon has been equipped, displaying the name of the weapon for feedback to the player

    }

    // Update Method to handle input for switching weapons in the inventory, which listens for the Q key press and cycles through the available weapons in the inventory when pressed
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))// Checks if the Q key is pressed down, and if so, proceeds to switch to the next weapon in the inventory
        {
            if (weaponSlots.Count > 1)// Checks if there is more than one weapon in the inventory before allowing the player to switch weapons, as switching is only necessary if there are multiple options available
            {
                int nextIndex = (currentWeaponIndex + 1) % weaponSlots.Count;// Calculates the index of the next weapon to equip by incrementing the currentWeaponIndex 
                EquipWeapon(nextIndex);// Calls the EquipWeapon method with the calculated next index to switch to the next weapon in the inventory when the Q key is pressed   
            }
        }
    }

    public void DropCurrentWeapon()
    {
        if (weaponSlots.Count == 0) return;

        //Gets the weapon we are holding
        Weapon weaponToDrop = weaponSlots[currentWeaponIndex];

        //Teleports it back to the floor and turn it back on
        weaponToDrop.transform.SetParent(null);
        weaponToDrop.gameObject.SetActive(true);
        weaponToDrop.transform.position = transform.position + transform.up * 1.2f;

        //Removes it from the list
        weaponSlots.RemoveAt(currentWeaponIndex);

        //Resets the hand
        if (weaponSlots.Count > 0)
        {
            currentWeaponIndex = 0;
            EquipWeapon(0);
        }
        else
        {
            GetComponent<PlayerCombat>().currentWeapon = null;
        }
    }
    public void RemoveBrokenWeapon(Weapon brokenWeapon)
    {
        if (weaponSlots.Contains(brokenWeapon))
        {
            Debug.Log("Removing shattered " + brokenWeapon.weaponName + " from inventory.");

            //Removes the physical link and the list entry
            weaponSlots.Remove(brokenWeapon);

            //If the list is now empty, resets completely
            if (weaponSlots.Count == 0)
            {
                currentWeaponIndex = 0;
                if (GetComponent<PlayerCombat>() != null)
                {
                    GetComponent<PlayerCombat>().currentWeapon = null;
                }
                Debug.Log("Inventory Empty: Protagonist is unarmed.");
            }
            else
            {
                //Keeps the index within the new smaller list size
                //This prevents the game from looking for a 3rd item that no longer exists
                currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, weaponSlots.Count - 1);

                //Auto-equips the next available weapon so combat doesn't stop
                EquipWeapon(currentWeaponIndex);
            }
        }
    }

}
