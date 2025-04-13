using UnityEngine;
using TMPro;

public class HeartbeatDisplay : MonoBehaviour
{
    [Header("TSS References")]
    public TELEMETRYDataHandler telemetryDataHandler;
    public EVANumberHandler evaNumberHandler;

    [Header("EV1 UI References")]
    public Transform ev1Background;
    public Transform ev1Foreground;
    public TextMeshPro ev1HeartRateTextMeshPro;

    [Header("EV2 UI References")]
    public Transform ev2Background;
    public Transform ev2Foreground;
    public TextMeshPro ev2HeartRateTextMeshPro;

    [Header("Color Configuration")]
    public Color normalColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;

    private void Update()
    {
        UpdateHeartRateDisplay();
    }

    private void UpdateHeartRateDisplay()
    {
        // Update EV1 heart rate
        string ev1Key = "eva1";
        float ev1HeartRate = telemetryDataHandler.GetHeartRate(ev1Key);
        UpdateUI(ev1HeartRate, ev1Foreground, ev1HeartRateTextMeshPro);

        // Update EV2 heart rate
        string ev2Key = "eva2";
        float ev2HeartRate = telemetryDataHandler.GetHeartRate(ev2Key);
        UpdateUI(ev2HeartRate, ev2Foreground, ev2HeartRateTextMeshPro);
    }

    private void UpdateUI(float heartRate, Transform foreground, TextMeshPro textMeshPro)
    {
        // Update progress bar scale
        float normalizedHeartRate = Mathf.Clamp01(heartRate / 160f); // Assuming 160 bpm is the max
        foreground.localScale = new Vector3(normalizedHeartRate, 1f, 1f);

        // Update text
        textMeshPro.text = $"{heartRate:F0} BPM";

        // Update color based on heart rate
        if (heartRate < 50)
        {
            textMeshPro.color = criticalColor; // Critical (too low)
        }
        else if (heartRate > 160)
        {
            textMeshPro.color = warningColor; // Warning (too high)
        }
        else
        {
            textMeshPro.color = normalColor; // Normal
        }
    }
}