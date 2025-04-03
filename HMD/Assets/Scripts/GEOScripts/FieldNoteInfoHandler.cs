using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class FieldNoteInfoHandler : MonoBehaviour
{
    // Reference to the IMUDataHandler script to access location data
    public IMUDataHandler imuDataHandler;
    
    // Reference to the TELEMETRYDataHandler script to access EVA time
    public TELEMETRYDataHandler telemetryDataHandler;
    
    // Reference to the EVANumberHandler script to know which EVA data to display
    public EVANumberHandler evaNumberHandler;
    
    // Reference to the TSScConnection script to check for SPEC updates
    public TSScConnection tsscConnection;
    
    // Reference to the single TextMeshPro field that contains date, time, and location info
    public TextMeshProUGUI dateTimeLocationText;
    
    // State tracking for the update cycle
    private enum UpdateState { Waiting, UpdateDetected, UpdateComplete }
    private UpdateState currentState = UpdateState.Waiting;
    
    void Start()
    {
        // Verify references are set
        if (imuDataHandler == null)
        {
            Debug.LogError("IMUDataHandler reference is not set. Please assign it in the inspector.");
        }
        
        if (telemetryDataHandler == null)
        {
            Debug.LogError("TELEMETRYDataHandler reference is not set. Please assign it in the inspector.");
        }
        
        if (evaNumberHandler == null)
        {
            Debug.LogError("EVANumberHandler reference is not set. Please assign it in the inspector.");
        }
        
        if (tsscConnection == null)
        {
            Debug.LogError("TSScConnection reference is not set. Please assign it in the inspector.");
        }
        
        if (dateTimeLocationText == null)
        {
            Debug.LogError("TextMeshProUGUI reference is not set. Please assign the date/time/location text field in the inspector.");
        }
    }
    
    void Update()
    {
        // Skip if any references are missing
        if (tsscConnection == null || imuDataHandler == null || telemetryDataHandler == null)
        {
            return;
        }
        
        // State machine to handle the update cycle
        switch (currentState)
        {
            case UpdateState.Waiting:
                // Wait for update to be detected
                if (tsscConnection.isSPECUpdated())
                {
                    currentState = UpdateState.UpdateDetected;
                    Debug.Log("SPEC update detected in FieldNoteInfoHandler, waiting for processing...");
                }
                break;
                
            case UpdateState.UpdateDetected:
                // Wait for update to be processed (flag becomes false)
                if (!tsscConnection.isSPECUpdated())
                {
                    currentState = UpdateState.UpdateComplete;
                    Debug.Log("SPEC update processed in FieldNoteInfoHandler, updating date/time/location...");
                }
                break;
                
            case UpdateState.UpdateComplete:
                // Update our display and reset state
                UpdateFieldNoteInfo();
                currentState = UpdateState.Waiting;
                Debug.Log("Date/time/location updated, waiting for next update...");
                break;
        }
    }
    
    public void UpdateFieldNoteInfo()
    {
        if (imuDataHandler == null || telemetryDataHandler == null || evaNumberHandler == null || dateTimeLocationText == null)
        {
            return;
        }
        
        try
        {
            // Get the current date
            DateTime currentDate = DateTime.Now;
            string dateString = "date: " + currentDate.ToString("yyyy-MM-dd");
            
            // Get the current EVA number and convert to eva1/eva2 format
            int evaNumber = evaNumberHandler.getEVANumber();
            string evaKey = "eva" + evaNumber;
            
            // Get mission time from telemetry data using the new method
            float evaTimeSeconds = telemetryDataHandler.GetEVATime();
            
            // Convert seconds to TimeSpan for proper formatting
            TimeSpan missionTime = TimeSpan.FromSeconds(evaTimeSeconds);
            
            // Format mission time as hours:minutes:seconds
            string timeString = "time: " + string.Format("{0:D2}:{1:D2}:{2:D2}", 
                (int)missionTime.TotalHours, // Get total hours (not just hours component)
                missionTime.Minutes, 
                missionTime.Seconds);
            
            // Default location string
            string locationString = "location: ";
            
            // Check if we have valid EVA number
            if (evaNumber != 1 && evaNumber != 2)
            {
                locationString += "Select EVA 1 or 2";
            }
            else
            {
                // Get location data from IMUDataHandler
                float posX = imuDataHandler.GetPosx(evaKey);
                float posY = imuDataHandler.GetPosy(evaKey);
                float heading = imuDataHandler.GetHeading(evaKey);
                
                // Format location data
                locationString += $"X:{posX:F1} Y:{posY:F1} H:{heading:F0}°";
            }
            
            // Combine all three lines into one string
            string combinedText = dateString + "\n" + timeString + "\n" + locationString;
            
            // Update the text
            dateTimeLocationText.text = combinedText;
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting field note info: " + e.Message);
            dateTimeLocationText.text = "date: Error\ntime: Error\nlocation: Error";
        }
    }
}