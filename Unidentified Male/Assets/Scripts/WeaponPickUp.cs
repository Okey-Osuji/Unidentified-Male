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
        // THE E KEY CHECK: This prevents automatic pickup of weapon
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // This weapon check will only work if the player has room in their inventory
            if (playerObject.GetComponent<PlayerCombat>().unlockedWeapons.Count < 3)
            {
                PickUp(playerObject);
            }
        }
    }

    public void PickUp(GameObject player)
    {
        PlayerCombat combat = player.GetComponent<PlayerCombat>();
        if (combat != null)
        {
            // The weapon PREFAB is passed
            bool success = combat.AddNewWeapon(weaponPrefab);

            if (success)
            {
                Debug.Log("Successfully picked up " + actualWeaponName);
                Destroy(gameObject);
            }
            else
            {
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
            Debug.Log($"Standing over {actualWeaponName}. Press E to pick up.");
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