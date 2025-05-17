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
    [SerializeField] private PressableButton backButton; 
    [SerializeField] private StepBoxController stepBoxController; 

    private ProcedureStep currentStep = ProcedureStep.StartVoiceRecording;
    public static event Action<ProcedureStep> OnStepChanged;
    
    // New event for when scan step needs to be updated with significance info
    public static event Action<bool> OnScanStepSignificanceUpdate;
    
    private FieldNoteHandler activeFieldNoteHandler;
    
    // Track whether scan is completed for the current step
    private bool isScanCompleted = false;
    private bool isSampleSignificant = false;

    public void SetActiveFieldNoteHandler(FieldNoteHandler handler)
    {
        activeFieldNoteHandler = handler;
    }

    public FieldNoteHandler GetActiveFieldNoteHandler()
    {
        return activeFieldNoteHandler;
    }
    
    // Get scan completion status
    public bool IsScanCompleted()
    {
        return isScanCompleted;
    }
    
    // Get the significance status
    public bool GetSampleSignificanceStatus()
    {
        return isSampleSignificant;
    }

    // Method to update the scan step text with significance information
    public void UpdateScanStepText(bool isSignificant)
    {

        Debug.Log("ProcedureChecklistManager.UpdateScanStepText called. currentStep: " + currentStep + ", isSignificant: " + isSignificant);
        // Only update if we're currently in the scan step
        if (currentStep == ProcedureStep.ScanSample)
        {
            // Store the significance and scan completion status
            isSampleSignificant = isSignificant;
            isScanCompleted = true;
            
            // Trigger the event to update the step text
            OnScanStepSignificanceUpdate?.Invoke(isSignificant);
            
            Debug.Log($"Scan completed. Sample is {(isSignificant ? "significant" : "not significant")}");
        }
    }

    // Rest of your existing code remains unchanged...
    private void Start()
    {
        // Initialize UI state
        UpdateChecklistUI();
        currentStep = ProcedureStep.StartVoiceRecording; 

        for (int i = 0; i < checklistButtons.Length; i++)
        {
            int buttonIndex = i; 
            if (checklistButtons[i] != null) 
            {
                 checklistButtons[i].OnClicked.AddListener(() => OnChecklistButtonClicked(buttonIndex));
            }
        }

        if (nextButton != null)
        {
            nextButton.OnClicked.AddListener(GoToNextStep);
        }
        if (backButton != null)
        {
            backButton.OnClicked.AddListener(GoToPreviousStep);
        }
        
        TriggerStepChanged(); 
    }

    private void OnChecklistButtonClicked(int buttonIndex)
    {
        if ((int)currentStep == buttonIndex)
        {
            if (currentStep == ProcedureStep.StartVoiceRecording) 
            {
                if (activeFieldNoteHandler != null)
                {
                    activeFieldNoteHandler.StartRecording();
                }
                else
                {
                    Debug.LogWarning("ProcedureChecklistManager: ActiveFieldNoteHandler is null. Cannot start recording.");
                }
            }
            
            if (currentStep == ProcedureStep.Complete) 
            {
                if (activeFieldNoteHandler != null)
                {
                    activeFieldNoteHandler.StopRecording();
                }
                else
                {
                    Debug.LogWarning("ProcedureChecklistManager: ActiveFieldNoteHandler is null. Cannot stop recording.");
                }
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
        currentStep = ProcedureStep.Completed; 
        TriggerStepChanged(); 

        yield return new WaitForSeconds(3.0f);

        if (stepBoxController != null)
        {
            stepBoxController.ShowDefaultStep();
        }
        ResetChecklistButtons();
        gameObject.SetActive(false);
        activeFieldNoteHandler = null; 
    }

    private void ResetChecklistButtons()
    {
        foreach (PressableButton button in checklistButtons)
        {
            if (button != null) 
            {
                StatefulInteractable statefulInteractable = button.GetComponent<StatefulInteractable>();
                if (statefulInteractable != null)
                {
                    statefulInteractable.ForceSetToggled(false);
                }
            }
        }
        
        // Reset statuses when creating a new field note
        isScanCompleted = false;
        isSampleSignificant = false;
    }
    
    public void GoToNextStep()
    {
        if (currentStep < ProcedureStep.Complete) 
        {
            currentStep = (ProcedureStep)((int)currentStep + 1);
            UpdateChecklistUI();
            TriggerStepChanged();
        }
    }

    public void GoToPreviousStep()
    {
        if (currentStep == ProcedureStep.Completed) 
        {
            currentStep = ProcedureStep.Complete;
            UpdateChecklistUI();
            TriggerStepChanged();
        }
        else if (currentStep > ProcedureStep.StartVoiceRecording)
        {
            currentStep = (ProcedureStep)((int)currentStep - 1);
            UpdateChecklistUI();
            TriggerStepChanged();
        }
    }
    
    private void UpdateChecklistUI()
    {
        for (int i = 0; i < checklistButtons.Length; i++)
        {
            if (checklistButtons[i] == null) continue; 

            StatefulInteractable interactable = checklistButtons[i].GetComponent<StatefulInteractable>();
            if (interactable != null)
            {
                if (i < (int)currentStep)
                {
                    interactable.ForceSetToggled(true);
                }
                else
                {
                     interactable.ForceSetToggled(false);
                }
            }
        }
        if (nextButton != null) nextButton.gameObject.SetActive(currentStep < ProcedureStep.Complete);
        if (backButton != null) backButton.gameObject.SetActive(currentStep > ProcedureStep.StartVoiceRecording && currentStep <= ProcedureStep.Completed);
    }
    
    private void TriggerStepChanged()
    {
        OnStepChanged?.Invoke(currentStep);
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(InitializeChecklistOnEnable());
    }

    private IEnumerator InitializeChecklistOnEnable()
    {
        yield return null; 

        currentStep = ProcedureStep.StartVoiceRecording;
        isScanCompleted = false;
        isSampleSignificant = false;
        UpdateChecklistUI();
        TriggerStepChanged();
    }
}