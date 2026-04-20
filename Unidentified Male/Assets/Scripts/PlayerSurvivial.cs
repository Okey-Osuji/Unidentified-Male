using UnityEngine;

public class PlayerSurvival : MonoBehaviour
{
    private StatusManager status;
    private PlayerCombat combat;

    void Start()
    {
        status = GetComponent<StatusManager>();
        combat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            PerformUniversalInteraction();// Calls PerformUniversalInteraction if E is pressed
        }
    }

    void PerformUniversalInteraction()
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, 1.5f);

        foreach (Collider2D obj in objects)
        {
            
            // Checks for the script component directly instead of just the tag
            WeaponPickup pickup = obj.GetComponent<WeaponPickup>();
            if (pickup != null)
            {
                // Calls the PickUp method in WeaponPickup.cs
                // This handles the "Inventory Full" check written in PlayerCombat
                pickup.PickUp(gameObject);
                return; 
            }

            // CORPSE INTERACTION
            if (obj.CompareTag("Corpse"))
            {
                if (status != null)
                {
                    status.Consume(); // Replenishes hunger/health
                    Destroy(obj.gameObject); // Removes the corpse from the block
                }
                return;
            }
        }
    }
}