using Unity.VisualScripting;


using UnityEngine;
using TMPro;
using System.Collections;

public class IngressTaskController : MonoBehaviour
{
    [Header("UIA Panel Reference")]
    public UIAPanelImage uiaPanelController;

    [Header("TSS Data Handlers")]
    // Access TSS Data through other scripts
    public TELEMETRYDataHandler telemetryDataHandler;
    public DataRanges dataRanges;
    public UIADataHandler uiaDataHandler;
    public DCUDataHandler dcuDataHandler;

    [Header("Progress Bar Game Objects")]
    // Progress Bar Game Objects
    public Transform ev1Background;
    public Transform ev1Foreground;
    public Transform ev2Background;
    public Transform ev2Foreground;
    public TextMeshPro ev1ProgressTextMeshPro;
    public TextMeshPro ev2ProgressTextMeshPro;

    // Progress Text Game Object
    public TextMeshPro taskStatusTextMeshPro;

    // Task Detail Game Objects
    public TextMeshPro stepTitleTextMeshPro;
    public TextMeshPro taskTextMeshPro;

    // Switch Location Game Object (This tells user where is the switch, either UIA or DCU)
    public TextMeshPro SwitchLocationText;

    [Header("Colors for text and overlays")]
    public Color completedColor = Color.green;
    public Color incompleteColor = Color.red;
    public Color inProgressColor = Color.yellow;

    private string[] tasks = new string[]
    {
        "Step 1: Connect UIA to DCU\nEV1 and EV2: connect UIA and DCU umbilical.",         // Task index 0
        "Step 2: Power UIA ON\nUIA: EV-1, EV-2 EMU PWR � ON.",         // Task index 1
        "Step 3: Configure DCU\nBOTH DCU: BATT � UMB.",         // Task index 2
        "Step 4: Vent O2 Tanks\nUIA: OXYGEN O2 VENT � OPEN.",         // Task index 3
        "Step 4: Vent O2 Tanks\nWait until both Primary and Secondary OXY tanks are < 10psi.",         // Task index 4
        "Step 4: Vent O2 Tanks\nUIA: OXYGEN O2 VENT � CLOSE.",         // Task index 5
        "Step 5: Empty Water Tanks\nBOTH DCU: PUMP � OPEN.",         // Task index 6
        "Step 5: Empty Water Tanks\nUIA: EV-1, EV-2 WASTE WATER � OPEN.",         // Task index 7
        "Step 5: Empty Water Tanks\nWait until EV1 and EV2 Coolant tanks < 5%.",         // Task index 8
        "Step 5: Empty Water Tanks\nUIA: EV-1, EV-2 WASTE WATER � CLOSE.",         // Task index 9
        "Step 6: Disconnect UIA from DCU\nUIA: EV-1, EV-2 EMU PWR � OFF.",         // Task index 10
        "Step 6: Disconnect UIA from DCU\nDCU: EV1 and EV2 disconnect umbilical.",         // Task index 11
        "End: Congratulations!\n You have completed the ingress task.",         // Task index 12
    };

    private int currentTaskIndex = 0;

    // Helper method to get the color name based on switch state
    private string GetColorName(bool switchState)
    {
        return switchState ? "green" : "red";
    }

    private void Start()
    {
        UpdateTaskText();
        uiaPanelController.PositionPanelToLeftOfUser();
        // Should hopefully fix array OOB error that happens when
        // Switching from co
        // mplete Egress (max step index=30) to new Ingress (max step index=12)
        // 30 > 12 -> array out of bounds error!
        currentTaskIndex = 0;
    }

