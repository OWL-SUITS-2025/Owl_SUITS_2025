using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class EgressProcedureController : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro instructionsText;

    [SerializeField]
    private PressableButton nextButton;

    [SerializeField]
    private PressableButton backButton;

    private string[] egressSteps;
    private int currentStepIndex = 0;

    private void Awake()
    {
        // Initialize egress procedure steps from the document
        InitializeEgressSteps();
    }

    private void Start()
    {
        // Add listeners to the buttons
        nextButton.OnClicked.AddListener(OnNextButtonClicked);
        backButton.OnClicked.AddListener(OnBackButtonClicked);

        // Display the first step
        DisplayCurrentStep();
    }

    private void OnDestroy()
    {
        // Remove listeners when the script is destroyed
        if (nextButton != null)
            nextButton.OnClicked.RemoveListener(OnNextButtonClicked);
        
        if (backButton != null)
            backButton.OnClicked.RemoveListener(OnBackButtonClicked);
    }

    private void InitializeEgressSteps()
    {
        // Define the egress procedure steps from the document
        egressSteps = new string[]
        {
            // Connect UIA to DCU and start Depress
            "UIA and DCU 1. EV1 verify umbilical connection from UIA to DCU",
            "UIA 2. EV-1, EMU PWR -- ON",
            "DCU 3. BATT -- UMB",
            "UIA 4. DEPRESS PUMP PWR -- ON",
            
            // Prep O2 Tanks
            "UIA 1. OXYGEN O2 VENT -- OPEN",
            "HMD 2. Wait until both Primary and Secondary OXY tanks are < 10psi",
            "UIA 3. OXYGEN O2 VENT -- CLOSE",
            "DCU 4. OXY -- PRI",
            "UIA 5. OXYGEN EMU-1 -- OPEN",
            "HMD 6. Wait until EV1 Primary O2 tank > 3000 psi",
            "UIA 7. OXYGEN EMU-1 -- CLOSE",
            "DCU 8. OXY -- SEC",
            "UIA 9. OXYGEN EMU-1 -- OPEN",
            "HMD 10. Wait until EV1 Secondary O2 tank > 3000 psi",
            "UIA 11. OXYGEN EMU-1 -- CLOSE",
            "DCU 12. OXY -- PRI",
            
            // END Depress, Check Switches and Disconnect
            "HMD 1. Wait until SUIT PRESSURE and O2 Pressure = 4",
            "UIA 2. DEPRESS PUMP PWR -- OFF",
            "DCU 3. BATT -- LOCAL",
            "UIA 4. EV-1 EMU PWR - OFF",
            "DCU 5. Verify OXY -- PRI",
            "DCU 6. Verify COMMS -- A",
            "DCU 7. Verify FAN -- PRI",
            "DCU 8. Verify PUMP -- CLOSE",
            "DCU 9. Verify CO2 -- A",
            "UIA and DCU 10. EV1 disconnect UIA and DCU umbilical",
            "DCU Verify Comms are working between DCU and PR.",
            "\"EV1 to PR, comm check, can you hear me?\"",
            "PR respond appropriately."
        };
    }

    private void DisplayCurrentStep()
    {
        if (instructionsText != null && currentStepIndex >= 0 && currentStepIndex < egressSteps.Length)
        {
            instructionsText.text = egressSteps[currentStepIndex];
        }
    }

    private void OnNextButtonClicked()
    {
        if (currentStepIndex < egressSteps.Length - 1)
        {
            currentStepIndex++;
            DisplayCurrentStep();
        }
    }

    private void OnBackButtonClicked()
    {
        if (currentStepIndex > 0)
        {
            currentStepIndex--;
            DisplayCurrentStep();
        }
    }
}