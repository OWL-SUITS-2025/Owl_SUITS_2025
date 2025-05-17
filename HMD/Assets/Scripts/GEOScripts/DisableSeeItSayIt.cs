using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using UnityEngine.UI; // For UI components
using System.Reflection; // For finding components via reflection

/// <summary>
/// Disables all See-it, Say-it labels in the scene while preserving speech recognition functionality.
/// Attach this to a GameObject in your scene (like a manager object).
/// </summary>
public class DisableSeeItSayItLabels : MonoBehaviour
{
    void Start()
    {
        // Find all GameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        int disabledCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Look for components with "SeeItSayItLabel" in their name
            Component[] components = obj.GetComponents<MonoBehaviour>();
            
            foreach (MonoBehaviour component in components)
            {
                // Check if the component name contains "SeeItSayItLabel"
                if (component != null && component.GetType().Name.Contains("SeeItSayItLabel"))
                {
                    // Disable the component
                    component.enabled = false;
                    disabledCount++;
                    Debug.Log($"Disabled See-it, Say-it label on {obj.name}");
                }
            }
            
            // Also check for StateVisualizer components that might control the labels
            foreach (MonoBehaviour component in components)
            {
                if (component != null && component.GetType().Name.Contains("StateVisualizer"))
                {
                    // Check if this visualizer controls speech labels
                    if (obj.name.Contains("SeeItSayIt") || obj.name.Contains("Speech") || obj.name.Contains("Voice"))
                    {
                        component.enabled = false;
                        disabledCount++;
                        Debug.Log($"Disabled speech-related StateVisualizer on {obj.name}");
                    }
                }
            }
        }
        
        Debug.Log($"Disabled {disabledCount} speech-related components");
        
        // Alternative approach: Try to find buttons and clear their speech keywords
        PressableButton[] allButtons = FindObjectsOfType<PressableButton>(true);
        int modifiedButtons = 0;
        
        foreach (PressableButton button in allButtons)
        {
            // Use reflection to find and clear SpeechRecognitionKeyword if it exists
            try {
                var interactable = button.GetComponent<StatefulInteractable>();
                if (interactable != null)
                {
                    // Try to get the property field via reflection
                    var field = interactable.GetType().GetField("speechRecognitionKeyword", 
                                          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (field != null)
                    {
                        field.SetValue(interactable, "");
                        modifiedButtons++;
                        Debug.Log($"Cleared speech keyword on button {button.gameObject.name}");
                    }
                }
            }
            catch (Exception e) {
                Debug.LogWarning($"Could not modify button {button.gameObject.name}: {e.Message}");
            }
        }
        
        Debug.Log($"Modified {modifiedButtons} buttons to clear speech keywords");
    }
}