using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.Subsystems;

[RequireComponent(typeof(PinPointButton))]
public class PinPointSpeechCommands : MonoBehaviour
{
    [Header("Speech Command Configuration")]
    [Tooltip("The exact phrase the user needs to say.")]
    public string keyword = "Place Pin";

    private PinPointButton pinPointButton;
    private KeywordRecognitionSubsystem keywordRecognitionSubsystem = null;

    void Awake()
    {
        pinPointButton = GetComponent<PinPointButton>();
        if (pinPointButton == null)
        {
            Debug.LogError("PinPointSpeechCommands: PinPointButton component not found on this GameObject.");
            enabled = false; 
            return;
        }
    }

    void OnEnable()
    {
        keywordRecognitionSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<KeywordRecognitionSubsystem>();

        if (keywordRecognitionSubsystem != null)
        {
            keywordRecognitionSubsystem.CreateOrGetEventForKeyword(keyword)
                                     .AddListener(OnKeywordRecognized);
            Debug.Log($"PinPointSpeechCommands: Listener added for keyword '{keyword}' on {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("PinPointSpeechCommands: KeywordRecognitionSubsystem not found. Speech commands will not work.");
        }
    }

    void OnDisable()
    {
        if (keywordRecognitionSubsystem != null && !string.IsNullOrEmpty(keyword))
        {
            Debug.Log($"PinPointSpeechCommands for '{keyword}' on {gameObject.name} is being disabled. Listener cleanup will rely on the subsystem or object destruction.");
        }
        keywordRecognitionSubsystem = null; // Clear the reference
    }

    private void OnKeywordRecognized()
    {
        Debug.Log($"PinPointSpeechCommands: Keyword '{keyword}' recognized on {gameObject.name}! Calling PinPointButton.");
        if (pinPointButton != null)
        {
            pinPointButton.OnPlacePinCommand();
        }
    }
}