using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class DataDisplayUpdater : MonoBehaviour
{
    // Reference to the TextMeshPro component
    public TextMeshPro dataText;
    
    // Reference to the TSScDataHandler
    public TSScDataHandler dataHandler;
    
    // Update interval (in seconds)
    public float updateInterval = 1.0f;
    private float timeSinceLastUpdate = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        if (dataText == null)
        {
            Debug.LogError("Data Text reference not set. Please assign the TextMeshPro component.");
        }
        
        if (dataHandler == null)
        {
            Debug.LogError("Data Handler reference not set. Please assign the TSScDataHandler component.");
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        
        if (timeSinceLastUpdate >= updateInterval)
        {
            UpdateDataDisplay();
            timeSinceLastUpdate = 0f;
        }
    }
    
    void UpdateDataDisplay()
    {
        if (dataHandler == null || dataText == null) return;
        
        StringBuilder sb = new StringBuilder();
        
        // Add a header
        sb.AppendLine("<b>TSS DATA MONITOR</b>");
        sb.AppendLine("---------------------------");
        
        // Connection status
        bool isConnected = dataHandler.TSSc.IsConnected();
        sb.AppendLine($"Connection: {(isConnected ? "<color=green>CONNECTED</color>" : "<color=red>DISCONNECTED</color>")}");
        sb.AppendLine();
        
        // Add each data type with truncated display
        AddDataSection(sb, "IMU", dataHandler.GetIMUData());
        AddDataSection(sb, "UIA", dataHandler.GetUIAData());
        AddDataSection(sb, "DCU", dataHandler.GetDCUData());
        AddDataSection(sb, "ROVER", dataHandler.GetROVERData());
        AddDataSection(sb, "SPEC", dataHandler.GetSPECData());
        AddDataSection(sb, "TELEMETRY", dataHandler.GetTELEMETRYData());
        AddDataSection(sb, "COMM", dataHandler.GetCOMMData());
        AddDataSection(sb, "ROVER_TELEMETRY", dataHandler.GetROVER_TELEMETRYData());
        AddDataSection(sb, "ERROR", dataHandler.GetERRORData());
        AddDataSection(sb, "EVA", dataHandler.GetEVAData());
        
        // Update the text
        dataText.text = sb.ToString();
    }
    
    // Helper method to add a section for each data type
    void AddDataSection(StringBuilder sb, string title, string jsonData)
    {
        sb.AppendLine($"<b>{title}:</b>");
        
        if (string.IsNullOrEmpty(jsonData))
        {
            sb.AppendLine("  <color=yellow>No data available</color>");
        }
        else
        {
            // Truncate long JSON for display
            string displayData = jsonData;
            if (displayData.Length > 100)
            {
                displayData = displayData.Substring(0, 100) + "...";
            }
            sb.AppendLine($"  {displayData}");
        }
        
        sb.AppendLine();
    }
}