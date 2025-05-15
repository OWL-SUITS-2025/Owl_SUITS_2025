using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class StepBoxController : MonoBehaviour
{
    [SerializeField] private TextMeshPro stepInstructionText;
    
    // Direct reference to the checklist manager, assignable in inspector
    [SerializeField] private ProcedureChecklistManager checklistManager;
    
    // Flag to track if we're in Step 0 (default state)
    private bool isInDefaultStep = true;
    
    private void Start()
    {
        // Initialize with default step
        ShowDefaultStep();
        
        // Validate the checklist manager reference
        if (checklistManager == null)
        {
            Debug.LogError("StepBoxController: ProcedureChecklistManager reference not set in inspector!");
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to step change events
        ProcedureChecklistManager.OnStepChanged += UpdateStepInstructions;
        
        // Subscribe to the scan step significance update event
        ProcedureChecklistManager.OnScanStepSignificanceUpdate += UpdateScanStepWithSignificance;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        ProcedureChecklistManager.OnStepChanged -= UpdateStepInstructions;
        ProcedureChecklistManager.OnScanStepSignificanceUpdate -= UpdateScanStepWithSignificance;
    }
    
    /// <summary>
    /// Updates the scan step text with significance information
    /// </summary>
    private void UpdateScanStepWithSignificance(bool isSignificant)
    {
        Debug.Log("StepBoxController.UpdateScanStepWithSignificance event handler called. isSignificant: " + isSignificant);
        Debug.Log("StepBoxController: checklistManager is null: " + (checklistManager == null));
        
        // Make sure we're still on the scan step and references are valid
        if (checklistManager != null && 
            checklistManager.IsScanCompleted() && 
            stepInstructionText != null)
        {
            string stepNumber = "4"; // Updated step number for scan step (was 3, now 4)
            string instruction;
            
            // Display different instructions based on significance
            if (isSignificant)
            {
                instruction = "Step " + stepNumber + ": Sample Evaluation\n\n" +
                            "• <b><u><color=#FFFF00>SCIENTIFICALLY SIGNIFICANT SAMPLE DETECTED</color></u></b>\n" +
                            "• Analyze sample and complete field note\n" +
                            "• Carefully collect and store sample\n" +
                            "• Comm: \"Significant sample collected\"";
            }
            else
            {
                instruction = "Step " + stepNumber + ": Sample Evaluation\n\n" +
                            "• <b><u>Sample is not scientifically significant</u></b>\n" +
                            "• Analyze sample and complete field note\n" +
                            "• Do not collect sample\n" +
                            "• Comm: \"Sample not significant\"";
            }
            
            // Update the text in the step box
            stepInstructionText.text = instruction;
            
            Debug.Log($"StepBoxController: Updated scan step text with significance: {isSignificant}");
        }
    }
    
    /// <summary>
    /// Public method to reset to Step 0 (default state)
    /// </summary>
    public void ShowDefaultStep()
    {
        isInDefaultStep = true;
        
        // Step 0 instructions
        string instruction = "Ready to Begin\n\n" +
                          "1. Announce over comms:\n" +
                          "   • \"Arrived at site, beginning sampling\n" +
                          "2. Raise right palm up to open Geo Hand Menu\n" +
                          "   • Click \"Start New Sample \" Button";
        
        // Update the text in the step box
        if (stepInstructionText != null)
        {
            stepInstructionText.text = instruction;
        }
    }
    
    private void UpdateStepInstructions(ProcedureStep step)
    {
        // When we get a step update from the checklist, we're no longer in the default step
        isInDefaultStep = false;
        
        string stepNumber = ((int)step + 1).ToString();
        string instruction = "";
        
        switch (step)
        {
            case ProcedureStep.StartVoiceRecording:
                instruction = "Step " + stepNumber + ": Start Voice Recording\n\n" +
                            "• Press voice recording button on task list to start \n" + 
                            "• Voice all thoughts and analysis outloud";
                break;
            case ProcedureStep.SetupFieldNote:
                instruction = "Step " + stepNumber + ": Set Up Field Note\n\n" +   
                            "• Grab Titles to reposition Field Notes/Checklist\n" +  
                            "• Organize and clear workspace\n" +
                            "• <u> Toggle field note edit switch on </u>";
                break;
            case ProcedureStep.TakePhoto:    
                instruction = "Step " + stepNumber + ": Take a Photo\n\n" +
                            "• Ensure sample is completely in view\n" +
                            "• Capture an image by saying: \n" +
                            "- \"Take Geological Photo\" \n" +
                            "• Check photo on Field Note, retake if needed";

                break;
            case ProcedureStep.ScanSample:
                // Only show the updated text if the scan has already been completed
                if (checklistManager != null && checklistManager.IsScanCompleted())
                {
                    // Call the update method with the current significance status
                    UpdateScanStepWithSignificance(checklistManager.GetSampleSignificanceStatus());
                    return; // Return early since we're handling the text update in the other method
                }
                else
                {
                    // Initial scan step text before scan is completed
                    instruction = "Step " + stepNumber + ": Perform XRF Scan\n\n" +
                                "• Press and hold trigger to start XRF scan\n" +
                                "• Aim close to sample until confrimation beep\n" +
                                "• Release trigger and check readings on field note\n";
                }
                break;
            case ProcedureStep.FinishFieldNote:
                instruction = "Step " + stepNumber + ": Finish Field Note\n\n" +
                            "• Ensure fieldnote is filled out\n" + 
                            "• Press Complete button to save fieldnote";
                break;
            case ProcedureStep.Complete:
                instruction = "Step " + stepNumber + ": Complete Process\n\n" +
                            "• Announce over comms:\n" +
                            " - Sampling complete, PR verify receipt of data\n" + 
                            " - Beginning nav to next location\n"+
                            "• Pin automatically dropped\n";
                break;
            case ProcedureStep.Completed:
                instruction = "Procedure Completed!\n\n" +
                            "• All data saved in field notebook\n";
                break;
        }
        
        // Update the text in the step box
        if (stepInstructionText != null)
        {
            stepInstructionText.text = instruction;
        }
    }
}