using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PerformInteraction();
        }
    }

    void PerformInteraction()
    {
        // Scan the area around the Triangle
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        
        GameObject bestWeapon = null;
        GameObject bestCorpse = null;

        foreach (var hit in hits)
        {
            // Checks for Weapon
            if (hit.GetComponent<WeaponPickup>() != null)
            {
                bestWeapon = hit.gameObject;
                break; // If the weapon is found, the code stops
            }
            // Checks for Corpse
            if (hit.CompareTag("Corpse"))
            {
                bestCorpse = hit.gameObject;
            }
        }

        // Logic for interaction with item pickup and corpse consumption
        if (bestWeapon != null)
        {
            bestWeapon.GetComponent<WeaponPickup>().PickUp(gameObject);
        }
        else if (bestCorpse != null)
        {
            GetComponent<StatusManager>().Consume();
            Destroy(bestCorpse);
        }
    }
}
