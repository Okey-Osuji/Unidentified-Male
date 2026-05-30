using UnityEngine;

public class FirearmWeapon : Weapon
{
    [Header("Firearm Settings")]
    public GameObject bulletPrefab; //Define prefab for the bullet
    private AudioSource audioSource;// Reference to the AudioSource component for playing sound effects
    [SerializeField] private AudioClip shootSound; // Sound effect for shooting the firearm
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        isFirearm = true; // Set the isFirearm property to true for firearm weapons
        audioSource = GetComponent<AudioSource>();
    }

    public override void Use()
{
    // Durability check
    if (isDepleted)
    {
        Debug.Log($"{weaponName} is broken!");
        return;
    }

    if (bulletPrefab != null)
    {
        // Gets the player's attackPoint so the bullet starts from the gun barrel position
        Transform firePoint = transform.root.GetComponent<PlayerCombat>().attackPoint;

        // Spawns the bullet using the firePoint rotation so it shoots in the direction the player is facing
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            audioSource.PlayOneShot(shootSound); // Plays the shooting sound effect when the firearm is used
        // Sets damage of bullet in relation to weapon
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = this.damageValue;
        }
    }
    else
    {
        Debug.LogWarning($"{weaponName} is missing a Bullet Prefab!");
    }

    ReduceDurability();
}
}
