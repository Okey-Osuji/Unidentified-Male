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
        
        GameObject bestCorpse = null;

        foreach (var hit in hits)
        {
            // Checks for Corpse
            if (hit.CompareTag("Corpse"))
            {
                bestCorpse = hit.gameObject;
            }
        }

        // E is only used for corpse consumption now, while weapon pickup is handled by F in WeaponPickup
        if (bestCorpse != null)
        {
            GetComponent<StatusManager>().Consume();
            Destroy(bestCorpse);
        }
    }
}