    private void Update()
    {
        UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        int currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
        UpdateSwitchLocationText(currentTaskIndex);
        float progress = 0f;

        switch (currentTaskIndex)
        {
            case 0: // Step 1: Connect UIA to DCU
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Disable all overlays if we go backwards to 1st step (UIA Panel not needed here)
                uiaPanelController.DeactivateAllOverlays();

                // Check if EV-1 and EV-2 have connected UIA and DCU umbilical
                // You can add the condition here based on the umbilical connection status
                // For now, let's assume they are connected

                // Update the status text
                taskStatusTextMeshPro.gameObject.SetActive(true);
                taskStatusTextMeshPro.text = $"<color=green>EV-1 and EV-2: Connected UIA and DCU umbilical.</color>";
                // TODO implement UIA and DCU umb check from EgressTaskController.cs
                progress = 1f;
                break;

            case 1: // Step 2: Power UIA ON
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Deactivate all overlays, then activate power ev1 (index 0) and power ev2 (index 1)
                uiaPanelController.DeactivateAllOverlays();
                uiaPanelController.SetOverlayState(0, true); // Power EV1
                uiaPanelController.SetOverlayState(1, true); // Power EV2
                // Check if EV-1 and EV-2 EMU PWR switches are ON
                bool isEVA1PowerOn = uiaDataHandler.GetPower("eva1");
                bool isEVA2PowerOn = uiaDataHandler.GetPower("eva2");

                // Check and change overlay colors if the user has completed the task or not
                if (isEVA1PowerOn)
                {
                    uiaPanelController.ChangeToCustomColor(0, completedColor);
                }
                else
                {
                    uiaPanelController.ChangeToCustomColor(0, incompleteColor);
                }

                if (isEVA2PowerOn)
                {
                    uiaPanelController.ChangeToCustomColor(1, completedColor);
                }
                else
                {
                    uiaPanelController.ChangeToCustomColor(1, incompleteColor);
                }


                if (isEVA1PowerOn && isEVA2PowerOn)
                {
                    progress = 1f;

                }

                // Update the status text
                taskStatusTextMeshPro.gameObject.SetActive(true);
                taskStatusTextMeshPro.text = $"<color={GetColorName(isEVA1PowerOn)}>EV-1 EMU PWR: {(isEVA1PowerOn ? "ON" : "OFF")}</color>\n<color={GetColorName(isEVA2PowerOn)}>EV-2 EMU PWR: {(isEVA2PowerOn ? "ON" : "OFF")}</color>";
                break;

            case 2: // Step 3: Configure DCU
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Deactivate all overlays, UIA panel not needed here
                uiaPanelController.DeactivateAllOverlays();


                // Check if BATT switch is UMB on both DCUs
                bool isDCU1BattUMB = dcuDataHandler.GetBatt("eva1");
                bool isDCU2BattUMB = dcuDataHandler.GetBatt("eva2");

                if (isDCU1BattUMB && isDCU2BattUMB)
                {
                    progress = 1f;
                }

                // Update the status text
                taskStatusTextMeshPro.gameObject.SetActive(true);
                taskStatusTextMeshPro.text = $"<color={GetColorName(isDCU1BattUMB)}>DCU1 BATT: {(isDCU1BattUMB ? "UMB" : "LOCAL")}</color>\n<color={GetColorName(isDCU2BattUMB)}>DCU2 BATT: {(isDCU2BattUMB ? "UMB" : "LOCAL")}</color>";
                break;

            case 3: // Step 4: Vent O2 Tanks - UIA: OXYGEN O2 VENT � OPEN
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Deactivate all overlays, then activate O2 vent (index 4) 
                uiaPanelController.DeactivateAllOverlays();
                uiaPanelController.SetOverlayState(4, true); // O2 Vent

                // Check if OXYGEN O2 VENT switch is OPEN
                bool isOxygenO2VentOpen = uiaDataHandler.GetOxy_Vent();


                if (isOxygenO2VentOpen)
                {
                    uiaPanelController.ChangeToCustomColor(4, inProgressColor); // should stay yellow bc it will eventually hit green in next step
                    progress = 1f;
                }

                // Update the status text
                taskStatusTextMeshPro.gameObject.SetActive(true);
                taskStatusTextMeshPro.text = $"<color={GetColorName(isOxygenO2VentOpen)}>OXYGEN O2 VENT: {(isOxygenO2VentOpen ? "OPEN" : "CLOSED")}</color>";
                break;

            case 4: // Step 4: Vent O2 Tanks - Wait until both Primary and Secondary OXY tanks are < 10psi
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Deactivate all overlays, then activate O2 vent (index 4) 
                uiaPanelController.DeactivateAllOverlays();
                uiaPanelController.SetOverlayState(4, true); // O2 Vent

                // Activate the progress bar
                ActivateProgressBar();

                float eva1PrimaryPressure = telemetryDataHandler.GetOxyPriPressure("eva1");
                float eva1SecondaryPressure = telemetryDataHandler.GetOxySecPressure("eva1");
                float eva2PrimaryPressure = telemetryDataHandler.GetOxyPriPressure("eva2");
                float eva2SecondaryPressure = telemetryDataHandler.GetOxySecPressure("eva2");

                // We want to make sure both are less than 10psi, so we use the maximum pressure of the two tanks
                float eva1MaxPressure = Mathf.Max(eva1PrimaryPressure, eva1SecondaryPressure);
                float eva2MaxPressure = Mathf.Max(eva2PrimaryPressure, eva2SecondaryPressure);

                // Debug primary and secondary pressure for both eva1 and 2, also max pressure
                Debug.Log($"EV1 Primary Pressure: {eva1PrimaryPressure:F0}psi, Secondary Pressure: {eva1SecondaryPressure:F0}psi, Max Pressure: {eva1MaxPressure:F0}psi");
                Debug.Log($"EV2 Primary Pressure: {eva2PrimaryPressure:F0}psi, Secondary Pressure: {eva2SecondaryPressure:F0}psi, Max Pressure: {eva2MaxPressure:F0}psi");




                float progressEV1 = 0f;
                float progressEV2 = 0f;

                if (eva1MaxPressure < 10f && eva2MaxPressure < 10f)
                {
                    // If both EV1 and EV2 primary pressures are below 10, consider the task as completed
                    progressEV1 = 1f;
                    progressEV2 = 1f;
                }
                else
                {
                    // Calculate progress based on the primary pressure values
                    double oxyPriPressureMax = dataRanges.oxy_pri_pressure.Max;

                    progressEV1 = Mathf.Clamp01((float)((oxyPriPressureMax - eva1MaxPressure) / (oxyPriPressureMax - 10f)));
                    progressEV2 = Mathf.Clamp01((float)((oxyPriPressureMax - eva2MaxPressure) / (oxyPriPressureMax - 10f)));
                    Debug.Log("Do we even get here?");
                    Debug.Log($"EV1 Progress: {progressEV1:F2}, EV2 Progress: {progressEV2:F2}");
                }

                UpdateProgressBarSize(progressEV1, progressEV2);

                ev1ProgressTextMeshPro.text = $"EV1: {eva1MaxPressure:F0}psi (Current) < 10psi (Goal)";
                ev2ProgressTextMeshPro.text = $"EV2: {eva2MaxPressure:F0}psi (Current) < 10psi (Goal)";

                // Update the color of the progress bars based on their respective progress
                if (progressEV1 < 1f)
                {
                    if (progressEV1 > 0.01f)
                    {
                        ev1Foreground.GetComponent<Renderer>().material.color = inProgressColor;
                        uiaPanelController.ChangeToCustomColor(4, inProgressColor); // can possibly remove this
                    }
                    else
                    {
                        ev1Foreground.GetComponent<Renderer>().material.color = incompleteColor;
                        uiaPanelController.ChangeToCustomColor(4, incompleteColor);
                    }
                }
                else
                {
                    ev1Foreground.GetComponent<Renderer>().material.color = completedColor;
                    uiaPanelController.ChangeToCustomColor(4, completedColor);
                }

                if (progressEV2 < 1f)
                {
                    if (progressEV2 > 0.01f)
                    {
                        ev2Foreground.GetComponent<Renderer>().material.color = inProgressColor;
                        uiaPanelController.ChangeToCustomColor(4, inProgressColor); // can possibly remove this

                    }
                    else
                    {
                        ev2Foreground.GetComponent<Renderer>().material.color = incompleteColor;
                        uiaPanelController.ChangeToCustomColor(4, incompleteColor);

                    }
                }
                else
                {
                    ev2Foreground.GetComponent<Renderer>().material.color = completedColor;
                    uiaPanelController.ChangeToCustomColor(4, completedColor);

                }

                if (eva1MaxPressure <= 10f && eva2MaxPressure <= 10f)
                {
                    // Debug
                    Debug.Log("Both EV1 and EV2 O2 tanks are below 10psi. Completed?");
                    Debug.Log($"EV1 Max Pressure: {eva1MaxPressure:F0}psi, EV2 Max Pressure: {eva2MaxPressure:F0}psi");
                    // GoForward();
                }
                break;

            case 5: // Step 4: Vent O2 Tanks - UIA: OXYGEN O2 VENT � CLOSE
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Deactivate all overlays, then activate O2 vent (index 4) 
                uiaPanelController.DeactivateAllOverlays();
                uiaPanelController.SetOverlayState(4, true); // O2 Vent

                // Check if OXYGEN O2 VENT switch is CLOSE
                bool isOxygenO2VentClose = !uiaDataHandler.GetOxy_Vent();

                if (isOxygenO2VentClose)
                {
                    uiaPanelController.ChangeToCustomColor(4, completedColor);
                    progress = 1f;
                }
                else
                {
                    uiaPanelController.ChangeToCustomColor(4, incompleteColor);

                }

                // Update the status text
                taskStatusTextMeshPro.gameObject.SetActive(true);
                taskStatusTextMeshPro.text = $"<color={GetColorName(isOxygenO2VentClose)}>OXYGEN O2 VENT: {(isOxygenO2VentClose ? "CLOSED" : "OPEN")}</color>";
                break;

            case 6: // Step 5: Empty Water Tanks - BOTH DCU: PUMP � OPEN
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Deactivate all overlays, this task doesn't involve UIA overlay 
                uiaPanelController.DeactivateAllOverlays();

                // Check if PUMP switch is OPEN on both DCUs
                bool isDCU1PumpOpen = dcuDataHandler.GetPump("eva1");
                bool isDCU2PumpOpen = dcuDataHandler.GetPump("eva2");

                if (isDCU1PumpOpen && isDCU2PumpOpen)
                {
                    progress = 1f;
                }

                // Update the status text
                taskStatusTextMeshPro.gameObject.SetActive(true);
                taskStatusTextMeshPro.text = $"<color={GetColorName(isDCU1PumpOpen)}>DCU1 PUMP: {(isDCU1PumpOpen ? "OPEN" : "CLOSED")}</color>\n<color={GetColorName(isDCU2PumpOpen)}>DCU2 PUMP: {(isDCU2PumpOpen ? "OPEN" : "CLOSED")}</color>";
                break;

            case 7: // Step 5: Empty Water Tanks - UIA: EV-1, EV-2 WASTE WATER � OPEN
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Deactivate all overlays, then activate water waste vents (index 5 and 6) 
                uiaPanelController.DeactivateAllOverlays();
                uiaPanelController.SetOverlayState(5, true); // water ev1 waste Vent
                uiaPanelController.SetOverlayState(6, true); // water ev2 waste Vent

                // Check if EV-1 and EV-2 WASTE WATER switches are OPEN
                bool isEVA1WasteWaterOpen = uiaDataHandler.GetWater_Waste("eva1");
                bool isEVA2WasteWaterOpen = uiaDataHandler.GetWater_Waste("eva2");

                // Check and change overlay colors if the user has completed the task or not
                if (isEVA1WasteWaterOpen)
                {
                    uiaPanelController.ChangeToCustomColor(5, inProgressColor);
                }

                if (isEVA2WasteWaterOpen)
                {
                    uiaPanelController.ChangeToCustomColor(6, inProgressColor);
                }
                if (isEVA1WasteWaterOpen && isEVA2WasteWaterOpen)
                {
                    progress = 1f;
                }

                // Update the status text
                taskStatusTextMeshPro.gameObject.SetActive(true);
                taskStatusTextMeshPro.text = $"<color={GetColorName(isEVA1WasteWaterOpen)}>EV-1 WASTE WATER: {(isEVA1WasteWaterOpen ? "OPEN" : "CLOSED")}</color>\n<color={GetColorName(isEVA2WasteWaterOpen)}>EV-2 WASTE WATER: {(isEVA2WasteWaterOpen ? "OPEN" : "CLOSED")}</color>";
                break;

            case 8: // Step 5: Empty Water Tanks - Wait until EV1 and EV2 Coolant tanks < 5%
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Deactivate all overlays, then activate water waste vents (index 5 and 6) 
                uiaPanelController.DeactivateAllOverlays();
                uiaPanelController.SetOverlayState(5, true); // water ev1 waste Vent
                uiaPanelController.SetOverlayState(6, true); // water ev2 waste Vent

                // Activate the progress bar
                ActivateProgressBar();

                float eva1CoolantMl = telemetryDataHandler.GetCoolantMl("eva1");
                float eva2CoolantMl = telemetryDataHandler.GetCoolantMl("eva2");

                // Reverse the progress calculation for the progress bar
                float progressEV1case8 = Mathf.Clamp01((100f - eva1CoolantMl) / 95f);
                float progressEV2case8 = Mathf.Clamp01((100f - eva2CoolantMl) / 95f);

                Debug.Log($"EV1 Coolant: {eva1CoolantMl:F0}ml, EV2 Coolant: {eva2CoolantMl:F0}ml");
                Debug.Log($"EV1 Progress: {progressEV1case8:F2}, EV2 Progress: {progressEV2case8:F2}");

                UpdateProgressBarSize(progressEV1case8, progressEV2case8);

                ev1ProgressTextMeshPro.text = $"EV1: {eva1CoolantMl:F0}% (Current) < 5% (Goal)";
                ev2ProgressTextMeshPro.text = $"EV2: {eva2CoolantMl:F0}% (Current) < 5% (Goal)";

                // Update the color of the progress bars based on their respective progress
                if (progressEV1case8 > 0f)
                {
                    if (progressEV1case8 < 0.99f)
                    {
                        ev1Foreground.GetComponent<Renderer>().material.color = inProgressColor;
                        uiaPanelController.ChangeToCustomColor(5, inProgressColor);
                    }
                    else
                    {
                        ev1Foreground.GetComponent<Renderer>().material.color = completedColor;
                        uiaPanelController.ChangeToCustomColor(5, completedColor);
                    }
                }
                else
                {
                    ev1Foreground.GetComponent<Renderer>().material.color = incompleteColor;
                    uiaPanelController.ChangeToCustomColor(5, incompleteColor);
                }

                if (progressEV2case8 > 0f)
                {
                    if (progressEV2case8 < 0.99f)
                    {
                        ev2Foreground.GetComponent<Renderer>().material.color = inProgressColor;
                        uiaPanelController.ChangeToCustomColor(6, inProgressColor);
                    }
                    else
                    {
                        ev2Foreground.GetComponent<Renderer>().material.color = completedColor;
                        uiaPanelController.ChangeToCustomColor(6, completedColor);
                    }
                }
                else
                {
                    ev2Foreground.GetComponent<Renderer>().material.color = incompleteColor;
                    uiaPanelController.ChangeToCustomColor(6, incompleteColor);
                }

                if (eva1CoolantMl <= 5f && eva2CoolantMl <= 5f)
                {
                    Debug.Log("Both EV1 and EV2 Coolant tanks are below 5%. Completed?");
                    Debug.Log($"EV1 Coolant: {eva1CoolantMl:F0}ml, EV2 Coolant: {eva2CoolantMl:F0}ml");
                }
                break;

            case 9: // Step 5: Empty Water Tanks - UIA: EV-1, EV-2 WASTE WATER � CLOSE
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Deactivate all overlays, then activate water waste vents (index 5 and 6) 
                uiaPanelController.DeactivateAllOverlays();
                uiaPanelController.SetOverlayState(5, true); // water ev1 waste Vent
                uiaPanelController.SetOverlayState(6, true); // water ev2 waste Vent


                // Check if EV-1 and EV-2 WASTE WATER switches are CLOSE
                bool isEVA1WasteWaterClose = !uiaDataHandler.GetWater_Waste("eva1");
                bool isEVA2WasteWaterClose = !uiaDataHandler.GetWater_Waste("eva2");

                // Check and change overlay colors if the user has completed the task or not
                if (isEVA1WasteWaterClose)
                {
                    uiaPanelController.ChangeToCustomColor(5, completedColor);
                }
                else
                {
                    uiaPanelController.ChangeToCustomColor(5, incompleteColor);
                }
                if (isEVA2WasteWaterClose)
                {
                    uiaPanelController.ChangeToCustomColor(6, completedColor);
                }
                else
                {
                    uiaPanelController.ChangeToCustomColor(6, incompleteColor);
                }


                if (isEVA1WasteWaterClose && isEVA2WasteWaterClose)
                {
                    progress = 1f;
                }

                // Update the status text
                taskStatusTextMeshPro.gameObject.SetActive(true);
                taskStatusTextMeshPro.text = $"<color={GetColorName(isEVA1WasteWaterClose)}>EV-1 WASTE WATER: {(isEVA1WasteWaterClose ? "CLOSED" : "OPEN")}</color>\n<color={GetColorName(isEVA2WasteWaterClose)}>EV-2 WASTE WATER: {(isEVA2WasteWaterClose ? "CLOSED" : "OPEN")}</color>";
                break;

            case 10: // Step 6: Disconnect UIA from DCU - UIA: EV-1, EV-2 EMU PWR � OFF
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Deactivate all overlays, then activate power ev1 (index 0) and power ev2 (index 1)
                uiaPanelController.DeactivateAllOverlays();
                uiaPanelController.SetOverlayState(0, true); // Power EV1
                uiaPanelController.SetOverlayState(1, true); // Power EV2

                // Check if EV-1 and EV-2 EMU PWR switches are OFF
                bool isEVA1PowerOff = !uiaDataHandler.GetPower("eva1");
                bool isEVA2PowerOff = !uiaDataHandler.GetPower("eva2");

                // Check and change overlay colors if the user has completed the task or not
                if (isEVA1PowerOff)
                {
                    uiaPanelController.ChangeToCustomColor(0, completedColor);
                }
                else
                {
                    uiaPanelController.ChangeToCustomColor(0, incompleteColor);
                }

                if (isEVA2PowerOff)
                {
                    uiaPanelController.ChangeToCustomColor(1, completedColor);
                }
                else
                {
                    uiaPanelController.ChangeToCustomColor(1, incompleteColor);
                }

                if (isEVA1PowerOff && isEVA2PowerOff)
                {
                    progress = 1f;
                }

                // Update the status text
                taskStatusTextMeshPro.gameObject.SetActive(true);
                taskStatusTextMeshPro.text = $"<color={GetColorName(isEVA1PowerOff)}>EV-1 EMU PWR: {(isEVA1PowerOff ? "OFF" : "ON")}</color>\n<color={GetColorName(isEVA2PowerOff)}>EV-2 EMU PWR: {(isEVA2PowerOff ? "OFF" : "ON")}</color>";
                break;

            case 11: // Step 6: Disconnect UIA from DCU - DCU: EV1 and EV2 disconnect umbilical
                // Reset progress bar and text
                ResetProgressBarAndText();


                // Deactivate all overlays, this task doesn't require UIA panel
                uiaPanelController.DeactivateAllOverlays();

                // Add text
                taskStatusTextMeshPro.gameObject.SetActive(true);
                taskStatusTextMeshPro.text = $"<color=green>EV-1 and EV-2: Disconnected UIA and DCU umbilical.</color>";

                // Check if EV1 and EV2 have disconnected UIA and DCU umbilical
                // You can add the condition here based on the umbilical disconnection status
                // For now, let's assume they are disconnected
                progress = 1f;
                break;
            case 12:
                // Reset progress bar and text
                ResetProgressBarAndText();

                // Deactivate all overlays for other cases
                uiaPanelController.DeactivateAllOverlays();

                // Add text
                taskStatusTextMeshPro.gameObject.SetActive(true);
                taskStatusTextMeshPro.text = $"<color=green>Congratulations! You have completed the SIGMA ingress task.</color>";

                progress = 1f;
                break;

            default:
                // Reset progress bar and text for other cases
                ResetProgressBarAndText();
                // Deactivate all overlays for other cases
                uiaPanelController.DeactivateAllOverlays();


                progress = 1f;
                break;
        }

        // Update the task status text
        if (progress < 1f)
        {
            taskStatusTextMeshPro.color = Color.red;
        }
        else
        {
            taskStatusTextMeshPro.color = Color.green;
        }
    }

