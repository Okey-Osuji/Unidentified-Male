using UnityEngine;



public class CameraFollow : MonoBehaviour
{
    public Transform target; // Drag player GameObject here
    public float smoothSpeed = 0.125f;// Smoothness of camera speed
    public Vector3 offset = new Vector3(0, 0, -15); // Keeps camera at a distance on Z-axis


    // LateUpdate Method runs after the player moves
    void LateUpdate() 
    {
        if (target == null) return;
        
        //The position where the camera will be at
        Vector3 desiredPosition = target.position + offset;
        // Smoothly interpolate between current position and player position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}

