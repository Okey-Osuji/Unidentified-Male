using UnityEngine;

public class PlayerSurvival : MonoBehaviour
{
    private StatusManager status;
    private PlayerCombat combat;
    [SerializeField] private AudioSource audioSource; // Reference to the AudioSource component for playing sound effects
    [SerializeField] private AudioClip eatSound; // Sound effect for eating a corpse

    void Start()
    {
        status = GetComponent<StatusManager>();
        combat = GetComponent<PlayerCombat>();
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component for playing sound effects
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
        // E scans for corpses only, because weapons/items are picked up with F
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, 1.5f);

        foreach (Collider2D obj in objects)
        {
            // CORPSE INTERACTION
            if (obj.CompareTag("Corpse"))
            {
                if (status != null)
                {
                    status.Consume(); // Replenishes hunger/health
                    Destroy(obj.gameObject); // Removes the corpse from the block
                    audioSource.PlayOneShot(eatSound); // Plays the eating sound effect when consuming a corpse
                      
                }
                return;
            }
        }
    }
}
