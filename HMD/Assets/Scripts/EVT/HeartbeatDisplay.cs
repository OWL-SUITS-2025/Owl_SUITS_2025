using UnityEngine;
using TMPro;

public class HeartbeatDisplay : MonoBehaviour
{
    [Header("TSS References")]
    public TELEMETRYDataHandler telemetryDataHandler;
    public DataRanges dataRanges;
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
        float ev1Progress = Mathf.Clamp01((ev1HeartRate - (float)dataRanges.heart_rate.Min) / (float)(dataRanges.heart_rate.Max - dataRanges.heart_rate.Min));
        UpdateForegroundBarSize(ev1Foreground, ev1Background, ev1Progress);

        ev1HeartRateTextMeshPro.text = $"{ev1HeartRate:F0} BPM";

        if (ev1HeartRate < 60f || ev1HeartRate > 160f)
        {
            ev1Foreground.GetComponent<Renderer>().material.color = criticalColor;
        }
        else if ((ev1HeartRate >= 60f && ev1HeartRate < 80f) || (ev1HeartRate > 130f && ev1HeartRate <= 160f))
        {
            ev1Foreground.GetComponent<Renderer>().material.color = warningColor;
        }
        else if (ev1HeartRate >= 80f && ev1HeartRate <= 130f)
        {
            ev1Foreground.GetComponent<Renderer>().material.color = normalColor;
        }
        
        
        // Update EV2 heart rate
        string ev2key = "eva2";
        float ev2HeartRate = telemetryDataHandler.GetHeartRate(ev2key);
        float ev2Progress = Mathf.Clamp01((ev2HeartRate - (float)dataRanges.heart_rate.Min) / (float)(dataRanges.heart_rate.Max - dataRanges.heart_rate.Min));
        UpdateForegroundBarSize(ev2Foreground, ev2Background, ev2Progress);

        ev2HeartRateTextMeshPro.text = $"{ev2HeartRate:F0} BPM";

        if (ev2HeartRate < 60f || ev2HeartRate > 160f)
        {
            ev2Foreground.GetComponent<Renderer>().material.color = criticalColor;
            
        }
        else if ((ev2HeartRate >= 60f && ev2HeartRate < 80f) || (ev2HeartRate > 130f && ev2HeartRate <= 160f))
        {
            ev2Foreground.GetComponent<Renderer>().material.color = warningColor;
        }
        else if (ev2HeartRate >= 80f && ev2HeartRate <= 130f)
        {
            ev2Foreground.GetComponent<Renderer>().material.color = normalColor;
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