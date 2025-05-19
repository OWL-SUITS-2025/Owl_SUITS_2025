using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

// Test script for the ApiService functions
public class ApiServiceTester : MonoBehaviour
{
    // Reference to the ApiService
    [SerializeField]
    private ApiService apiService;
    
    // Test audio clip to upload
    [SerializeField]
    private AudioClip testAudioClip;
    
    // Audio source to play retrieved audio
    [SerializeField]
    private AudioSource audioSource;
    
    // Text components to display test results
    [SerializeField]
    private TextMeshProUGUI statusText;
    
    // MRTK Buttons for testing
    [SerializeField]
    private PressableButton recordAudioButton;
    
    [SerializeField]
    private PressableButton playRecordedButton;
    
    [SerializeField]
    private PressableButton uploadAudioButton;
    
    [SerializeField]
    private PressableButton uploadPoiButton;
    
    [SerializeField]
    private PressableButton retrievePoisButton;
    
    [SerializeField]
    private PressableButton retrieveAudioButton;
    
    // Store the last uploaded audio ID for testing
    private int lastAudioId = -1;
    
    // Store the list of retrieved POIs
    private List<PinData> retrievedPins = new List<PinData>();
    
    // For recording audio
    private bool isRecording = false;
    private float recordingTime = 5f; // 5 seconds recording
    private AudioClip recordedClip;
    
    // Start is called before the first frame update
    void Start()
    {
        // Find the ApiService if not set
        if (apiService == null)
        {
            apiService = FindObjectOfType<ApiService>();
        }
        
        // Make sure we have a status text
        if (statusText == null)
        {
            Debug.LogWarning("Status text not set - won't see test results");
        }
        
        // Set up button events
        if (recordAudioButton != null)
        {
            recordAudioButton.OnClicked.AddListener(StartRecording);
        }
        
        if (playRecordedButton != null)
        {
            playRecordedButton.OnClicked.AddListener(PlayRecordedAudio);
        }
        
        if (uploadAudioButton != null)
        {
            uploadAudioButton.OnClicked.AddListener(TestUploadAudio);
        }
        
        if (uploadPoiButton != null)
        {
            uploadPoiButton.OnClicked.AddListener(TestUploadPoi);
        }
        
        if (retrievePoisButton != null)
        {
            retrievePoisButton.OnClicked.AddListener(TestRetrievePois);
        }
        
        if (retrieveAudioButton != null)
        {
            retrieveAudioButton.OnClicked.AddListener(TestRetrieveAudio);
        }
        
        UpdateStatus("Ready for testing. Press a button to test each function.");
    }
    
