using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 12f; // Speed of bullet
    public float damage;// Set by the shooter (Enemy or Player) when fired
    public float lifetime = 2f; // Makes sure bullet doesn't travel forever

    private const float RaycastSkin = 0.05f;// Small extra distance added to the bullet raycast so fast bullets do not skip thin colliders
    private Rigidbody2D rb;// Reference to the bullet's Rigidbody2D, which is used to move the bullet through Unity's physics system
    private Collider2D bulletCollider;// Reference to the bullet's collider, which is disabled after impact so the bullet cannot hit anything else
    private SpriteRenderer spriteRenderer;// Reference to the bullet's SpriteRenderer, which is hidden immediately when the bullet hits something
    private bool hasHit;// Tracks whether the bullet has already hit something, preventing it from dealing damage more than once

    // Awake runs before Start and caches the components the bullet needs while it is alive
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();// Finds the Rigidbody2D attached to this bullet
        bulletCollider = GetComponent<Collider2D>();// Finds the Collider2D attached to this bullet
        spriteRenderer = GetComponent<SpriteRenderer>();// Finds the SpriteRenderer attached to this bullet
    }

    // Start launches the bullet forward and schedules it to be destroyed after its lifetime expires
    void Start()
    {
        if (rb != null)
        {
            rb.linearVelocity = transform.up * speed;// Moves the bullet in the direction it is facing
        }

        Destroy(gameObject, lifetime);// Destroys the bullet after a short time so missed shots do not stay in the scene forever
    }

    // FixedUpdate checks the space in front of the bullet so fast shots cannot visually pass through targets before the trigger fires
    void FixedUpdate()
    {
        if (hasHit) return;// Stops checking after the bullet has already hit something

        float travelDistance = speed * Time.fixedDeltaTime + RaycastSkin;// Calculates how far the bullet will move during this physics step
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.up, travelDistance);// Casts a line in front of the bullet to find targets before the bullet passes them

        RaycastHit2D closestHit = new RaycastHit2D();
        bool foundTarget = false;
        float closestDistance = float.MaxValue;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider == bulletCollider) continue;// Ignores empty hits and the bullet's own collider
            if (!CanDamageCollider(hit.collider)) continue;// Ignores objects that are not valid bullet targets

            if (hit.distance < closestDistance)
            {
                closestHit = hit;// Stores the closest valid target so the bullet stops at the first thing it should hit
                closestDistance = hit.distance;
                foundTarget = true;
            }
        }

        if (foundTarget)
        {
            transform.position = closestHit.point;// Moves the bullet to the impact point before hiding it
            DamageCollider(closestHit.collider);// Applies damage to the target that was hit
        }
    }

    // OnTriggerEnter2D runs when the bullet's trigger collider overlaps another 2D collider
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;// Prevents one bullet from damaging multiple targets if more than one trigger event fires

        DamageCollider(other);// Handles damage if the trigger touches a valid target
    }

    // CanDamageCollider checks whether the collider belongs to a player or enemy that bullets can damage
    bool CanDamageCollider(Collider2D other)
    {
        if (other.CompareTag("Player")) return other.GetComponent<StatusManager>() != null;// Allows bullets to damage the player if the player has a StatusManager

        Enemy enemy = other.GetComponentInParent<Enemy>();// Gets the enemy script, even if the collider is on a child object
        return enemy != null && !enemy.isDead;// Allows bullets to damage living enemies
    }

    // DamageCollider applies bullet damage to the player or enemy attached to the collider that was hit
    void DamageCollider(Collider2D other)
    {
        if (hasHit) return;// Prevents damage from being applied more than once

        // --- HIT PLAYER LOGIC ---
        if (other.CompareTag("Player"))
        {
            StatusManager status = other.GetComponent<StatusManager>();// Gets the player's health/status script
            if (status != null)
            {
                status.TakeDamage(damage);// Applies bullet damage to the player
                HitAndDestroy(); // Bullet disappears on hit
            }
            return; // Exit so we don't check Enemy logic
        }

        // --- HIT ENEMY LOGIC ---
        Enemy enemy = other.GetComponentInParent<Enemy>();// Gets the enemy script, even if the collider is on a child object
        if (enemy != null)
        {
            enemy.TakeDamage(damage);// Applies bullet damage to the enemy
            HitAndDestroy(); // Bullet disappears on hit
            return;
        }


    }

    // HitAndDestroy stops the bullet, hides it immediately, then destroys it from the scene
    void HitAndDestroy()
    {
        hasHit = true;// Marks the bullet as used so it cannot damage another target

        if (rb != null) rb.linearVelocity = Vector2.zero;// Stops the bullet from moving after impact
        if (bulletCollider != null) bulletCollider.enabled = false;// Turns off collision so the bullet cannot trigger more hits
        if (spriteRenderer != null) spriteRenderer.enabled = false;// Hides the bullet sprite immediately on impact

        gameObject.SetActive(false);// Disables the whole bullet immediately so it cannot be seen for the rest of the frame
        Destroy(gameObject);// Removes the bullet object from the scene
    }

    // OnDrawGizmos draws a debug ray in the Scene view to show the bullet's forward direction
    void OnDrawGizmos()
    {
        // Draws a green line showing exactly where the bullet should move
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * 1f);
    }
}
