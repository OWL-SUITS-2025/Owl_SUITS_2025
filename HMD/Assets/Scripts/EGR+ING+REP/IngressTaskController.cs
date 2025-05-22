using UnityEngine;
using TMPro;
using System.Collections;

public class IngressTaskController : MonoBehaviour
{
    [Header("UIA Panel Reference")]
    public UIAPanelImage uiaPanelController;

    [Header("TSS Data Handlers")]
    public TELEMETRYDataHandler telemetryDataHandler;
    public DataRanges dataRanges;
    public UIADataHandler uiaDataHandler;
    public DCUDataHandler dcuDataHandler;

    [Header("EVA Number Handler")]
    public EVANumberHandler evaNumberHandler;

    [Header("Current Step Progress Bar Game Objects")]
    public Transform evaBackground;
    public Transform evaForeground;
    public TextMeshPro evaProgressTextMeshPro;

    [Header("Overall Progress Bar Game Objects")]
    public Transform overallBackgroundBar;
    public Transform overallForegroundBar;
    public TextMeshPro overallProgressTextMeshPro;

    [Header("Task Panel Misc Objects")]
    public TextMeshPro taskStatusTextMeshPro;
    public TextMeshPro stepTitleTextMeshPro;
    public TextMeshPro taskTextMeshPro;
    public TextMeshPro SwitchLocationText;

    [Header("Colors for text and overlays")]
    public Color completedColor = Color.green;
    public Color incompleteColor = Color.red;
    public Color inProgressColor = Color.yellow;

    private int evaNumber;
    private string evaIDString;

    private int overlayEMUPowerIndex;
    private int overlayWasteWaterIndex;
    private const int OVERLAY_O2_VENT_INDEX = 4;
    
    private string[] tasks = new string[]
    {
        "Connect UIA to DCU and start Depress\nUIA and DCU: EV# connect UIA and DCU umbilical",
        "Connect UIA to DCU and start Depress\nUIA: EV-# EMU PWR – ON",
        "Connect UIA to DCU and start Depress\nDCU: BATT – UMB",
        "Vent O2 Tanks\nUIA: OXYGEN O2 VENT – OPEN",
        "Vent O2 Tanks\nHMD: Wait until both Primary and Secondary OXY tanks are < 10 psi",
        "Vent O2 Tanks\nUIA: OXYGEN O2 VENT – CLOSE",
        "Empty Water Tanks\nDCU: PUMP – OPEN",
        "Empty Water Tanks\nUIA: EV-# WASTE WATER – OPEN",
        "Empty Water Tanks\nHMD: Wait until water EV# Coolant tank is < 5%",
        "Empty Water Tanks\nUIA: EV-# WASTE WATER – CLOSE",
        "Disconnect UIA from DCU\nUIA: EV-# EMU PWR – OFF",
        "Disconnect UIA from DCU\nDCU: EV# disconnect umbilical",
        "Procedure Complete\nIngress procedure finished."
    };
        
    private int currentTaskIndex = 0;

    private string GetColorName(bool conditionMet, bool isVerification = false)
    {
        if (isVerification) return conditionMet ? "green" : "red";
        return conditionMet ? "green" : "red";
    }

    private Color GetProgressColor(bool conditionMet, bool inProgressState = false)
    {
        if (conditionMet) return completedColor;
        if (inProgressState) return inProgressColor;
        return incompleteColor;
    }

