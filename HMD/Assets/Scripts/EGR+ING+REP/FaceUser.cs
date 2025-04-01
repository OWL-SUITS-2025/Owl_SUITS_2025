using UnityEngine;

public class FaceUser : MonoBehaviour
{
    // Optional: Control rotation speed for smoother turning
    public float rotationSpeed = 5.0f;
    
    // Optional: Only rotate horizontally (Y-axis)
    public bool horizontalOnly = false;

    // Optional: Only rotate vertically (X-axis)
    public bool verticalOnly = false;
    // Reference to the main camera (user's head)
    private Camera mainCamera;
    
    void Start()
    {
        // Find the main camera
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        
        Vector3 directionToUser;
        
        if (horizontalOnly)
        {
            // Calculate direction to user, but only on the horizontal plane
            // panel looks left and right
            Vector3 userPositionSameHeight = new Vector3(
                mainCamera.transform.position.x,
                transform.position.y,
                mainCamera.transform.position.z
            );
            directionToUser = userPositionSameHeight - transform.position;
        }
        else if (verticalOnly)
        {
            // Calculate direction to user, but only on the vertical plane
            // panel looks up and down
            Vector3 userPositionSameHeight = new Vector3(
                transform.position.x,
                mainCamera.transform.position.y,
                mainCamera.transform.position.z
            );
            directionToUser = userPositionSameHeight - transform.position;
        }
        else
        {
            // Calculate direction to user in 3D space
            directionToUser = mainCamera.transform.position - transform.position;
        }
        
        // Skip if we're too close to the user (prevents jittery rotation)
        if (directionToUser.magnitude < 0.01f) return;
        
        // Calculate the target rotation to face the user
        Quaternion targetRotation = Quaternion.LookRotation(directionToUser);
        
        // Add the 180-degree Y rotation since UI typically faces backward
        targetRotation *= Quaternion.Euler(0, 180, 0);
        
        if (rotationSpeed > 0)
        {
            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            // Instantly face the user
            transform.rotation = targetRotation;
        }
    }
}