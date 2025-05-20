using UnityEngine;
using TMPro;
using System.Collections;

public class EgressTaskController : MonoBehaviour
{

    [Header("UIA Panel Reference")]
    public UIAPanelImage uiaPanelController;

    [Header("TSS Data Handlers")]

    // Access TSS Data through other scripts
    public TELEMETRYDataHandler telemetryDataHandler;
    public DataRanges dataRanges;
    public UIADataHandler uiaDataHandler;
    public DCUDataHandler dcuDataHandler;

    [Header("EVA Number Handler")]
    public EVANumberHandler evaNumberHandler; // new procedure only needs 1 eva stuff to be displayed

    [Header("Current Step Progress Bar Game Objects")]
    // Progress Bar Game Objects
    // new procedure only needs 1 eva stuff to be displayed
    public Transform evaBackground;
    public Transform evaForeground;
    public TextMeshPro evaProgressTextMeshPro;

    [Header("Overall Progress Bar Game Objects")]
    // Overall Progress Bar Game Object
    public Transform overallBackgroundBar;
    public Transform overallForegroundBar;
    public TextMeshPro overallProgressTextMeshPro;

    [Header("Task Panel Misc Objects")]
    // Progress Text Game Objects
    public TextMeshPro taskStatusTextMeshPro;

    // Task Detail Game Objects
    public TextMeshPro stepTitleTextMeshPro;
    public TextMeshPro taskTextMeshPro;

    // Switch Location Game Object (This tells user where is the switch, either
    // UIA or DCU)
    public TextMeshPro SwitchLocationText;
    
    [Header("Progress Colors for text and overlays")]
    public Color completedColor = Color.green;
    public Color incompleteColor = Color.red;
    public Color inProgressColor = Color.yellow;


    // let's define eva num here for later declaration
    private int evaNumber;
    private string evaIDString; // will be "eva1" or "eva2" depending on if ev1 or ev2 is selected at boot



    // Indices for UIAPanelImage overlays array
        // PWREV1 (0)
        // PWREV2 (1)
        // O2EV1 (2)
        // O2EV2 (3)
        // O2Vent (4)
        // H2OWasteEV1 (5)
        // H2OWasteEV2 (6)
        // DepressPumpPWR (7)
        // H2OSupplyEV1 (8)
        // H2OSupplyEV2 (9)
    private int overlayEMUPowerIndex;
    private int overlayEMUOxygenIndex;
    private const int OVERLAY_O2_VENT_INDEX = 4; // since this don't matter regarding eva number
    private const int OVERLAY_DEPRESS_PUMP_INDEX = 7; // since this don't matter regarding eva number

