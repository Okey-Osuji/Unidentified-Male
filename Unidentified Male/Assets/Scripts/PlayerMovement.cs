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
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();// Get the Rigidbody2D component for movement
        keyboard = Keyboard.current;// Get the current keyboard device
    }
    
    void Update()
    {
        if (keyboard == null) return;// If no keyboard is detected, skip input processing
        
        // Read movement input
        moveInput.x = (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);// Calculate horizontal movement input based on WASD keys
        moveInput.y = (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0);// Calculate movement input based on WASD keys
        
        // Read sprint input - this needs to be checked every frame
        isSprinting = keyboard.rightShiftKey.isPressed;
    }
    
    void FixedUpdate()
    {
        if (rb == null) return;// If no Rigidbody2D is attached, skip movement processing
        
        float speed = isSprinting ? sprintSpeed : walkSpeed;// Determine the current speed based on whether the player is sprinting
        rb.linearVelocity = moveInput * speed;// Set the Rigidbody2D's velocity based on the movement input and current speed
    }
}
