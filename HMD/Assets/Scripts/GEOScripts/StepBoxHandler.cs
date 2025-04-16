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
    
    private void UpdateStepInstructions(ProcedureStep step)
    {
        string stepNumber = ((int)step + 1).ToString();
        string instruction = "";
        
        switch (step)
        {
            case ProcedureStep.StartVoiceRecording:
                instruction = "Step " + stepNumber + ": Start Voice Recording\n\n" +
                            "• Press the voice recording button on task list\n" + 
                            "• Voice all thoughts out loud";
                break;
            case ProcedureStep.SetupFieldNote:
                instruction = "Step " + stepNumber + ": Set Up Field Note\n\n" +
                            "• Open the field note\n" +
                            "• Turn on the edit toggle switch";
                break;
            case ProcedureStep.ScanSample:
                instruction = "Step " + stepNumber + ": Scan the Sample\n\n" +
                            "• Inspect the rock visually\n" +
                            "• Press and hold trigger to start XRF scan\n" +
                            "• Hold steady until confirmation beep\n" +
                            "• Release trigger";
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
                            "• Complete all required fields\n" +
                            "• Adjust property sliders";
                break;
            case ProcedureStep.Complete:
                instruction = "Step " + stepNumber + ": Complete Process\n\n" +
                            "• Press complete button on field note\n" +
                            "• System will store data and stop recording";
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