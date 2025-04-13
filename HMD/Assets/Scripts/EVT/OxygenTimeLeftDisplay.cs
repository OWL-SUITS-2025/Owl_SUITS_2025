using UnityEngine;
using TMPro;

public class OxygenTimeLeftDisplay : MonoBehaviour
{
    [Header("TSS References")]
    public TELEMETRYDataHandler telemetryDataHandler;
    public EVANumberHandler evaNumberHandler;

    [Header("EV1 UI References")]
    public TextMeshPro ev1OxygenTimeLeftTextMeshPro;

    [Header("EV2 UI References")]
    public TextMeshPro ev2OxygenTimeLeftTextMeshPro;

    private void Update()
    {
        UpdateOxygenTimeLeft();
    }

    private void UpdateOxygenTimeLeft()
    {
        // Update EV1 oxygen time left
        string ev1Key = "eva1";
        float ev1OxygenTimeLeft = telemetryDataHandler.GetOxyTimeLeft(ev1Key);
        string ev1FormattedTime = FormatTime(ev1OxygenTimeLeft);
        ev1OxygenTimeLeftTextMeshPro.text = ev1FormattedTime;

        // Update EV2 oxygen time left
        string ev2Key = "eva2";
        float ev2OxygenTimeLeft = telemetryDataHandler.GetOxyTimeLeft(ev2Key);
        string ev2FormattedTime = FormatTime(ev2OxygenTimeLeft);
        ev2OxygenTimeLeftTextMeshPro.text = ev2FormattedTime;
    }

    private static string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }
}