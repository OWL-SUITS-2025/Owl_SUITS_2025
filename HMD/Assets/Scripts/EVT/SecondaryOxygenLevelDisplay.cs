// NOTE: We could probably merge both this script and PrimaryOxygenLevelDisplay.cs
// I say this because the only difference is the function call on line 51/73 (telemetryDataHandler.GetOxySecStorage(ev1Key))
// But for now, we will keep them separate.


using UnityEngine;
using TMPro;

public class SecondaryOxygenLevelDisplay : MonoBehaviour
{
    [Header("Secondary Oxygen Level Display")]
    [Tooltip("This script is for SECONDARY oxygen levels for both EVAs.")]
    [Header("TSS References")]
    public TELEMETRYDataHandler telemetryDataHandler;
    public DataRanges dataRanges;
    // This script allows us to get current EVA Number, used to denote which EVA is primary.
    public EVANumberHandler evaNumberHandler;
    
    // The new interface design seems to want information for both EVAs.
    // I've added information for both EVAs in the EVTDisplay.
    // We still use the EVA Number Handler to denote which EVA is the primary one.
    // (e.g., if user is EVA1, then EVA1 is primary and EVA2 is secondary)

    [Header("EV1 UI References")]
    public Transform ev1background;
    public Transform ev1foreground;
    public TextMeshPro ev1oxygenLevelTextMeshPro;

    [Header("EV2 UI References")]
    public Transform ev2background;
    public Transform ev2foreground;
    public TextMeshPro ev2oxygenLevelTextMeshPro;

    [Header("Color Configuration")]
    public Color normalColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;


    

    private void Update()
    {
        UpdateOxygenLevel();
    }

    private void UpdateOxygenLevel()
    {
        // Update EV1 oxygen level
        string ev1Key = "eva1";
        float ev1OxygenLevel = telemetryDataHandler.GetOxySecStorage(ev1Key);
        // Debug.Log($"EV1 Secondary Oxygen Level: {ev1OxygenLevel}");



        float ev1Progress = Mathf.Clamp01(ev1OxygenLevel/100f);

        // Debug.Log($"EV1 Secondary Oxygen Level Progress: {ev1Progress}");
        UpdateForegroundBarSize(ev1foreground, ev1background, ev1Progress);

        ev1oxygenLevelTextMeshPro.text = $"{ev1OxygenLevel:F0}%";

        if (ev1OxygenLevel < 30f)
        {
            ev1foreground.GetComponent<Renderer>().material.color = criticalColor;
        }
        else if (ev1OxygenLevel < 60f)
        {
            ev1foreground.GetComponent<Renderer>().material.color = warningColor;
        }
        else
        {
            ev1foreground.GetComponent<Renderer>().material.color = normalColor;
        }

        // Update EV2 oxygen level
        string ev2Key = "eva2";
        float ev2OxygenLevel = telemetryDataHandler.GetOxySecStorage(ev2Key);
        // Debug.Log($"EV2 Secondary Oxygen Level: {ev2OxygenLevel}");

        float ev2Progress = Mathf.Clamp01(ev2OxygenLevel/100f);
        
        // Debug.Log($"EV2 Secondary Oxygen Level Progress: {ev2Progress}");
        
        UpdateForegroundBarSize(ev2foreground, ev2background, ev2Progress);

        ev2oxygenLevelTextMeshPro.text = $"{ev2OxygenLevel:F0}%";

        if (ev2OxygenLevel < 30f)
        {
            ev2foreground.GetComponent<Renderer>().material.color = criticalColor;
        }
        else if (ev2OxygenLevel < 60f)
        {
            ev2foreground.GetComponent<Renderer>().material.color = warningColor;
        }
        else
        {
            ev2foreground.GetComponent<Renderer>().material.color = normalColor;
        }
    }

    private static void UpdateForegroundBarSize(Transform foreground, Transform background, float progress)
    {
        Vector3 backgroundScale = background.localScale;
        Vector3 foregroundScale = foreground.localScale;

        // Calculate the new scale of the foreground object
        foregroundScale.x = backgroundScale.x * progress;

        // Update the scale of the foreground object
        foreground.localScale = foregroundScale;

        // Align the left side of the foreground bar with the left side of the background bar
        Vector3 foregroundPosition = foreground.localPosition;
        Vector3 backgroundPosition = background.localPosition;
        float leftPivot = backgroundPosition.x - backgroundScale.x * 0.5f;
        foregroundPosition.x = leftPivot + foregroundScale.x * 0.5f;

        // Update foreground local position
        foreground.localPosition = foregroundPosition;
    }
}