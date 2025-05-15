using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MixedReality.Toolkit.Subsystems; // You confirmed this works for KeywordRecognitionSubsystem
using MixedReality.Toolkit; // For XRSubsystemHelpers (usually)

[RequireComponent(typeof(FieldNoteHandler))]
public class FieldNoteSpeechCommands : MonoBehaviour
{
    [Header("Speech Command Configuration")]
    [Tooltip("The exact phrase the user needs to say.")]
    public string keyword = "Take Geological Photo";

    private FieldNoteHandler fieldNoteHandler;
    private KeywordRecognitionSubsystem keywordRecognitionSubsystem = null;
    // private StateAggregator stateAggregator; // REMOVED for simplicity / to fix error

    void Awake()
    {
        fieldNoteHandler = GetComponent<FieldNoteHandler>();
        if (fieldNoteHandler == null)
        {
            Debug.LogError("FieldNoteSpeechCommands: FieldNoteHandler component not found on this GameObject.");
            enabled = false; 
            return;
        }
        // stateAggregator = GetComponent<StateAggregator>(); // REMOVED
    }

    void OnEnable()
    {
        // Ensure XRSubsystemHelpers is correctly namespaced. If MixedReality.Toolkit doesn't work,
        // try to find its specific namespace if MRTK has moved it.
        keywordRecognitionSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<KeywordRecognitionSubsystem>();

        if (keywordRecognitionSubsystem != null)
        {
            // Use CreateOrGetEventForKeyword as per MRTK3 documentation example
            keywordRecognitionSubsystem.CreateOrGetEventForKeyword(keyword)
                                     .AddListener(OnKeywordRecognized);
            Debug.Log($"FieldNoteSpeechCommands: Listener added for keyword '{keyword}' on {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("FieldNoteSpeechCommands: KeywordRecognitionSubsystem not found. Speech commands will not work.");
        }
    }

   void OnDisable()
    {
        if (keywordRecognitionSubsystem != null && !string.IsNullOrEmpty(keyword))
        {
            Debug.Log($"FieldNoteSpeechCommands for '{keyword}' on {gameObject.name} is being disabled. Listener cleanup will rely on the subsystem or object destruction.");
        }
        keywordRecognitionSubsystem = null; // Clear the reference
    }

    private void OnKeywordRecognized()
    {
        // if (stateAggregator != null && !stateAggregator.IsGazePinchSelectedOrFocused) // REMOVED
        // {
        //     Debug.Log($"FieldNoteSpeechCommands: Keyword '{keyword}' recognized, but FieldNote not focused/selected. Ignoring.");
        //     return;
        // }

        Debug.Log($"FieldNoteSpeechCommands: Keyword '{keyword}' recognized on {gameObject.name}! Calling FieldNoteHandler.");
        if (fieldNoteHandler != null)
        {
            fieldNoteHandler.GeologicalPhotoCommandReceived();
        }
    }
}