    private void Start()
    {
        if (evaNumberHandler == null)
        {
            Debug.LogError("IngressTaskController: EVANumberHandler is not assigned! Defaulting to EV1.");
            evaNumber = 1;
        }
        else
        {
            evaNumber = evaNumberHandler.getEVANumber();
        }
        evaIDString = "eva" + evaNumber;

        overlayEMUPowerIndex = evaNumber - 1;
        overlayWasteWaterIndex = 5 + (evaNumber - 1);
        
        FormatTaskStrings();

        if (TaskManager.Instance == null)
        {
            Debug.LogError("IngressTaskController: TaskManager.Instance is NULL! UI will likely not function correctly for task progression.");
            currentTaskIndex = 0; 
        }
        else
        {
            currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
        }

        if (currentTaskIndex >= tasks.Length)
        {
            if (TaskManager.Instance != null) TaskManager.Instance.SetCurrentTaskIndex(0);
            currentTaskIndex = 0;
        }

        UpdateTaskText();
        UpdateOverallProgressBar();
        UpdateOverallProgressText();

        if (uiaPanelController != null)
        {
            uiaPanelController.PositionPanelToLeftOfUser();
        }
        else
        {
            Debug.LogError("IngressTaskController: UIAPanelController is not assigned!");
        }

        // This was in your script, will always reset to task 0 on start of this scene/controller
        currentTaskIndex = 0;
        if (TaskManager.Instance != null) TaskManager.Instance.SetCurrentTaskIndex(0);
        else Debug.LogError("IngressTaskController: TaskManager.Instance is NULL when trying to set index to 0 at end of Start().");
    }

