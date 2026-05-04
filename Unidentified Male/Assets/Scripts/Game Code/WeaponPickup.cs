using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Data")]
    public GameObject weaponPrefab;
    private string actualWeaponName;

    private bool isPlayerInRange = false;
    private GameObject playerObject;

    void Start()
    {
        if (weaponPrefab != null)
        {
            Weapon wScript = weaponPrefab.GetComponent<Weapon>();
            if (wScript != null) actualWeaponName = wScript.weaponName;
        }
        else
        {
            actualWeaponName = "Unknown Weapon";
        }

        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));


        
    }

    void Update()
    {
        // THE F KEY CHECK: This prevents automatic pickup of weapon
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            // Calls the pickup logic even if inventory is full so the player can get an Inventory Full message or swap weapons
            PickUp(playerObject);
        }
    }

    public void PickUp(GameObject player)
    {
        PlayerCombat combat = player.GetComponent<PlayerCombat>();
        if (combat != null)
        {
            // Passes the weapon prefab and the floor position to PlayerCombat so it can add or swap the weapon
            bool success = combat.PickUpOrSwapWeapon(weaponPrefab, transform.position);

            if (success)
            {
                // If the pickup or swap worked, remove this floor item from the scene
                Debug.Log("Successfully picked up " + actualWeaponName);
                Destroy(gameObject);
            }
            else
            {
                // If the pickup failed, keep the item on the floor and tell the player why
                Debug.Log("Inventory Full! You cannot pick up " + actualWeaponName);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // If the player is on the weapon, they CAN choose to pick up weapon or not
            isPlayerInRange = true;
            playerObject = other.gameObject;
            Debug.Log($"Standing over {actualWeaponName}. Press F to pick up.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //Checks when the player leaves the range of any interactable media 
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerObject = null;
        }
    }
}
