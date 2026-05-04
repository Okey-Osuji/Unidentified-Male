using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 12f; // Speed of bullet
    public float damage;// Set by the shooter (Enemy or Player) when fired
    public float lifetime = 2f; // Makes sure bullet doesn't travel forever

    void Start() => Destroy(gameObject, lifetime);

    void Update()
    {
        // Moves bullet forward in the direction the player is facing
        transform.position += transform.up * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // --- HIT PLAYER LOGIC ---
        if (other.CompareTag("Player"))
        {
            StatusManager status = other.GetComponent<StatusManager>();
            if (status != null)
            {
                status.TakeDamage(damage);
                Destroy(gameObject); // Bullet disappears on hit
            }
            return; // Exit so we don't check Enemy logic
        }

        // --- HIT ENEMY LOGIC ---
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject); // Bullet disappears on hit
            return;
        }


    }

    void OnDrawGizmos()
    {
        // Draws a green line showing exactly where the bullet should move
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.right * 1f);
    }
}