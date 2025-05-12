using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class POIDataManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText; // Reference to your text object
    [SerializeField] private string serverUrl = "http://3.145.48.180:7070/poi";
    
    [Serializable]
    public class POI
    {
        public string name;
        public float x;
        public float y;
        public string[] tags;
        public string description;
    }
    
    [Serializable]
    public class POIListWrapper
    {
        public List<POI> items;
    }
    
    void Start()
    {
        // Get all POIs when the script starts
        StartCoroutine(GetAllPOIs());
    }
    
    public IEnumerator GetAllPOIs()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(serverUrl))
        {
            // Send the request and wait for response
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
                displayText.text = "Error loading POIs: " + webRequest.error;
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Raw JSON response: " + jsonResponse);
                
                // Parse JSON response
                try {
                    // Unity's JsonUtility can't directly deserialize JSON arrays, so we need a workaround
                    // Method 1: Manual parsing to handle array directly
                    List<POI> poiList = new List<POI>();
                    
                    // Check if response starts with [ which indicates it's an array
                    if (jsonResponse.TrimStart().StartsWith("["))
                    {
                        // Convert array to wrapper object
                        string wrappedJson = "{\"items\":" + jsonResponse + "}";
                        POIListWrapper wrapper = JsonUtility.FromJson<POIListWrapper>(wrappedJson);
                        poiList = wrapper.items;
                    }
                    else if (jsonResponse.TrimStart().StartsWith("{"))
                    {
                        // If it's a single object
                        POI singlePoi = JsonUtility.FromJson<POI>(jsonResponse);
                        poiList.Add(singlePoi);
                    }
                    
                    // Display POI information in the text object
                    DisplayPOIs(poiList);
                }
                catch (Exception e) {
                    Debug.LogError("JSON parsing error: " + e.Message);
                    displayText.text = "Error parsing data. Check console for details.";
                }
            }
        }
    }
    
    private void DisplayPOIs(List<POI> pois)
    {
        if (pois == null || pois.Count == 0)
        {
            displayText.text = "No POIs found";
            return;
        }
        
        string displayContent = "Points of Interest:\n\n";
        
        foreach (POI poi in pois)
        {
            displayContent += $"• {poi.name}\n";
            displayContent += $"  Location: ({poi.x}, {poi.y})\n";
            displayContent += $"  Description: {poi.description}\n\n";
        }
        
        displayText.text = displayContent;
    }
    
    // Method to create a new POI
    public void CreateNewPOI(string name, float x, float y, string description)
    {
        StartCoroutine(CreatePOI(name, x, y, description));
    }
    
    public IEnumerator CreatePOI(string name, float x, float y, string description)
    {
        POI newPoi = new POI
        {
            name = name,
            x = x,
            y = y,
            tags = new string[0], // Empty array as requested
            description = description
        };
        
        string jsonData = JsonUtility.ToJson(newPoi);
        
        using (UnityWebRequest webRequest = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error creating POI: " + webRequest.error);
            }
            else
            {
                Debug.Log("POI created successfully!");
                // Refresh the POI list
                StartCoroutine(GetAllPOIs());
            }
        }
    }
    
    // Method for test1 button functionality
    public void OnTest1ButtonClicked()
    {
        // Call method with specified parameters
        CreateNewPOI("ecir stius 123&%$^@#$", 12314523475f, 0.0900134f, "don't read this message");
    }
}