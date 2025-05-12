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
    
    // Flag to track if we're in Step 0 (default state)
    private bool isInDefaultStep = true;
    
    private void Start()
    {
        // Initialize with Step 0
        ShowDefaultStep();
    }
    
    private void OnEnable()
    {
        // Subscribe to step change events
        ProcedureChecklistManager.OnStepChanged += UpdateStepInstructions;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from step change events
        ProcedureChecklistManager.OnStepChanged -= UpdateStepInstructions;
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
                          "2. Click \"Start New Sample\" Button in Geo Hand Menu\n";
        
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
                            "• Press the voice recording button on task list to start voice recording\n" + 
                            "• Voice all thoughts out loud during sampling";
                break;
            case ProcedureStep.SetupFieldNote:
                instruction = "Step " + stepNumber + ": Set Up Field Note\n\n" +
                            "• Orientate fieldnote and notebook out of by looking left \n" +
                            "• Toggle on the edit switch on the field note";
                break;
            case ProcedureStep.ScanSample:
                instruction = "Step " + stepNumber + ": Scan the Sample\n\n" +
                            "• Press and hold trigger to start XRF scan\n" +
                            "• Hold steady until confirmation beep\n" +
                            "• Comm: \"Scan Complete, PR Verify data recieved\"";
                break;
            case ProcedureStep.TakePhoto:    
                instruction = "Step " + stepNumber + ": Take a Photo\n\n" +
                            "• Set up diffused lighting\n" +
                            "• Clean camera lens\n" +
                            "• Position camera above sample\n" +
                            "• Capture image";
                break;
            case ProcedureStep.FinishFieldNote:
                instruction = "Step " + stepNumber + ": Finish Field Note\n\n" +
                            "• Edit all required fields and sliders\n" + 
                            "• Check fieldnote Significance Indicator (!)\n" +
                            "• Collect sample if indicator (!) is yellow\n" +
                            "• Press Complete button to save fieldnote";
                break;
            case ProcedureStep.Complete:
                instruction = "Step " + stepNumber + ": Complete Process\n\n" +
                            "• Drop pin on sample\n" +
                            "• Announce over comms:\n" +
                            " - Sampling complete, PR verify receipt of data\n" + 
                            " - Beginning nav to next location\n";
                break;
            case ProcedureStep.Completed:
                instruction = "Procedure Completed!\n\n" +
                            "• All data saved\n" +
                            "• Recording stored";
                break;
        }
        
        // Update the text in the step box
        if (stepInstructionText != null)
        {
            stepInstructionText.text = instruction;
        }
    }
}