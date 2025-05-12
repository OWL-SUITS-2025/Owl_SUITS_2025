using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class ProcedureChecklistManager : MonoBehaviour
{
    [SerializeField] private PressableButton[] checklistButtons;
    [SerializeField] private PressableButton nextButton;
    [SerializeField] private PressableButton backButton; // Added back button reference
    [SerializeField] private StepBoxController stepBoxController; // Reference to the StepBoxController
    
    private ProcedureStep currentStep = ProcedureStep.StartVoiceRecording;
    
    // Event that will be triggered when the step changes
    public static event Action<ProcedureStep> OnStepChanged;
    
    private void Start()
    {
        // Initialize UI state
        UpdateChecklistUI();
        currentStep = ProcedureStep.StartVoiceRecording;
        
        // Set up button event listeners
        for (int i = 0; i < checklistButtons.Length; i++)
        {
            int buttonIndex = i; // Capture for lambda
            checklistButtons[i].OnClicked.AddListener(() => OnChecklistButtonClicked(buttonIndex));
        }
        
        if (nextButton != null)
        {
            nextButton.OnClicked.AddListener(GoToNextStep);
        }
        
        // Set up back button listener
        if (backButton != null)
        {
            backButton.OnClicked.AddListener(GoToPreviousStep);
        }
        
        // Trigger initial step
        TriggerStepChanged();
    }
    
    private void OnChecklistButtonClicked(int buttonIndex)
    {
        // Only allow clicking the button for the current step
        if ((int)currentStep == buttonIndex)
        {
            // Check if this is the last button in the array
            if (buttonIndex == checklistButtons.Length - 1)
            {
                // Start the coroutine to complete the checklist
                StartCoroutine(ResetAfterCompletion());
            }
            else
            {
                GoToNextStep();
            }
        }
    }
    
    private IEnumerator ResetAfterCompletion()
    {
        // Wait a short time to allow the Completed message to be seen
        yield return new WaitForSeconds(3.0f);

        // 1. Reset step box to step 0
        if (stepBoxController != null)
        {
            stepBoxController.ShowDefaultStep();
        }
        else
        {
            Debug.LogWarning("StepBoxController reference is missing in ProcedureChecklistManager");
        }
        
        // 2. Reset all checklist buttons visual state
        ResetChecklistButtons();
        
        // 3. Deactivate the checklist GameObject
        gameObject.SetActive(false);
    }
    
    private void ResetChecklistButtons()
    {
        // Reset the state of all buttons
        foreach (PressableButton button in checklistButtons)
        {
            // Use the appropriate method to untoggle based on your button implementation
            // For MRTK3 PressableButton, we would typically reset the visual state
            StatefulInteractable statefulInteractable = button.GetComponent<StatefulInteractable>();
            if (statefulInteractable != null)
            {
                statefulInteractable.ForceSetToggled(false);
            }
        }
        
        // Reset the current step to the beginning
        currentStep = ProcedureStep.StartVoiceRecording;
    }
    
    public void GoToNextStep()
    {
        // Advance to the next step if we haven't completed all steps
        if (currentStep < ProcedureStep.Completed)
        {
            currentStep = (ProcedureStep)((int)currentStep + 1);
            UpdateChecklistUI();
            TriggerStepChanged();
        }
    }
    
    public void GoToPreviousStep()
    {
        // Go back to the previous step if we're not at the first step
        if (currentStep > ProcedureStep.StartVoiceRecording)
        {
            currentStep = (ProcedureStep)((int)currentStep - 1);
            UpdateChecklistUI();
            TriggerStepChanged();
        }
        // If we're at the first step, do nothing as requested
    }
    
    private void UpdateChecklistUI()
    {
        // Update which buttons are checked
        for (int i = 0; i < checklistButtons.Length; i++)
        {
            // Set visual state of buttons based on current step
            // This assumes you have a way to visually change the button state
            // You may need to adjust this based on the specific implementation of your checklist
            if (i < (int)currentStep)
            {
                // Mark steps before current as completed
                // Implementation depends on your UI setup
            }
        }
    }
    
    private void TriggerStepChanged()
    {
        // Notify listeners about the step change
        OnStepChanged?.Invoke(currentStep);
    }

    // Add this OnEnable method to your ProcedureChecklistManager.cs script

    private void OnEnable()
    {
        // Every time this GameObject is activated, reset to the first step
        currentStep = ProcedureStep.StartVoiceRecording;
        
        // Update UI
        UpdateChecklistUI();
        
        // Trigger the step changed event to update the StepBoxController
        TriggerStepChanged();
    }
}