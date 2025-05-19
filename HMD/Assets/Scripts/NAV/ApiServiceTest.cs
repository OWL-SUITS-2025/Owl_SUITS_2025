using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class ApiServiceTest : MonoBehaviour
{
    [SerializeField]
    private ApiService apiService;
    
    [SerializeField]
    private TextMeshPro statusText;
    
    // Button to start the test
    [SerializeField]
    private PressableButton testButton;
    
    // Flag to prevent multiple test runs at once
    private bool isTestRunning = false;
    
    private void Start()
    {
        if (testButton != null)
        {
            testButton.OnClicked.AddListener(StartTest);
        }
        
        // Make sure we have an ApiService instance
        if (apiService == null)
        {
            apiService = FindObjectOfType<ApiService>();
            if (apiService == null)
            {
                Debug.LogError("ApiService not found! Please assign it in the inspector.");
                UpdateStatus("ERROR: ApiService not found!");
            }
        }
    }
    
    public void StartTest()
    {
        if (isTestRunning)
        {
            Debug.Log("Test already running. Please wait.");
            return;
        }
        
        isTestRunning = true;
        UpdateStatus("Starting API Test...");
        StartCoroutine(RunApiTest());
    }
    
    private IEnumerator RunApiTest()
    {
        // Create sample PinData objects with null audioId and audioClip
        List<PinData> testPins = CreateTestPins();
        
        // Test PostPOI
        yield return StartCoroutine(TestPostPOI(testPins));
        
        // Wait a bit to ensure server has processed the data
        UpdateStatus("Waiting for server processing...");
        yield return new WaitForSeconds(2f);
        
        // Test RetrievePOIs
        yield return StartCoroutine(TestRetrievePOIs());
        
        // Test complete
        UpdateStatus("API Test Complete!");
        isTestRunning = false;
    }
    
    private List<PinData> CreateTestPins()
    {
        UpdateStatus("Creating test pins...");
        
        List<PinData> pins = new List<PinData>();
        
        // Create 3 test pins with different data but null audioId and null audioClip
        pins.Add(new PinData(
            1.5f, 2.3f,
            "Test Pin 1",
            new string[0],
            "This is a test pin for API testing",
            "TestPin",
            0,    // Using 0 for audioId to represent null in the API
            null  // Null audioClip
        ));
        
        pins.Add(new PinData(
            -3.2f, 4.7f,
            "Test Pin 2",
            new string[0],
            "Another test pin for API verification",
            "TestPin",
            0,    // Using 0 for audioId to represent null in the API
            null  // Null audioClip
        ));
        
        pins.Add(new PinData(
            0.0f, -1.8f,
            "Test Pin 3",
            new string[0],
            "Third test pin with null audio data",
            "TestPin",
            0,    // Using 0 for audioId to represent null in the API
            null  // Null audioClip
        ));
        
        Debug.Log($"Created {pins.Count} test pins");
        return pins;
    }
    
    private IEnumerator TestPostPOI(List<PinData> pins)
    {
        int pinCount = 0;
        foreach (var pin in pins)
        {
            pinCount++;
            UpdateStatus($"Posting test pin {pinCount}/{pins.Count}...");
            
            yield return StartCoroutine(apiService.PostPOI(pin));
            
            // Log the result
            Debug.Log($"Posted test pin {pinCount}: {pin.name}");
        }
        
        UpdateStatus($"Posted {pins.Count} test pins");
    }
    
    private IEnumerator TestRetrievePOIs()
    {
        UpdateStatus("Retrieving POIs from server...");
        
        bool retrieveComplete = false;
        List<PinData> retrievedPins = null;
        
        yield return StartCoroutine(apiService.RetrievePOIs((pins) => {
            retrievedPins = pins;
            retrieveComplete = true;
        }));
        
        // Wait until retrieval is complete
        while (!retrieveComplete)
        {
            yield return null;
        }
        
        // Process results
        if (retrievedPins != null)
        {
            Debug.Log($"Retrieved {retrievedPins.Count} pins from server");
            UpdateStatus($"Retrieved {retrievedPins.Count} pins");
            
            // Log the details of each retrieved pin
            foreach (var pin in retrievedPins)
            {
                string audioInfo = pin.audioId > 0 ? $"audioId: {pin.audioId}" : "No audio";
                Debug.Log($"Pin: {pin.name}, Position: ({pin.x}, {pin.y}), {audioInfo}");
            }
        }
        else
        {
            Debug.LogError("Failed to retrieve pins from server");
            UpdateStatus("Error retrieving pins!");
        }
    }
    
    private void UpdateStatus(string message)
    {
        Debug.Log("Test Status: " + message);
        
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}