    private void UpdateProgressBarSize(float progressEV1, float progressEV2)
    {
        // In Unity, if you want to transform a 3D object in one direction, you have to
        // translate the object as you scale it to ensure there exists a pivot point

        // EV1 Progress Bar
        Vector3 ev1BackgroundScale = ev1Background.localScale;
        Vector3 ev1ForegroundScale = ev1Foreground.localScale;

        ev1ForegroundScale.x = ev1BackgroundScale.x * progressEV1;
        ev1Foreground.localScale = ev1ForegroundScale;

        Vector3 ev1ForegroundPosition = ev1Foreground.localPosition;
        Vector3 ev1BackgroundPosition = ev1Background.localPosition;
        float ev1LeftPivot = ev1BackgroundPosition.x - ev1BackgroundScale.x * 0.5f;
        ev1ForegroundPosition.x = ev1LeftPivot + ev1ForegroundScale.x * 0.5f;
        ev1Foreground.localPosition = ev1ForegroundPosition;

        // EV2 Progress Bar
        Vector3 ev2BackgroundScale = ev2Background.localScale;
        Vector3 ev2ForegroundScale = ev2Foreground.localScale;

        ev2ForegroundScale.x = ev2BackgroundScale.x * progressEV2;
        ev2Foreground.localScale = ev2ForegroundScale;

        Vector3 ev2ForegroundPosition = ev2Foreground.localPosition;
        Vector3 ev2BackgroundPosition = ev2Background.localPosition;
        float ev2LeftPivot = ev2BackgroundPosition.x - ev2BackgroundScale.x * 0.5f;
        ev2ForegroundPosition.x = ev2LeftPivot + ev2ForegroundScale.x * 0.5f;
        ev2Foreground.localPosition = ev2ForegroundPosition;
    }

