using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UI;

public class UIAPanelImage : MonoBehaviour
{
    
    // Begin UIA Panel Overlay Objects
    public GameObject PWREV1;
    public GameObject PWREV2;

    public GameObject O2EV1;
    public GameObject O2EV2;

    public GameObject O2Vent;
    public GameObject H2OWasteEV1;
    public GameObject H2OWasteEV2;
    // public RawImage testImage;

    // Array to hold all overlay objects
    private GameObject[] overlays;

    // End UIA Panel Overlay Objects
    
    // Distance from user
    public float distanceFromUser = 1.0f;
    
    // Height relative to eye level
    public float heightOffset = 0.0f;
    
    
    
    // Reference to the camera (user's head)
    private Camera mainCamera;
    
    void Start()
    {
        // Get references to components
        mainCamera = Camera.main;
        
        // Since the panel is floating and not following user, we need to set its position manually
        // Position should be set to the left of the user
        PositionPanelToLeftOfUser();
        
        overlays = new GameObject[] { PWREV1, PWREV2, O2EV1, O2EV2, O2Vent, H2OWasteEV1, H2OWasteEV2 };

        // Hide all overlays initially
        DeactivateAllOverlays();

        // Change all overlay colors to red (incomplete by default)
        for (int i = 0; i < overlays.Length; i++) {
            ChangeToCustomColor(i, Color.red);
        }
        
    }
    
    // Position the panel to the left of the user at the specified distance
    public void PositionPanelToLeftOfUser()
    {   
        if (mainCamera == null) return;
        
        // Get the user's position and left direction
        Vector3 userPosition = mainCamera.transform.position;
        Vector3 userForward = mainCamera.transform.forward;
        Vector3 userRight = mainCamera.transform.right;
        
        // Calculate position - slightly forward and to the left
        // Reduced the leftward offset from 0.5 to 0.3 meters
        Vector3 panelPosition = userPosition + (userForward * 1.0f) - (userRight * 0.3f);
        
        
        // Set the panel position
        transform.position = panelPosition;

        // Make the panel face the user
        transform.LookAt(userPosition);

        // Add a slight rotation to better face the user
        // This additional rotation helps ensure the panel faces the user properly
        transform.rotation *= Quaternion.Euler(0, 180, 0);
    }
    
    // Toggle a specific overlay by index
    public void ToggleOverlay(int index)
    {
        if (index >= 0 && index < overlays.Length) // out of bounds check
        {
            overlays[index].SetActive(!overlays[index].activeSelf); 
        }

        
    }
    
    // Set a specific overlay's state
    public void SetOverlayState(int index, bool active)
    {
        if (index >= 0 && index < overlays.Length)
        {
            overlays[index].SetActive(active);
        }
    }

    // Deactivate all overlays
    public void DeactivateAllOverlays()
    {
        foreach (GameObject overlay in overlays)
        {
            overlay.SetActive(false);
        }
    }


   // Methods to change overlay colors 
    public void ResetToWhite(int index)
    {
        RawImage image = overlays[index].GetComponentInChildren<RawImage>();
        image.color = Color.white;
    }
    
    // Unity colors
    public void ChangeToCustomColor(int index, Color newColor)
    {
        RawImage image = overlays[index].GetComponentInChildren<RawImage>();
        image.color = newColor;
    }
}