    // thank you Elgoog Magic Gerald Mini (gemini) for this formating :folding_hands:
    private string[] tasks = new string[]
    {
        // Task Group 1: Connect UIA to DCU and start Depress
        "Connect UIA to DCU and start Depress\nUIA: Connect UIA to DCU and start Depress", // Task index 0
        "Connect UIA to DCU and start Depress\nUIA: EV# verify umbilical connection from UIA to DCU", // Task index 1 (EV# will be replaced)
        "Connect UIA to DCU and start Depress\nUIA: EV-# EMU PWR – ON", // Task index 2 (EV-# will be replaced)
        "Connect UIA to DCU and start Depress\nDCU: BATT – UMB", // Task index 3
        "Connect UIA to DCU and start Depress\nUIA: DEPRESS PUMP PWR – ON", // Task index 4

        // Task Group 2: Prep O2 Tanks
        "Prep O2 Tanks\nUIA: OXYGEN O2 VENT – OPEN", // Task index 5
        "Prep O2 Tanks\nHMD: Wait until both Primary and Secondary OXY tanks are < 10 psi", // Task index 6
        "Prep O2 Tanks\nUIA: OXYGEN O2 VENT – CLOSE", // Task index 7
        "Prep O2 Tanks\nDCU: OXY – PRI", // Task index 8
        "Prep O2 Tanks\nUIA: OXYGEN EMU-# – OPEN", // Task index 9 (EMU-# will be replaced)
        "Prep O2 Tanks\nHMD: Wait until EV# Primary O2 tank > 3000 psi", // Task index 10 (EV# will be replaced)
        "Prep O2 Tanks\nUIA: OXYGEN EMU-# – CLOSE", // Task index 11 (EMU-# will be replaced)
        "Prep O2 Tanks\nDCU: OXY – SEC", // Task index 12
        "Prep O2 Tanks\nUIA: OXYGEN EMU-# – OPEN", // Task index 13 (EMU-# will be replaced)
        "Prep O2 Tanks\nHMD: Wait until EV# Secondary O2 tank > 3000 psi", // Task index 14 (EV# will be replaced)
        "Prep O2 Tanks\nUIA: OXYGEN EMU-# – CLOSE", // Task index 15 (EMU-# will be replaced)
        "Prep O2 Tanks\nDCU: OXY – PRI", // Task index 16

        // Task Group 3: END Depress, Check Switches and Disconnect
        "END Depress, Check Switches and Disconnect\nHMD: Wait until SUIT PRESSURE and O2 Pressure = 4", // Task index 17
        "END Depress, Check Switches and Disconnect\nUIA: DEPRESS PUMP PWR – OFF", // Task index 18
        "END Depress, Check Switches and Disconnect\nDCU: BATT – LOCAL", // Task index 19
        "END Depress, Check Switches and Disconnect\nUIA: EV-# EMU PWR - OFF", // Task index 20 (EV-# will be replaced)
        "END Depress, Check Switches and Disconnect\nDCU: Verify OXY – PRI", // Task index 21
        "END Depress, Check Switches and Disconnect\nDCU: Verify COMMS – A", // Task index 22
        "END Depress, Check Switches and Disconnect\nDCU: Verify FAN – PRI", // Task index 23
        "END Depress, Check Switches and Disconnect\nDCU: Verify PUMP – CLOSE", // Task index 24
        "END Depress, Check Switches and Disconnect\nDCU: Verify CO2 – A", // Task index 25
        "END Depress, Check Switches and Disconnect\nUIA and DCU: EV# disconnect UIA and DCU umbilical", // Task index 26 (EV# will be replaced)
        "Procedure Complete\nExit the pod, follow egress path, and maintain communication. Stay safe!" // Task index 27
    };
    // index used for egress progress
    private int currentTaskIndex = 0;

    private string GetColorName(bool conditionMet, bool isVerification = false)
    {
        if (isVerification)
        {
            return conditionMet ? "green" : "red"; // For verify steps, red if not met
        }
        return conditionMet ? "green" : "red"; // Standard: green if complete, red if not
    }
    private Color GetProgressColor(bool conditionMet, bool inProgressState = false)
    {
        if (conditionMet) return completedColor;
        if (inProgressState) return inProgressColor;
        return incompleteColor;
    }