    private void ActivateProgressBar()
    {
        ev1Background.gameObject.SetActive(true);
        ev1Foreground.gameObject.SetActive(true);
        ev1ProgressTextMeshPro.gameObject.SetActive(true);

        ev2Background.gameObject.SetActive(true);
        ev2Foreground.gameObject.SetActive(true);
        ev2ProgressTextMeshPro.gameObject.SetActive(true);
    }

    private void ResetProgressBarAndText()
    {
        // Deactivate the progress bars
        ev1Background.gameObject.SetActive(false);
        ev1Foreground.gameObject.SetActive(false);
        ev1ProgressTextMeshPro.gameObject.SetActive(false);

        ev2Background.gameObject.SetActive(false);
        ev2Foreground.gameObject.SetActive(false);
        ev2ProgressTextMeshPro.gameObject.SetActive(false);

        // Reset the progress text
        ev1ProgressTextMeshPro.text = string.Empty;
        ev2ProgressTextMeshPro.text = string.Empty;

        // Deactivate the status text
        taskStatusTextMeshPro.gameObject.SetActive(false);

        // Reset the status text
        taskStatusTextMeshPro.text = string.Empty;
    }

    public void GoForward()
    {
        int currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
        if (currentTaskIndex < tasks.Length - 1)
        {
            TaskManager.Instance.SetCurrentTaskIndex(currentTaskIndex + 1);
            UpdateTaskText();
        }
    }

    public void GoBack()
    {
        int currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
        if (currentTaskIndex > 0)
        {
            TaskManager.Instance.SetCurrentTaskIndex(currentTaskIndex - 1);
            UpdateTaskText();
        }
    }

    private void UpdateTaskText()
    {
        int currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
        string currentTask = tasks[currentTaskIndex];
        string[] lines = currentTask.Split('\n');
        stepTitleTextMeshPro.text = lines[0];
        taskTextMeshPro.text = string.Join("\n", lines, 1, lines.Length - 1);
    }

    private void UpdateSwitchLocationText(int currentTaskIndex)
    {
        if (currentTaskIndex >= 0 && currentTaskIndex < tasks.Length)
        {
            string currentTask = tasks[currentTaskIndex];
            if (currentTask.Contains("UIA") && currentTask.Contains("DCU"))
            {
                SwitchLocationText.text = "DCU\nUIA";
            }
            else if (currentTask.Contains("UIA"))
            {
                SwitchLocationText.text = "UIA";
            }
            else if (currentTask.Contains("DCU"))
            {
                SwitchLocationText.text = "DCU";
            }
            else
            {
                SwitchLocationText.text = "";
            }
        }
        else
        {
            SwitchLocationText.text = "";
        }
    }
}