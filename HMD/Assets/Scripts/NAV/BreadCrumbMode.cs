using UnityEngine;
using UnityEngine.Events;
using TMPro;
// using Microsoft.MixedReality.Toolkit.Utilities;
// using Microsoft.MixedReality.Toolkit.Input;
// using Microsoft.MixedReality.Toolkit.CameraSystem;

namespace Microsoft.MixedReality.Toolkit.UI
{
    public class BreadcrumbMode : MonoBehaviour
    {
        [SerializeField] private GameObject breadcrumbPrefab; // Drag your breadcrumb prefab here
        [SerializeField] private float distanceThreshold = 5f; // Adjust the distance threshold as needed
        [SerializeField] private Transform parentObject; 
        [SerializeField] private IMUDataHandler imuDataHandler;        // drag your IMUDataHandler
        [SerializeField] private EVANumberHandler evaNumberHandler;
        // Used for highlighting when a mode is currently activated
        [SerializeField] private GameObject breadCrumbBackplate;
        public TextMeshPro breadCrumbStatusText;
      

        private bool isBreadcrumbModeActive = false;
        private int breadcrumbCount = 1;
        private Vector3 lastBreadcrumbPosition;
        private const float VISIBILITY_DISTANCE = 50f; // Distance threshold for breadcrumb visibility

        private void Start()
        {
            lastBreadcrumbPosition = Camera.main.transform.position;
        }

        public void OnBreadcrumbModeButtonPressed()
        {
            // Toggle between the two modes
            isBreadcrumbModeActive = !isBreadcrumbModeActive;
            if (isBreadcrumbModeActive)
            {
                // Start auto drop if it is active
                StartCoroutine(AutoDropBreadcrumbs());
                HighlightButton(breadCrumbBackplate);
            }
            else
            {
                // Unhighlight button if breadcrumb mode is not active
                UnhighlightButton(breadCrumbBackplate);
            }
            UpdateBreadcrumbStatusText(); // Update the status text
        }

        private void UpdateBreadcrumbStatusText()
        {
            if (breadCrumbStatusText != null)
            {
                breadCrumbStatusText.text = isBreadcrumbModeActive ? "ON" : "OFF";
            }
        }

        private System.Collections.IEnumerator AutoDropBreadcrumbs()
        {
            while (isBreadcrumbModeActive)
            {
                // Calculate the distance between the current position and the last breadcrumb position
                float distance = Vector3.Distance(Camera.main.transform.position, lastBreadcrumbPosition);

                if (distance >= distanceThreshold)
                {
                    // Drop a breadcrumb at the current position
                    GameObject breadcrumb = Instantiate(breadcrumbPrefab, Camera.main.transform.position, Quaternion.identity);

                    breadcrumb.transform.SetParent(parentObject, worldPositionStays: true); // Set parent

                    // Assign the "Breadcrumb" tag to the dropped breadcrumb prefab
                    breadcrumb.tag = "Breadcrumb";

               
                    /*

                    int evaNumber = evaNumberHandler != null ? evaNumberHandler.getEVANumber() : 0;
                    string evaKey = "eva" + evaNumber;

                    // Only proceed if we have a valid EVA number
                    if (evaNumber == 1 || evaNumber == 2)
                    {
                        float x = imuDataHandler.GetPosx(evaKey);          // or just imu.getX()
                        float y = imuDataHandler.GetPosy(evaKey);          // same idea for Y
                       

                    }
                    */

                   
                    // PinRegistry.AddPin(new PinData("breadcrumb", posx, posy, breadcrumbCount.ToString()));

                   

                    breadcrumbCount++;
                    lastBreadcrumbPosition = Camera.main.transform.position;
                }

                yield return null;
            }
        }

        private void HighlightButton(GameObject backplate)
        {
            // Activate the transparent yellow backplate
            if (backplate != null)
            {
                backplate.SetActive(true);
            }
        }

        private void UnhighlightButton(GameObject backplate)
        {
            // Deactivate the transparent yellow backplate
            if (backplate != null)
            {
                backplate.SetActive(false);
            }
        }

    }
}