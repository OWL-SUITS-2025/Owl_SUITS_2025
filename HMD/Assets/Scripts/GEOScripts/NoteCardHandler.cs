using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using UnityEngine.UI;

public class NoteCardInformation : MonoBehaviour
{
    // Reference to the single TextMeshPro component on the NoteCard
    public TextMeshProUGUI cardInfoText;
    
    // Reference to the science indicator (exclamation mark)
    public GameObject scienceIndicator;
    
    // Reference to the associated field note GameObject
    private GameObject associatedFieldNote;
    
    // Reference to the image display
    public Image sampleImageDisplay;
    
    // Method to set the card data
    public void SetCardData(string dateTime, string location, string sampleName, string rockType, GameObject fieldNote, bool isScientificallySignificant, float evaTimeSeconds = 0, Texture2D sampleImage = null)
    {
        // Store the reference to the field note
        associatedFieldNote = fieldNote;
        
        if (cardInfoText != null)
        {
            // Parse the date from the dateTime string
            string date = "";
            
            if (!string.IsNullOrEmpty(dateTime))
            {
                try
                {
                    DateTime dt = DateTime.Parse(dateTime);
                    date = dt.ToString("yyyy-MM-dd");
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing dateTime: " + e.Message);
                    date = "Error";
                }
            }
            
            // Format EVA time from seconds to HH:mm:ss
            TimeSpan evaTime = TimeSpan.FromSeconds(evaTimeSeconds);
            string time = string.Format("{0:D2}:{1:D2}:{2:D2}",
                (int)evaTime.TotalHours,  // Get total hours
                evaTime.Minutes,
                evaTime.Seconds);
            
            // Format the text according to the specified format
            string cardText = $"Name: {sampleName}\nType: {rockType}\n\n";
            cardText += $"Date: {date}\n";
            cardText += $"EVA Time: {time}\n";
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
            
            // Display the sample image if provided and we have a reference to the image component
            if (sampleImage != null && sampleImageDisplay != null)
            {
                // Create a sprite from the texture and display it
                Sprite imageSprite = Sprite.Create(sampleImage, 
                                                  new Rect(0, 0, sampleImage.width, sampleImage.height),
                                                  new Vector2(0.5f, 0.5f));
                sampleImageDisplay.sprite = imageSprite;
                sampleImageDisplay.enabled = true;
                Debug.Log("Sample image displayed on note card");
            }
            else if (sampleImageDisplay != null) 
            {
                // Disable the image display if no image was provided
                sampleImageDisplay.enabled = false;
                Debug.Log("No sample image provided for note card");
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