    private void FormatTaskStrings()
    {
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = tasks[i].Replace("EV#", "EV" + evaNumber).Replace("EV-#", "EV-" + evaNumber).Replace("EMU-#", "EMU-" + evaNumber);
        }
    }

    private void Update()
    {
        if (telemetryDataHandler == null || uiaDataHandler == null || dcuDataHandler == null)
        {
            if (taskStatusTextMeshPro != null && !taskStatusTextMeshPro.gameObject.activeSelf) 
            {
                taskStatusTextMeshPro.text = "<color=red>Error: Data Handlers not assigned.</color>";
                taskStatusTextMeshPro.gameObject.SetActive(true);
            }
            // Keep this error log as it's critical for setup
            Debug.LogError("IngressTaskController: One or more data handlers are not assigned in the inspector! Halting UpdateProgressBar.");
            return;
        }
        UpdateProgressBar();
    }

    private void UpdateOverallProgressBar()
    {
        if (TaskManager.Instance == null) { return; }
        int taskIdx = TaskManager.Instance.GetCurrentTaskIndex();
        int totalTasks = tasks.Length;
        if (totalTasks == 0) { return; }

        float progress = (float)taskIdx / (totalTasks - 1);
        if (taskIdx == totalTasks - 1) progress = 1f;

        if (overallBackgroundBar && overallForegroundBar)
        {
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
    }

    private void UpdateOverallProgressText()
    {
        if (TaskManager.Instance == null) { return; }
        int taskIdx = TaskManager.Instance.GetCurrentTaskIndex();
        int totalTasks = tasks.Length;
        string textToSet = $"Ingress: {taskIdx + 1}/{totalTasks} Steps";
        if (overallProgressTextMeshPro) overallProgressTextMeshPro.text = textToSet;
    }

    private void UpdateProgressBar()
    {
        if (TaskManager.Instance == null) { return; }
        currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
        
        UpdateSwitchLocationText(currentTaskIndex);
        bool taskCompleted = false;
        Color currentOverlayColor = incompleteColor;

        if (uiaPanelController == null)
        {
            Debug.LogError("IngressTaskController: UIAPanelController is NULL!"); // Corrected controller name
            if (taskStatusTextMeshPro != null) { taskStatusTextMeshPro.text = "<color=red>UIA Panel Error</color>"; taskStatusTextMeshPro.gameObject.SetActive(true); }
            return;
        }

        ResetProgressBarAndText();
        uiaPanelController.DeactivateAllOverlays();
        
        float maxOxyPressure = 0f; 

        switch (currentTaskIndex)
        {
            case 0:
                taskCompleted = true;
                taskStatusTextMeshPro.text = "Manually verify that UIA and DCU umbilical is connected.";
                currentOverlayColor = completedColor;
                break;

            case 1:
                uiaPanelController.SetOverlayState(overlayEMUPowerIndex, true);
                bool isEvaPowerOn = uiaDataHandler.GetPower(evaIDString);
                taskCompleted = isEvaPowerOn;
                currentOverlayColor = GetProgressColor(taskCompleted);
                uiaPanelController.ChangeToCustomColor(overlayEMUPowerIndex, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>EV-{evaNumber} EMU PWR: {(isEvaPowerOn ? "ON" : "OFF")}</color>";
                break;

            case 2:
                bool isBattUmb = dcuDataHandler.GetBatt(evaIDString);
                taskCompleted = isBattUmb;
                currentOverlayColor = GetProgressColor(taskCompleted);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>DCU BATT: {(isBattUmb ? "UMB" : "LOCAL")}</color>";
                break;

            case 3:
                uiaPanelController.SetOverlayState(OVERLAY_O2_VENT_INDEX, true);
                bool isO2VentOpen = uiaDataHandler.GetOxy_Vent();
                taskCompleted = isO2VentOpen;
                currentOverlayColor = GetProgressColor(taskCompleted, taskCompleted); 
                if (taskCompleted) currentOverlayColor = inProgressColor; 
                else currentOverlayColor = incompleteColor;
                uiaPanelController.ChangeToCustomColor(OVERLAY_O2_VENT_INDEX, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>OXYGEN O2 VENT: {(isO2VentOpen ? "OPEN" : "CLOSED")}</color>";
                break;

            case 4:
                ActivateProgressBar();
                uiaPanelController.SetOverlayState(OVERLAY_O2_VENT_INDEX, true);
                float priOxyPressure = telemetryDataHandler.GetOxyPriPressure(evaIDString);
                float secOxyPressure = telemetryDataHandler.GetOxySecPressure(evaIDString);
                maxOxyPressure = Mathf.Max(priOxyPressure, secOxyPressure);
                taskCompleted = maxOxyPressure < 10f;

                float progressWaitOxyLow = 0f;
                if (taskCompleted)
                {
                    progressWaitOxyLow = 1f;
                    currentOverlayColor = completedColor;
                }
                else
                {
                    float initialMaxPressure = (dataRanges != null && dataRanges.oxy_pri_pressure != null) ? (float)dataRanges.oxy_pri_pressure.Max : 3000f;
                    if (initialMaxPressure <= 10f) initialMaxPressure = 3000f;
                    progressWaitOxyLow = Mathf.Clamp01((initialMaxPressure - maxOxyPressure) / (initialMaxPressure - 10f));
                    currentOverlayColor = GetProgressColor(taskCompleted, maxOxyPressure < initialMaxPressure && maxOxyPressure >=10f);
                }
                UpdateEvaProgressBar(progressWaitOxyLow, currentOverlayColor);
                if(evaProgressTextMeshPro) evaProgressTextMeshPro.text = $"EVA: {maxOxyPressure:F0}psi (Current) < 10psi (Goal)";
                uiaPanelController.ChangeToCustomColor(OVERLAY_O2_VENT_INDEX, currentOverlayColor);
                break;

            case 5:
                uiaPanelController.SetOverlayState(OVERLAY_O2_VENT_INDEX, true);
                bool isO2VentClosed = !uiaDataHandler.GetOxy_Vent();
                taskCompleted = isO2VentClosed;
                currentOverlayColor = GetProgressColor(taskCompleted);
                uiaPanelController.ChangeToCustomColor(OVERLAY_O2_VENT_INDEX, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}> O2 VENT: {(isO2VentClosed ? "CLOSED" : "OPEN")}</color>";
                break;

            case 6:
                bool isPumpOpen = dcuDataHandler.GetPump(evaIDString);
                taskCompleted = isPumpOpen;
                if (taskCompleted) currentOverlayColor = inProgressColor;
                else currentOverlayColor = incompleteColor;
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>DCU PUMP: {(isPumpOpen ? "OPEN" : "CLOSED")}</color>";
                break;

            case 7:
                uiaPanelController.SetOverlayState(overlayWasteWaterIndex, true);
                bool isWasteWaterOpen = uiaDataHandler.GetWater_Waste(evaIDString);
                taskCompleted = isWasteWaterOpen;
                if (taskCompleted) currentOverlayColor = inProgressColor;
                else currentOverlayColor = incompleteColor;
                uiaPanelController.ChangeToCustomColor(overlayWasteWaterIndex, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>EV-{evaNumber} WASTE WATER: {(isWasteWaterOpen ? "OPEN" : "CLOSED")}</color>";
                break;

            case 8:
                ActivateProgressBar();
                uiaPanelController.SetOverlayState(overlayWasteWaterIndex, true);
                float coolantMl = telemetryDataHandler.GetCoolantMl(evaIDString);
                taskCompleted = coolantMl < 5f;

                float progressCoolantLow = 0f;
                if (taskCompleted)
                {
                    progressCoolantLow = 1f;
                    currentOverlayColor = completedColor;
                }
                else
                {
                    float range = 100f - 5f; if (range <=0) range = 1f;
                    progressCoolantLow = Mathf.Clamp01((100f - coolantMl) / range);
                    currentOverlayColor = GetProgressColor(taskCompleted, coolantMl < 100f && coolantMl >= 5f);
                }
                UpdateEvaProgressBar(progressCoolantLow, currentOverlayColor);
                if (evaProgressTextMeshPro != null) evaProgressTextMeshPro.text = $"EVA Coolant: {coolantMl:F0}% (Current) < 5% (Goal)";
                uiaPanelController.ChangeToCustomColor(overlayWasteWaterIndex, currentOverlayColor);
                break;

            case 9:
                uiaPanelController.SetOverlayState(overlayWasteWaterIndex, true);
                bool isWasteWaterClose = !uiaDataHandler.GetWater_Waste(evaIDString);
                taskCompleted = isWasteWaterClose;
                currentOverlayColor = GetProgressColor(taskCompleted);
                uiaPanelController.ChangeToCustomColor(overlayWasteWaterIndex, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>EV-{evaNumber} WASTE WATER: {(isWasteWaterClose ? "CLOSED" : "OPEN")}</color>";
                break;

            case 10:
                uiaPanelController.SetOverlayState(overlayEMUPowerIndex, true);
                bool isEvaPowerOff = !uiaDataHandler.GetPower(evaIDString);
                taskCompleted = isEvaPowerOff;
                currentOverlayColor = GetProgressColor(taskCompleted);
                uiaPanelController.ChangeToCustomColor(overlayEMUPowerIndex, currentOverlayColor);
                taskStatusTextMeshPro.text = $"<color={GetColorName(taskCompleted)}>EV-{evaNumber} EMU PWR: {(isEvaPowerOff ? "OFF" : "ON")}</color>";
                break;

            case 11:
                taskCompleted = true;
                taskStatusTextMeshPro.text = $"EV{evaNumber} disconnect from DCU umbilical (Manually verify)";
                currentOverlayColor = completedColor;
                break;

            case 12:
                taskCompleted = true;
                currentOverlayColor = completedColor;
                taskStatusTextMeshPro.text = "Ingress Procedure Complete! Welcome back to the pod!";
                break;

            default:
                ResetProgressBarAndText();
                uiaPanelController.DeactivateAllOverlays();
                taskStatusTextMeshPro.text = "Unknown Task Index.";
                currentOverlayColor = incompleteColor;
                break;
        }
        
        if (taskStatusTextMeshPro != null)
        {
            if (!string.IsNullOrEmpty(taskStatusTextMeshPro.text))
            {
                taskStatusTextMeshPro.gameObject.SetActive(true);
                Color baseTextColor = taskCompleted ? completedColor : incompleteColor;

                if (IsWaitingTask(currentTaskIndex) && !taskCompleted) {
                    bool inProg = false;
                    if (currentTaskIndex == 4) { 
                        float pri = telemetryDataHandler.GetOxyPriPressure(evaIDString);
                        float sec = telemetryDataHandler.GetOxySecPressure(evaIDString);
                        float currentMaxOxy = Mathf.Max(pri,sec);
                        float initialMax = (dataRanges != null && dataRanges.oxy_pri_pressure != null) ? (float)dataRanges.oxy_pri_pressure.Max : 3000f;
                        inProg = currentMaxOxy < initialMax && currentMaxOxy >= 10f;
                    } else if (currentTaskIndex == 8) { 
                        float currentCoolant = telemetryDataHandler.GetCoolantMl(evaIDString);
                        inProg = currentCoolant < 100f && currentCoolant >= 5f;
                    }
                    if(inProg) baseTextColor = inProgressColor;
                }
                taskStatusTextMeshPro.color = baseTextColor;
            }
            else
            {
                taskStatusTextMeshPro.gameObject.SetActive(false);
            }
        }
    }

    private bool IsWaitingTask(int taskIndex) {
        return taskIndex == 4 || taskIndex == 8;
    }

    private void UpdateEvaProgressBar(float progress, Color barColor)
    {
        if (evaBackground == null || evaForeground == null) return;

        Vector3 backgroundScale = evaBackground.localScale;
        Vector3 foregroundScale = evaForeground.localScale;
        foregroundScale.x = backgroundScale.x * progress;
        evaForeground.localScale = foregroundScale;

        Vector3 foregroundPosition = evaForeground.localPosition;
        Vector3 backgroundPosition = evaBackground.localPosition;
        float leftPivot = backgroundPosition.x - backgroundScale.x * 0.5f;
        foregroundPosition.x = leftPivot + foregroundScale.x * 0.5f;
        evaForeground.localPosition = foregroundPosition;

        Renderer fgRenderer = evaForeground.GetComponent<Renderer>();
        if (fgRenderer != null && fgRenderer.material != null)
        {
            fgRenderer.material.color = barColor;
        }
    }

    private void ActivateProgressBar()
    {
        if(evaBackground) evaBackground.gameObject.SetActive(true);
        if(evaForeground) evaForeground.gameObject.SetActive(true);
        if(evaProgressTextMeshPro) evaProgressTextMeshPro.gameObject.SetActive(true);
    }

    private void ResetProgressBarAndText()
    {
        if(evaBackground) evaBackground.gameObject.SetActive(false);
        if(evaForeground) evaForeground.gameObject.SetActive(false);
        if(evaProgressTextMeshPro != null) {
            evaProgressTextMeshPro.gameObject.SetActive(false);
            evaProgressTextMeshPro.text = string.Empty;
        }

        if(taskStatusTextMeshPro != null) {
            taskStatusTextMeshPro.gameObject.SetActive(false);
            taskStatusTextMeshPro.text = string.Empty;
        }
    }

    public void GoForward()
    {
        if (TaskManager.Instance == null) { return; }
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
        if (TaskManager.Instance == null) { return; }
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
        if (TaskManager.Instance == null) { return; }
        int taskIdx = TaskManager.Instance.GetCurrentTaskIndex();
        if (taskIdx >= 0 && taskIdx < tasks.Length)
        {
            string currentTaskString = tasks[taskIdx];
            string[] lines = currentTaskString.Split('\n');
            if (stepTitleTextMeshPro) stepTitleTextMeshPro.text = lines[0];
            
            if (taskTextMeshPro) {
                taskTextMeshPro.text = string.Join("\n", lines, 1, lines.Length - 1);
            }
        }
    }

    private void UpdateSwitchLocationText(int taskIndex)
    {
        if (SwitchLocationText == null) { return; } 

        if (taskIndex >= 0 && taskIndex < tasks.Length)
        {
            string currentTaskFullString = tasks[taskIndex];
            string[] lines = currentTaskFullString.Split('\n');
            string locationTextToSet = "";
            if (lines.Length > 1)
            {
                string subtaskLine = lines[1];
                if (subtaskLine.Contains(":"))
                {
                    string location = subtaskLine.Substring(0, subtaskLine.IndexOf(':')).Trim();
                    if (location.Equals("UIA and DCU", System.StringComparison.OrdinalIgnoreCase)) locationTextToSet = "UIA\nDCU";
                    else if (location.Equals("HMD", System.StringComparison.OrdinalIgnoreCase)) locationTextToSet = "";
                    else locationTextToSet = location;
                }
            }
            SwitchLocationText.text = locationTextToSet;
        }
        else
        {
            SwitchLocationText.text = "";
        }
    }
}