    // Update the status text
    private void UpdateStatus(string message)
    {
        Debug.Log("API Test: " + message);
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
    
    // Start recording audio
    public void StartRecording()
    {
        if (isRecording)
        {
            UpdateStatus("Already recording!");
            return;
        }
        
        // Check for microphone
        if (Microphone.devices.Length == 0)
        {
            UpdateStatus("Error: No microphone found!");
            return;
        }
        
        UpdateStatus("Recording for 5 seconds...");
        
        // Start recording
        isRecording = true;
        recordedClip = Microphone.Start(null, false, (int)recordingTime, 44100);
        
        // Stop recording after the set time
        StartCoroutine(StopRecordingAfterTime());
    }
    
    // Stop recording after the set time
    private IEnumerator StopRecordingAfterTime()
    {
        // Wait for recording to complete
        yield return new WaitForSeconds(recordingTime);
        
        // Stop recording
        Microphone.End(null);
        isRecording = false;
        
        // Set as the test audio clip
        testAudioClip = recordedClip;
        
        UpdateStatus("Recording complete! Ready to play or upload.");
    }
    
    // Play back the recorded audio
    public void PlayRecordedAudio()
    {
        if (testAudioClip == null)
        {
            UpdateStatus("Error: No recorded audio to play. Record audio first.");
            return;
        }
        
        if (audioSource == null)
        {
            UpdateStatus("Error: No audio source available for playback.");
            return;
        }
        
        // Play the recorded audio
        audioSource.clip = testAudioClip;
        audioSource.Play();
        
        UpdateStatus("Playing recorded audio...");
    }
    
    // Test the PostAudioFile function
    public void TestUploadAudio()
    {
        // Make sure we have an audio clip
        if (testAudioClip == null)
        {
            UpdateStatus("Error: No test audio clip available. Record audio first.");
            return;
        }
        
        UpdateStatus("Testing PostAudioFile...");
        
        // Call the API function
        StartCoroutine(apiService.PostAudioFile(testAudioClip, "test_audio", 
            (audioId) => {
                lastAudioId = audioId;
                UpdateStatus("Audio uploaded successfully. ID: " + audioId);
            }
        ));
    }
    
    // Test the PostPOI function
    public void TestUploadPoi()
    {
        // Make sure we have an audio ID from previous test
        if (lastAudioId == -1)
        {
            UpdateStatus("Error: Upload audio first to get an ID");
            return;
        }
        
        UpdateStatus("Testing PostPOI...");
        
        // Create a test pin with empty tags list
        PinData testPin = new PinData(
            1.23f, 4.56f,
            "Test POI",
            new string[0],  // Empty tags list
            "This is a test POI",
            "test-type",
            lastAudioId
        );
        
        // Call the API function
        StartCoroutine(apiService.PostPOI(testPin));
        
        // Since PostPOI doesn't have a callback, we'll update status after a delay
        StartCoroutine(DelayedStatusUpdate("POI upload test completed. Check console for results.", 2f));
    }
    
    // Test the RetrievePOIs function
    public void TestRetrievePois()
    {
        UpdateStatus("Testing RetrievePOIs...");
        
        // Call the API function
        StartCoroutine(apiService.RetrievePOIs(
            (pinDataList) => {
                retrievedPins = pinDataList;
                UpdateStatus("Retrieved " + pinDataList.Count + " POIs");
                
                // Display details of the first POI if available
                if (pinDataList.Count > 0)
                {
                    PinData firstPin = pinDataList[0];
                    string details = "First POI: " + firstPin.name + 
                                     ", Audio ID: " + firstPin.audioId + 
                                     ", Has Audio Clip: " + (firstPin.localAudioClip != null);
                    
                    Debug.Log(details);
                    StartCoroutine(DelayedStatusUpdate(details, 3f));
                }
            }
        ));
    }
    
    // Test the RetrieveAudio function
    public void TestRetrieveAudio()
    {
        // Try to use lastAudioId first
        int audioIdToRetrieve = lastAudioId;
        
        // If we don't have lastAudioId but have retrieved POIs,
        // use the first POI's audio ID if available
        if (audioIdToRetrieve == -1 && retrievedPins.Count > 0 && retrievedPins[0].audioId != 0)
        {
            audioIdToRetrieve = retrievedPins[0].audioId;
        }
        
        // Check if we have a valid audio ID
        if (audioIdToRetrieve <= 0)
        {
            UpdateStatus("Error: No valid audio ID available for testing");
            return;
        }
        
        UpdateStatus("Testing RetrieveAudio with ID: " + audioIdToRetrieve);
        
        // Call the API function
        StartCoroutine(apiService.RetrieveAudio(audioIdToRetrieve, 
            (audioClip) => {
                if (audioClip == null)
                {
                    UpdateStatus("Error: Could not retrieve audio");
                    return;
                }
                
                UpdateStatus("Audio retrieved: " + audioClip.length + " seconds");
                
                // Play the audio if we have an audio source
                if (audioSource != null)
                {
                    audioSource.clip = audioClip;
                    audioSource.Play();
                    UpdateStatus("Playing retrieved audio...");
                }
            }
        ));
    }
    
    // Helper function to update status after a delay
    private IEnumerator DelayedStatusUpdate(string message, float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateStatus(message);
    }
}