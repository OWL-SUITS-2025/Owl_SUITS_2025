using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SpecResultsHandler : MonoBehaviour
{
    // Reference to the SPECDataHandler script to access SPEC data
    public SPECDataHandler specDataHandler;
    
    // Reference to the TSScConnection script to check for updates
    public TSScConnection tsscConnection;
    
    // Reference to the EVANumberHandler script to know which EVA data to display
    public EVANumberHandler evaNumberHandler;
    
    // Reference to the Text component in the Spec Results Placeholder
    public TextMeshProUGUI specResultsText;
    
    // State tracking for the update cycle
    private enum UpdateState { Waiting, UpdateDetected, UpdateComplete }
    private UpdateState currentState = UpdateState.Waiting;
    
    void Start()
    {
        // Verify references are set
        if (specDataHandler == null)
        {
            Debug.LogError("SPECDataHandler reference is not set. Please assign it in the inspector.");
        }
        
        if (tsscConnection == null)
        {
            Debug.LogError("TSScConnection reference is not set. Please assign it in the inspector.");
        }
        
        if (evaNumberHandler == null)
        {
            Debug.LogError("EVANumberHandler reference is not set. Please assign it in the inspector.");
        }
        
        if (specResultsText == null)
        {
            Debug.LogError("SpecResultsText reference is not set. Please assign the TextMeshProUGUI component in the Spec Results Placeholder.");
        }
    }
    
    void Update()
    {
        // Skip if any references are missing
        if (tsscConnection == null || specDataHandler == null)
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
                    Debug.Log("SPEC update detected, waiting for processing...");
                }
                break;
                
            case UpdateState.UpdateDetected:
                // Wait for update to be processed (flag becomes false)
                if (!tsscConnection.isSPECUpdated())
                {
                    currentState = UpdateState.UpdateComplete;
                    Debug.Log("SPEC update processed, updating UI...");
                }
                break;
                
            case UpdateState.UpdateComplete:
                // Update our display and reset state
                UpdateSpecResults();
                currentState = UpdateState.Waiting;
                Debug.Log("UI updated, waiting for next update...");
                break;
        }
    }
    
    public void UpdateSpecResults()
    {
        if (specDataHandler == null || evaNumberHandler == null || specResultsText == null)
        {
            return;
        }
        
        try
        {
            // Get the current EVA number and convert to eva1/eva2 format
            int evaNumber = evaNumberHandler.getEVANumber();
            string evaKey = "eva" + evaNumber;
            
            // Check if we have valid EVA number
            if (evaNumber != 1 && evaNumber != 2)
            {
                specResultsText.text = "Please select EVA 1 or EVA 2";
                return;
            }
            
            // Get sample name and ID
            string rockName = specDataHandler.GetName(evaKey);
            float rockId = specDataHandler.GetId(evaKey);
            
            // If we couldn't get the data, display an error
            if (string.IsNullOrEmpty(rockName))
            {
                specResultsText.text = "No spectrometry data available for EVA " + evaNumber;
                return;
            }
            
            // Build string for text display
            string results = $"Sample Name: {rockName}\n";
            results += $"Sample ID: {rockId}\n\n";
            results += "Mineral Composition:\n";
            
            // Create two columns of mineral data
            string[] compounds = new string[] { "SiO2", "TiO2", "Al2O3", "FeO", "MnO", "MgO", "CaO", "K2O", "P2O3", "other" };
            
            // Split into two arrays for the two columns
            int halfLength = compounds.Length / 2 + compounds.Length % 2; // Ceiling division
            string[] leftColumnCompounds = new string[halfLength];
            string[] rightColumnCompounds = new string[compounds.Length - halfLength];
            
            // Populate the column arrays
            for (int i = 0; i < halfLength; i++)
            {
                leftColumnCompounds[i] = compounds[i];
            }
            
            for (int i = 0; i < compounds.Length - halfLength; i++)
            {
                rightColumnCompounds[i] = compounds[i + halfLength];
            }
            
            // Format each row with the two columns
            for (int i = 0; i < halfLength; i++)
            {
                string leftColumn = $"{leftColumnCompounds[i]}: {specDataHandler.GetCompoundData(evaKey, leftColumnCompounds[i]):F2}%";
                
                // Add padding to make columns align
                leftColumn = leftColumn.PadRight(16);
                
                // Add right column if available
                if (i < rightColumnCompounds.Length)
                {
                    string rightColumn = $"{rightColumnCompounds[i]}: {specDataHandler.GetCompoundData(evaKey, rightColumnCompounds[i]):F2}%";
                    results += leftColumn + rightColumn + "\n";
                }
                else
                {
                    results += leftColumn + "\n";
                }
            }
            
            // Update the text component
            specResultsText.text = results;
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting spectrometry data: " + e.Message);
            specResultsText.text = "Error getting spectrometry data";
        }
    }
}