using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings and Keyboard Input")]
    public float walkSpeed = 5f;// Base walking speed
    public float sprintSpeed = 8f;// Sprinting speed when the sprint key is held down
    private Rigidbody2D rb;// Reference to the Rigidbody2D component for physics-based movement
    private Vector2 moveInput;// Stores the current movement input from the player
    private bool isSprinting;// Flag to track whether the player is currently sprinting
    
    private Keyboard keyboard;// Reference to the keyboard input device
    public Animator animator;// Reference to the Animator component, which can be used to control the player's animations based on movement and actions
    public bool isMoving = false;// Initializes the "IsMoving" parameter in the Animator to false, which can be used to control the player's movement animations based on whether they are currently moving or not
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();// Get the Rigidbody2D component for movement
        keyboard = Keyboard.current;// Get the current keyboard device
        animator = GetComponent<Animator>();// Get the Animator component
    }
    
    void Update()
{
    if (keyboard == null) return;
    
    // Reads movement input
    moveInput.x = (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
    moveInput.y = (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0);
    
    isSprinting = keyboard.rightShiftKey.isPressed;

    // Rotation Logic
    // Only rotates if the player is actually pressing a movement key
    if (moveInput != Vector2.zero)
    {
        // Calculates the angle (in radians) then convert to degrees
        // Atan2 takes (y, x)
        float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;

        // Applies the rotation
        // Since soldier is drawn facing UP, we subtract 90 degrees 
        // to align Unity's "Right" (0°) with the player sprite's "Top".
        transform.rotation = Quaternion.Euler(0, 0, targetAngle - 90f);
        animator.SetBool("IsMoving", true); // Set the "IsMoving" parameter in the Animator to true when there is movement input
    }
    else
    {
        animator.SetBool("IsMoving", false); // Set the "IsMoving" parameter in the Animator to false when there is no movement input
    }
}
    
    void FixedUpdate()
    {
        if (rb == null) return;// If no Rigidbody2D is attached, skip movement processing
        
        float speed = isSprinting ? sprintSpeed : walkSpeed;// Determine the current speed based on whether the player is sprinting
        rb.linearVelocity = moveInput * speed;// Set the Rigidbody2D's velocity based on the movement input and current speed
    }
}
