using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 12f;// Speed of bullet, fast enough to be a threat, slow enough to dodge
    public float damage;// Set by the Enemy class when fired, determined by weapon type
    public float lifetime = 2f;// Makes sure bullet doesn't travel forever

    void Start() => Destroy(gameObject, lifetime); 

    void Update()
    {
        // Moves forward in the direction it is facing
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StatusManager status = other.GetComponent<StatusManager>();
            if (status != null) status.TakeDamage(damage);
            
            Destroy(gameObject); // Bullet disappears on hit
        }
    }
}

