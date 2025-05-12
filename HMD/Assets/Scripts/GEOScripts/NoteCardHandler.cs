using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class NoteCardInformation : MonoBehaviour
{
    // Reference to the single TextMeshPro component on the NoteCard
    public TextMeshProUGUI cardInfoText;
    
    // Reference to the science indicator (exclamation mark)
    public GameObject scienceIndicator;
    
    // Reference to the associated field note GameObject
    private GameObject associatedFieldNote;
    
    // Method to set the card data
    public void SetCardData(string dateTime, string location, string sampleName, string rockType, GameObject fieldNote, bool isScientificallySignificant)
    {
        // Store the reference to the field note
        associatedFieldNote = fieldNote;
        
        if (cardInfoText != null)
        {
            // Parse the date and time from the dateTime string
            string date = "";
            string time = "";
            
            if (!string.IsNullOrEmpty(dateTime))
            {
                try
                {
                    DateTime dt = DateTime.Parse(dateTime);
                    date = dt.ToString("yyyy-MM-dd");
                    time = dt.ToString("HH:mm:ss");
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing dateTime: " + e.Message);
                    date = "Error";
                    time = "Error";
                }
            }
            
            // Format the text according to the specified format
            string cardText = $"Name: {sampleName}\nType: {rockType}\n\n";
            cardText += $"Date: {date}\n";
            cardText += $"Time: {time}\n";
            cardText += $"Location: {location}";
            
            // Set the text on the TextMeshPro component
            cardInfoText.text = cardText;

            // Set the science indicator visibility based on scientific significance
            if (scienceIndicator != null)
            {
                scienceIndicator.SetActive(isScientificallySignificant);
            }
            else
            {
                Debug.LogWarning("Science Indicator reference is not set on NoteCardInformation component");
            }
        }
        else
        {
            Debug.LogError("Card Info Text reference is not set on NoteCardInformation component");
        }
    }
    
    // Method to toggle the field note visibility
    public void ToggleFieldNoteVisibility()
    {
        if (associatedFieldNote != null)
        {
            // Toggle the field note's active state to make it visible
            associatedFieldNote.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No associated field note found for this note card");
        }
    }
}