    private void Start()
    {
        // redone the way we get the EVA number bc we only need to display one EVA
        if (evaNumberHandler == null)
        {
            Debug.LogError("EgressTaskController: EVANumberHandler is not assigned!");
            // default to ev1, if you get this then assign in unity bro
            evaNumber = 1;
        }
        else
        {
            evaNumber = evaNumberHandler.getEVANumber();
        }
        evaIDString = "eva" + evaNumber;

        // Set overlay indices based on evaNumber
        // Asked Elgoog Magic Gerald Mini (gemini 2.5 pro) for 
        // help on this part to simplify the logic
        // and i trust it with my life
        overlayEMUPowerIndex = evaNumber - 1; // 0 for EV1 (PWREV1), 1 for EV2 (PWREV2)
        overlayEMUOxygenIndex = evaNumber + 1; // 2 for EV1 (O2EV1), 3 for EV2 (O2EV2)
        // Initialize task text with correct EV number
        FormatTaskStrings();
        currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex(); // Ensure we start at the correct task if reloaded
        if(currentTaskIndex >= tasks.Length) // Safety check if TaskManager holds an out-of-bounds index
        {
            TaskManager.Instance.SetCurrentTaskIndex(0);
            currentTaskIndex = 0;
        }

        UpdateTaskText();
        UpdateOverallProgressBar();
        UpdateOverallProgressText();

        // ooooo Elgoog Magic Gerald Mini (gemini 2.5 pro) suggested this v nice
        if (uiaPanelController != null)
        {
            uiaPanelController.PositionPanelToLeftOfUser();
        }
        else
        {
            Debug.LogError("EgressTaskController: UIAPanelController is not assigned!");
        }

        // Should hopefully fix array OOB error that happens when
        // Switching from complete Egress (max step index=30) to new Ingress (max step index=12)
        // 30 > 12 -> array out of bounds error!
        currentTaskIndex = 0;
    }
    private void FormatTaskStrings()
    {
        // Format the task strings to include the EVA number
        // Replace "EV#" and "EV-#" with the EVA number
        // Replace "EMU-#" with the EVA number
        // thank you Elgoog Magic Gerald Mini 
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = tasks[i].Replace("EV#", "EV" + evaNumber).Replace("EV-#", "EV-" + evaNumber).Replace("EMU-#", "EMU-" + evaNumber);
        }
    }

    private void Update()
    {
        // someone cooked here... and it was Elgoog Magic Gerald Mini....
        // man i wwonder how future me will look back at this code when 
        // i have to do the redesign
        
        // if you're reading this hi (react :wave: on ts commit bro)

        // Ensure data handlers are available
        if (telemetryDataHandler == null || uiaDataHandler == null || dcuDataHandler == null)
        {
            // Optionally, display an error to the user or log, then return
            if (taskStatusTextMeshPro != null)
            {
                taskStatusTextMeshPro.text = "<color=red>Error: Data Handlers not assigned.</color>";
                taskStatusTextMeshPro.gameObject.SetActive(true);
            }
            Debug.LogError("EgressTaskController: One or more data handlers are not assigned in the inspector!");
            return;
        }
        UpdateProgressBar();
    }


    // Update the Overall Egress Progress Bar + Text
    private void UpdateOverallProgressBar()
    {
        int currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
        int totalTasks = tasks.Length;
        if (totalTasks == 0) return; // divide by zero check 💔 this was causing the infinite progres bar glitch

        float progress = (float)currentTaskIndex / (totalTasks - 1); // -1 because index is 0-based
        if (currentTaskIndex == totalTasks - 1) progress = 1f; // Ensure 100% on last step selected

        Vector3 backgroundScale = overallBackgroundBar.localScale;
        Vector3 foregroundScale = overallForegroundBar.localScale;

        foregroundScale.x = backgroundScale.x * progress;
        overallForegroundBar.localScale = foregroundScale;

        Vector3 foregroundPosition = overallForegroundBar.localPosition;
        Vector3 backgroundPosition = overallBackgroundBar.localPosition;
        float leftPivot = backgroundPosition.x - backgroundScale.x * 0.5f;
        foregroundPosition.x = leftPivot + foregroundScale.x * 0.5f;
        overallForegroundBar.localPosition = foregroundPosition;
    }

    private void UpdateOverallProgressText()
    {
        int currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
        int totalTasks = tasks.Length;

        overallProgressTextMeshPro.text = $"Egress: {currentTaskIndex + 1}/{totalTasks} Steps";
    }

    // Update the individual step's progress bar + text
    
    // WARNING: this is a super long function bc its a whole procedure
    // and i don't want to break it up into multiple functions
    // and Elgoog Magic Gerald Mini (gemini 2.5 pro) helped with it so i trust it works


    // after code read through: hhhooollly shit Gerald made it simpler 
    // 
    // and it works!!


    private void UpdateProgressBar()
    {
        currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex(); // Get current task from manager
        UpdateSwitchLocationText(currentTaskIndex);
        bool taskCompleted = false; // General flag for simple switch tasks
        Color currentOverlayColor = incompleteColor; // Default overlay color

        // Ensure uiaPanelController is available before trying to use it
        if (uiaPanelController == null)
        {
            Debug.LogError("UIAPanelController is not assigned in EgressTaskController!");
            taskStatusTextMeshPro.text = "<color=red>UIA Panel Error</color>";
            taskStatusTextMeshPro.gameObject.SetActive(true);
            return;
        }

        ResetProgressBarAndText(); // Reset individual progress bar and status text visibility first
        uiaPanelController.DeactivateAllOverlays(); // Deactivate all overlays by default

        switch (currentTaskIndex)
        {
            // --- Task Group 1: Connect UIA to DCU and start Depress ---
            case 0: // UIA: Connect UIA to DCU and start Depress (Assumed)
                taskCompleted = true; // Assumed complete
                taskStatusTextMeshPro.text = "Manually verify umbilical connection of UIA to DCU";
                currentOverlayColor = completedColor;
                break;

            case 1: // UIA: EV# verify umbilical connection from UIA to DCU (Assumed)
                taskCompleted = true; // Assumed complete
                taskStatusTextMeshPro.text = $"EV{evaNumber} umbilical manually verified";
                currentOverlayColor = completedColor;
                break;

            case 2: // UIA: EV-# EMU PWR – ON
                uiaPanelController.SetOverlayState(overlayEMUPowerIndex, true);
                bool isEvaPowerOn = uiaDataHandler.GetPower(evaIDString); // true if ON
                taskCompleted = isEvaPowerOn;
                currentOverlayColor = GetProgressColor(taskCompleted);
                uiaPanelController.ChangeToCustomColor(overlayEMUPowerIndex, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>EV-{evaNumber} EMU PWR: {(isEvaPowerOn ? "ON" : "OFF")}</color>";
                break;

            case 3: // DCU: BATT – UMB
                bool isBattUmb = dcuDataHandler.GetBatt(evaIDString); // true if UMB
                taskCompleted = isBattUmb;
                currentOverlayColor = GetProgressColor(taskCompleted);
                // No UIA overlay for DCU tasks, but status text is important
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>DCU BATT: {(isBattUmb ? "UMB" : "LOCAL")}</color>";
                break;

            case 4: // UIA: DEPRESS PUMP PWR – ON
                uiaPanelController.SetOverlayState(OVERLAY_DEPRESS_PUMP_INDEX, true);
                bool isDepressPumpOn = uiaDataHandler.GetDepress(); // true if ON
                taskCompleted = isDepressPumpOn;
                currentOverlayColor = GetProgressColor(taskCompleted);
                uiaPanelController.ChangeToCustomColor(OVERLAY_DEPRESS_PUMP_INDEX, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>DEPRESS PUMP PWR: {(isDepressPumpOn ? "ON" : "OFF")}</color>";
                break;

            // --- Task Group 2: Prep O2 Tanks ---
            case 5: // UIA: OXYGEN O2 VENT – OPEN
                uiaPanelController.SetOverlayState(OVERLAY_O2_VENT_INDEX, true);
                bool isO2VentOpen = uiaDataHandler.GetOxy_Vent(); // true if OPEN
                taskCompleted = isO2VentOpen;
                currentOverlayColor = GetProgressColor(taskCompleted, taskCompleted); // Yellow if open (in progress for next step)
                if (taskCompleted) currentOverlayColor = inProgressColor; // Explicitly yellow if open, as next step is wait
                else currentOverlayColor = incompleteColor;
                uiaPanelController.ChangeToCustomColor(OVERLAY_O2_VENT_INDEX, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>OXYGEN O2 VENT: {(isO2VentOpen ? "OPEN" : "CLOSED")}</color>";
                break;

            case 6: // HMD: Wait until both Primary and Secondary OXY tanks are < 10 psi
                ActivateProgressBar();
                uiaPanelController.SetOverlayState(OVERLAY_O2_VENT_INDEX, true); // Keep O2 Vent overlay active
                float priOxyPressure = telemetryDataHandler.GetOxyPriPressure(evaIDString);
                float secOxyPressure = telemetryDataHandler.GetOxySecPressure(evaIDString);
                float maxOxyPressure = Mathf.Max(priOxyPressure, secOxyPressure);
                taskCompleted = maxOxyPressure < 10f;

                float progressWaitOxyLow = 0f;
                if (taskCompleted)
                {
                    progressWaitOxyLow = 1f;
                    currentOverlayColor = completedColor;
                }
                else
                {
                    // Example progress: 0 if current pressure is at max (e.g. 3000), 1 if at 10.
                    // This assumes dataRanges.oxy_pri_pressure.Max is a relevant upper bound.
                    float initialMaxPressure = (float)dataRanges.oxy_pri_pressure.Max; // Or a more relevant starting high pressure
                    if (initialMaxPressure <= 10f) initialMaxPressure = 3000f; // Ensure non-zero range
                    progressWaitOxyLow = Mathf.Clamp01((initialMaxPressure - maxOxyPressure) / (initialMaxPressure - 10f));
                    currentOverlayColor = GetProgressColor(taskCompleted, maxOxyPressure < initialMaxPressure); // Yellow if decreasing
                }
                UpdateEvaProgressBar(progressWaitOxyLow, currentOverlayColor);
                evaProgressTextMeshPro.text = $"EVA: {maxOxyPressure:F0}psi (Current) < 10psi (Goal)";
                uiaPanelController.ChangeToCustomColor(OVERLAY_O2_VENT_INDEX, currentOverlayColor);
                break;

            case 7: // UIA: OXYGEN O2 VENT – CLOSE
                uiaPanelController.SetOverlayState(OVERLAY_O2_VENT_INDEX, true);
                bool isO2VentClosed = !uiaDataHandler.GetOxy_Vent(); // true if CLOSED
                taskCompleted = isO2VentClosed;
                currentOverlayColor = GetProgressColor(taskCompleted);
                uiaPanelController.ChangeToCustomColor(OVERLAY_O2_VENT_INDEX, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>OXYGEN O2 VENT: {(isO2VentClosed ? "CLOSED" : "OPEN")}</color>";
                break;

            case 8: // DCU: OXY – PRI
                bool isDcuOxyPri = dcuDataHandler.GetOxy(evaIDString); // true if PRI
                taskCompleted = isDcuOxyPri;
                currentOverlayColor = GetProgressColor(taskCompleted);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>DCU OXY: {(isDcuOxyPri ? "PRI" : "SEC")}</color>";
                break;

            case 9: // UIA: OXYGEN EMU-# – OPEN
                uiaPanelController.SetOverlayState(overlayEMUOxygenIndex, true);
                bool isEmuOxyOpen = uiaDataHandler.GetOxy(evaIDString); // UIA's switch for EVA's oxygen line, true if OPEN
                taskCompleted = isEmuOxyOpen;
                if (taskCompleted) currentOverlayColor = inProgressColor; // Yellow, next step is wait
                else currentOverlayColor = incompleteColor;
                uiaPanelController.ChangeToCustomColor(overlayEMUOxygenIndex, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>OXYGEN EV-{evaNumber}: {(isEmuOxyOpen ? "OPEN" : "CLOSED")}</color>";
                break;

            case 10: // HMD: Wait until EV# Primary O2 tank > 3000 psi
                ActivateProgressBar();
                uiaPanelController.SetOverlayState(overlayEMUOxygenIndex, true); // Keep EMU Oxy overlay active
                float priOxyTankPressure = telemetryDataHandler.GetOxyPriPressure(evaIDString);
                // reduced this threshold to 2990 psi because sometimes tss isnt exactly at 3000 (but its like at 2998 bro just mark as complete) 
                taskCompleted = priOxyTankPressure >= 2990f;

                float progressPriOxyHigh = Mathf.Clamp01(priOxyTankPressure / 3000f);
                currentOverlayColor = GetProgressColor(taskCompleted, priOxyTankPressure > 0 && !taskCompleted);

                UpdateEvaProgressBar(progressPriOxyHigh, currentOverlayColor);
                evaProgressTextMeshPro.text = $"EVA Pri O2: {priOxyTankPressure:F0}psi / 3000psi (Goal)";
                uiaPanelController.ChangeToCustomColor(overlayEMUOxygenIndex, currentOverlayColor);
                break;

            case 11: // UIA: OXYGEN EMU-# – CLOSE
                uiaPanelController.SetOverlayState(overlayEMUOxygenIndex, true);
                bool isEmuOxyClosed = !uiaDataHandler.GetOxy(evaIDString); // true if CLOSED
                taskCompleted = isEmuOxyClosed;
                currentOverlayColor = GetProgressColor(taskCompleted);
                uiaPanelController.ChangeToCustomColor(overlayEMUOxygenIndex, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>OXYGEN EV-{evaNumber}: {(isEmuOxyClosed ? "CLOSED" : "OPEN")}</color>";
                break;

            case 12: // DCU: OXY – SEC
                bool isDcuOxySec = !dcuDataHandler.GetOxy(evaIDString); // true if SEC
                taskCompleted = isDcuOxySec;
                currentOverlayColor = GetProgressColor(taskCompleted);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>DCU OXY: {(isDcuOxySec ? "SEC" : "PRI")}</color>";
                break;

            case 13: // UIA: OXYGEN EMU-# – OPEN (again for secondary fill)
                uiaPanelController.SetOverlayState(overlayEMUOxygenIndex, true);
                isEmuOxyOpen = uiaDataHandler.GetOxy(evaIDString); // true if OPEN
                taskCompleted = isEmuOxyOpen;
                if (taskCompleted) currentOverlayColor = inProgressColor; // Yellow, next step is wait
                else currentOverlayColor = incompleteColor;
                uiaPanelController.ChangeToCustomColor(overlayEMUOxygenIndex, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>OXYGEN EV-{evaNumber}: {(isEmuOxyOpen ? "OPEN" : "CLOSED")}</color>";
                break;

            case 14: // HMD: Wait until EV# Secondary O2 tank > 3000 psi
                ActivateProgressBar();
                uiaPanelController.SetOverlayState(overlayEMUOxygenIndex, true); // Keep EMU Oxy overlay active
                float secOxyTankPressure = telemetryDataHandler.GetOxySecPressure(evaIDString);
                taskCompleted = secOxyTankPressure >= 3000f;

                float progressSecOxyHigh = Mathf.Clamp01(secOxyTankPressure / 3000f);
                currentOverlayColor = GetProgressColor(taskCompleted, secOxyTankPressure > 0 && !taskCompleted);

                UpdateEvaProgressBar(progressSecOxyHigh, currentOverlayColor);
                evaProgressTextMeshPro.text = $"EVA Sec O2: {secOxyTankPressure:F0}psi / 3000psi (Goal)";
                uiaPanelController.ChangeToCustomColor(overlayEMUOxygenIndex, currentOverlayColor);
                break;

            case 15: // UIA: OXYGEN EMU-# – CLOSE (after secondary fill)
                uiaPanelController.SetOverlayState(overlayEMUOxygenIndex, true);
                isEmuOxyClosed = !uiaDataHandler.GetOxy(evaIDString); // true if CLOSED
                taskCompleted = isEmuOxyClosed;
                currentOverlayColor = GetProgressColor(taskCompleted);
                uiaPanelController.ChangeToCustomColor(overlayEMUOxygenIndex, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>OXYGEN EMU-{evaNumber}: {(isEmuOxyClosed ? "CLOSED" : "OPEN")}</color>";
                break;

            case 16: // DCU: OXY – PRI (final set)
                isDcuOxyPri = dcuDataHandler.GetOxy(evaIDString); // true if PRI
                taskCompleted = isDcuOxyPri;
                currentOverlayColor = GetProgressColor(taskCompleted);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>DCU OXY: {(isDcuOxyPri ? "PRI" : "SEC")}</color>";
                break;

            // --- Task Group 3: END Depress, Check Switches and Disconnect ---
            case 17: // HMD: Wait until SUIT PRESSURE and O2 Pressure = 4
                ActivateProgressBar();
                // DEPRESS PUMP PWR should still be ON, so keep its overlay 
                uiaPanelController.SetOverlayState(OVERLAY_DEPRESS_PUMP_INDEX, true); 
                uiaPanelController.ChangeToCustomColor(OVERLAY_DEPRESS_PUMP_INDEX, inProgressColor);


                float suitPressureOxy = telemetryDataHandler.GetSuitPressureOxy(evaIDString);
                taskCompleted = Mathf.Approximately(suitPressureOxy, 4f) || suitPressureOxy > 4f; // Allow slightly over

                float progressSuitPressure = Mathf.Clamp01(suitPressureOxy / 4f);
                currentOverlayColor = GetProgressColor(taskCompleted, suitPressureOxy > 0 && !taskCompleted);

                UpdateEvaProgressBar(progressSuitPressure, currentOverlayColor);
                evaProgressTextMeshPro.text = $"EVA Suit/O2 Pressure: {suitPressureOxy:F1}psi / 4.0psi (Goal)";
                uiaPanelController.ChangeToCustomColor(OVERLAY_DEPRESS_PUMP_INDEX, currentOverlayColor);
                break;

            case 18: // UIA: DEPRESS PUMP PWR – OFF
                uiaPanelController.SetOverlayState(OVERLAY_DEPRESS_PUMP_INDEX, true);
                bool isDepressPumpOff = !uiaDataHandler.GetDepress(); // true if OFF
                taskCompleted = isDepressPumpOff;
                currentOverlayColor = GetProgressColor(taskCompleted);
                uiaPanelController.ChangeToCustomColor(OVERLAY_DEPRESS_PUMP_INDEX, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>DEPRESS PUMP PWR: {(isDepressPumpOff ? "OFF" : "ON")}</color>";
                break;

            case 19: // DCU: BATT – LOCAL
                bool isBattLocal = !dcuDataHandler.GetBatt(evaIDString); // true if LOCAL
                taskCompleted = isBattLocal;
                currentOverlayColor = GetProgressColor(taskCompleted);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>DCU BATT: {(isBattLocal ? "LOCAL" : "UMB")}</color>";
                break;

            case 20: // UIA: EV-# EMU PWR - OFF
                uiaPanelController.SetOverlayState(overlayEMUPowerIndex, true);
                bool isEvaPowerOff = !uiaDataHandler.GetPower(evaIDString); // true if OFF
                taskCompleted = isEvaPowerOff;
                currentOverlayColor = GetProgressColor(taskCompleted);
                uiaPanelController.ChangeToCustomColor(overlayEMUPowerIndex, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>EV-{evaNumber} EMU PWR: {(isEvaPowerOff ? "OFF" : "ON")}</color>";
                break;

            case 21: // DCU: Verify OXY – PRI
                bool verifyOxyPri = dcuDataHandler.GetOxy(evaIDString); // true if PRI
                taskCompleted = verifyOxyPri;
                currentOverlayColor = GetProgressColor(taskCompleted);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted, true)}>Verify DCU OXY: {(verifyOxyPri ? "PRI" : "SEC")}</color>";
                break;

            case 22: // DCU: Verify COMMS – A
                bool verifyCommsA = dcuDataHandler.GetComm(evaIDString); // true if A
                taskCompleted = verifyCommsA;
                currentOverlayColor = GetProgressColor(taskCompleted);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted, true)}>Verify DCU COMMS: {(verifyCommsA ? "A" : "B")}</color>";
                break;

            case 23: // DCU: Verify FAN – PRI
                bool verifyFanPri = dcuDataHandler.GetFan(evaIDString); // true if PRI
                taskCompleted = verifyFanPri;
                currentOverlayColor = GetProgressColor(taskCompleted);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted, true)}>Verify DCU FAN: {(verifyFanPri ? "PRI" : "SEC")}</color>";
                break;

            case 24: // DCU: Verify PUMP – CLOSE
                bool verifyPumpClose = !dcuDataHandler.GetPump(evaIDString); // true if CLOSE
                taskCompleted = verifyPumpClose;
                currentOverlayColor = GetProgressColor(taskCompleted);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted, true)}>Verify DCU PUMP: {(verifyPumpClose ? "CLOSE" : "OPEN")}</color>";
                break;

            case 25: // DCU: Verify CO2 – A
                bool verifyCO2A = dcuDataHandler.GetCO2(evaIDString); // true if A
                taskCompleted = verifyCO2A;
                currentOverlayColor = GetProgressColor(taskCompleted);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted, true)}>Verify DCU CO2: {(verifyCO2A ? "A" : "B")}</color>";
                break;

            case 26: // UIA and DCU: EV# disconnect UIA and DCU umbilical (Assumed)
                taskCompleted = true; // Assumed complete
                taskStatusTextMeshPro.text = $"EV{evaNumber} disconnect UIA and DCU umbilical (Manually verify)";
                currentOverlayColor = completedColor;
                break;

            case 27: // Procedure Complete
                taskCompleted = true; // Final step is just text
                currentOverlayColor = completedColor;
                taskStatusTextMeshPro.text = "Egress Complete! Confirm with PR team that comms are working.";
                break;

            default:
                ResetProgressBarAndText();
                uiaPanelController.DeactivateAllOverlays();
                taskStatusTextMeshPro.text = "Unknown Task Index.";
                currentOverlayColor = incompleteColor;
                break;
        }

        // Set visibility of status text if it has content
        if (!string.IsNullOrEmpty(taskStatusTextMeshPro.text))
        {
            taskStatusTextMeshPro.gameObject.SetActive(true);
            // General color for the text block, specific parts are colored with rich text
            taskStatusTextMeshPro.color = taskCompleted ? completedColor : incompleteColor;
            if (IsWaitingTask(currentTaskIndex) && !taskCompleted)
            {
                bool inProg = false; // Determine if actually in progress for waiting tasks
                if (currentTaskIndex == 6)
                { // OXY tanks < 10 psi
                    float pri = telemetryDataHandler.GetOxyPriPressure(evaIDString);
                    float sec = telemetryDataHandler.GetOxySecPressure(evaIDString);
                    inProg = Mathf.Max(pri, sec) < (float)dataRanges.oxy_pri_pressure.Max; // In progress if not at max
                }
                else if (currentTaskIndex == 10)
                { // Pri O2 > 3000
                    inProg = telemetryDataHandler.GetOxyPriPressure(evaIDString) > 0;
                }
                else if (currentTaskIndex == 14)
                { // Sec O2 > 3000
                    inProg = telemetryDataHandler.GetOxySecPressure(evaIDString) > 0;
                }
                else if (currentTaskIndex == 17)
                { // Suit Pressure = 4
                    inProg = telemetryDataHandler.GetSuitPressureOxy(evaIDString) > 0;
                }
                if (inProg) taskStatusTextMeshPro.color = inProgressColor;
            }
        }
        else
        {
            taskStatusTextMeshPro.gameObject.SetActive(false);
        }
    }



    private bool IsWaitingTask(int taskIndex) {
        return taskIndex == 6 || taskIndex == 10 || taskIndex == 14 || taskIndex == 17;
    }
    
    // replaces UpdateProgressBarSize for new ev requirements
    private void UpdateEvaProgressBar(float progress, Color barColor)
    {
        Vector3 backgroundScale = evaBackground.localScale;
        Vector3 foregroundScale = evaForeground.localScale;

        foregroundScale.x = backgroundScale.x * progress;
        evaForeground.localScale = foregroundScale;

        Vector3 foregroundPosition = evaForeground.localPosition;
        Vector3 backgroundPosition = evaBackground.localPosition;
        float leftPivot = backgroundPosition.x - backgroundScale.x * 0.5f;
        foregroundPosition.x = leftPivot + foregroundScale.x * 0.5f;
        evaForeground.localPosition = foregroundPosition;

        if (evaForeground.GetComponent<Renderer>() != null)
        {
            evaForeground.GetComponent<Renderer>().material.color = barColor;
        }
    }

    private void ActivateProgressBar()
    {
        evaBackground.gameObject.SetActive(true);
        evaForeground.gameObject.SetActive(true);
        evaProgressTextMeshPro.gameObject.SetActive(true);
    }

    private void ResetProgressBarAndText()
    {
        evaBackground.gameObject.SetActive(false);
        evaForeground.gameObject.SetActive(false);
        evaProgressTextMeshPro.gameObject.SetActive(false);
        evaProgressTextMeshPro.text = string.Empty;

        taskStatusTextMeshPro.gameObject.SetActive(false);
        taskStatusTextMeshPro.text = string.Empty;
    }

    public void GoForward()
    {
        int taskIdx = TaskManager.Instance.GetCurrentTaskIndex();
        if (taskIdx < tasks.Length - 1)
        {
            TaskManager.Instance.SetCurrentTaskIndex(taskIdx + 1);
            UpdateTaskText();
            UpdateOverallProgressBar();
            UpdateOverallProgressText();
        }
    }

    public void GoBack()
    {
        int taskIdx = TaskManager.Instance.GetCurrentTaskIndex();
        if (taskIdx > 0)
        {
            TaskManager.Instance.SetCurrentTaskIndex(taskIdx - 1);
            UpdateTaskText();
            UpdateOverallProgressBar();
            UpdateOverallProgressText();
        }
    }

    private void UpdateTaskText()
    {
        int taskIdx = TaskManager.Instance.GetCurrentTaskIndex();
        if (taskIdx >= 0 && taskIdx < tasks.Length)
        {
            string currentTaskString = tasks[taskIdx];
            string[] lines = currentTaskString.Split('\n');
            stepTitleTextMeshPro.text = lines[0]; // The "Header" part
            if (lines.Length > 1)
            {
                taskTextMeshPro.text = string.Join("\n", lines, 1, lines.Length - 1); // The "SUBTASK LOCATION: DESCRIPTION"
            }
            else
            {
                taskTextMeshPro.text = ""; // No sub-description
            }
        }
    }

    private void UpdateSwitchLocationText(int taskIndex)
    {
        if (taskIndex >= 0 && taskIndex < tasks.Length)
        {
            string currentTaskFullString = tasks[taskIndex];
            // The location is in the second line (subtask line) before the colon
            string[] lines = currentTaskFullString.Split('\n');
            if (lines.Length > 1)
            {
                string subtaskLine = lines[1];
                if (subtaskLine.Contains(":"))
                {
                    string location = subtaskLine.Substring(0, subtaskLine.IndexOf(':')).Trim();
                     // Handle "UIA and DCU" case specifically for multi-line
                    if (location.Equals("UIA and DCU", System.StringComparison.OrdinalIgnoreCase))
                    {
                        SwitchLocationText.text = "UIA\nDCU";
                    }
                    else if (location.Equals("HMD", System.StringComparison.OrdinalIgnoreCase))
                    {
                        SwitchLocationText.text = ""; // As per request for HMD tasks
                    }
                    else
                    {
                        SwitchLocationText.text = location;
                    }
                }
                else
                {
                    SwitchLocationText.text = ""; // No colon, can't determine location
                }
            }
            else
            {
                SwitchLocationText.text = ""; // Only header line
            }
        }
        else
        {
            SwitchLocationText.text = "";
        }
    }
}