using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using UnityEngine.Networking;
using System.Text;

// Service for interacting with the POI and Audio API backend
public class ApiService : MonoBehaviour
{
        [Header("Auto Sync Settings")]
    [SerializeField] private bool autoSyncEnabled = false;
    [SerializeField] private float syncInterval = 1.0f; // Time in seconds between sync operations
    
    private bool isSyncing = false;
    private Coroutine autoSyncCoroutine;
    
    private void Start()
    {
        // Start auto-sync if enabled
        if (autoSyncEnabled)
        {
            StartAutoSync();
        }
    }
    
    // Public method to enable auto-sync
    public void StartAutoSync()
    {
        if (autoSyncCoroutine != null)
        {
            StopCoroutine(autoSyncCoroutine);
        }
        
        autoSyncEnabled = true;
        autoSyncCoroutine = StartCoroutine(AutoSyncCoroutine());
        Debug.Log("Auto sync started");
    }
    
    // Public method to disable auto-sync
    public void StopAutoSync()
    {
        if (autoSyncCoroutine != null)
        {
            StopCoroutine(autoSyncCoroutine);
            autoSyncCoroutine = null;
        }
        
        autoSyncEnabled = false;
        Debug.Log("Auto sync stopped");
    }
    
    // Coroutine that handles automatic syncing
    private IEnumerator AutoSyncCoroutine()
    {
        while (autoSyncEnabled)
        {
            // Only start a new sync if we're not already syncing
            if (!isSyncing)
            {
                StartCoroutine(SyncPOIsCoroutine());
            }
            
            // Wait for the specified interval
            yield return new WaitForSeconds(syncInterval);
        }
    }
    
    // Modify the SyncPOIsCoroutine to track when it's running
    private IEnumerator SyncPOIsCoroutine()
    {
        // Set the flag to indicate we're syncing
        isSyncing = true;
        
        Debug.Log("Starting POI synchronization...");
        
        // Call RetrievePOIs to get all POIs from the server
        bool syncComplete = false;
        List<PinData> serverPOIs = null;
        
        StartCoroutine(RetrievePOIs((retrievedPOIs) => {
            serverPOIs = retrievedPOIs;
            syncComplete = true;
        }));
        
        // Wait for the POI retrieval to complete
        while (!syncComplete)
        {
            yield return null;
        }
        
        Debug.Log($"Retrieved {serverPOIs.Count} POIs from server");
        
        // For each POI from the server, check if it exists in the local registry
        int addedCount = 0;
        foreach (PinData serverPOI in serverPOIs)
        {
            bool existsLocally = false;
            
            // Check if this POI already exists in the local registry
            foreach (PinData localPOI in PinRegistry.Pins)
            {
                // Check if coordinates and name match
                float distanceThreshold = 0.01f; // Adjust this tolerance as needed
                bool samePosition = Vector2.Distance(new Vector2(localPOI.x, localPOI.y), 
                                                   new Vector2(serverPOI.x, serverPOI.y)) < distanceThreshold;
                bool sameName = localPOI.name == serverPOI.name;
                
                if (samePosition && sameName)
                {
                    existsLocally = true;
                    break;
                }
            }
            
            // If POI doesn't exist locally, add it to the registry
            if (!existsLocally)
            {
                // Add to registry but don't send to server again
                PinRegistry.Pins.Add(serverPOI);
                addedCount++;
                
                Debug.Log($"Added new POI from server: {serverPOI.name} at ({serverPOI.x}, {serverPOI.y})");
            }
        }
        
        Debug.Log($"Sync complete. Added {addedCount} new POIs to local registry.");
        
        // Reset the flag to indicate we've finished syncing
        isSyncing = false;
    }
    
    [SerializeField]
    private string baseUrl = "http://3.144.242.35:7070";
    
    // For debugging API issues
    private void PrintEndpointUrl(string endpoint)
    {
        Debug.Log("API URL: " + baseUrl + endpoint);
    }

    // Uploads an audio file to the backend
    public IEnumerator PostAudioFile(AudioClip audioClip, string fileName, Action<int> onComplete)
    {
        // Log detailed information about the audio clip
        Debug.Log("Audio clip info - Name: " + audioClip.name + 
                  ", Length: " + audioClip.length + " seconds, " +
                  "Channels: " + audioClip.channels + 
                  ", Frequency: " + audioClip.frequency + " Hz");
        
        // Convert AudioClip to WAV
        byte[] audioData = WavUtility.FromAudioClip(audioClip);
        Debug.Log("Converted WAV size: " + audioData.Length + " bytes");
        
        // Create form data for file upload
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", audioData, fileName + ".wav", "audio/wav");
        
        // Create request
        string url = $"{baseUrl}/audio";
        Debug.Log("Sending POST request to: " + url);
        
        // Create and send the request
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.timeout = 30; // Set a longer timeout (30 seconds)
            Debug.Log("Starting request - Timeout set to: " + www.timeout + " seconds");
            
            // Send request
            yield return www.SendWebRequest();

            // Detailed error logging
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Audio upload failed with result: " + www.result);
                Debug.LogError("Error message: " + www.error);
                Debug.LogError("Response code: " + www.responseCode);
                Debug.LogError("URL attempted: " + url);
                
                // Try to get response data if available
                if (www.downloadHandler != null && www.downloadHandler.data != null)
                {
                    try
                    {
                        string errorResponse = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                        Debug.LogError("Error response body: " + errorResponse);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Could not read error response body: " + e.Message);
                    }
                }
                
                // Try to get request data
                if (www.uploadHandler != null)
                {
                    Debug.Log("Upload handler content type: " + www.uploadHandler.contentType);
                }
            }
            else
            {
                // Parse response to get audio ID
                string responseText = www.downloadHandler.text;
                Debug.Log("Audio upload response: " + responseText);
                
                try
                {
                    // Parse JSON response to get the audio ID
                    AudioUploadResponse response = JsonUtility.FromJson<AudioUploadResponse>(responseText);
                    Debug.Log("Successfully parsed response. ID: " + response.id);
                    onComplete?.Invoke(response.id);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing response JSON: " + e.Message);
                    Debug.LogError("Raw response was: " + responseText);
                }
            }
        }
    }

    // Posts a POI to the backend
    public IEnumerator PostPOI(PinData pinData)
    {
        // Convert PinData to API format
        POIRequest poiRequest = new POIRequest
        {
            name = pinData.name,
            x = pinData.x,
            y = pinData.y,
            tags = pinData.tags,
            description = pinData.description,
            type = pinData.type,
            audioId = pinData.audioId
        };

        // Convert to JSON
        string jsonData = JsonUtility.ToJson(poiRequest);
        
        // Create request
        string url = $"{baseUrl}/poi";
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // Send request
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("POI upload error: " + www.error);
            }
            else
            {
                Debug.Log("POI upload success: " + www.downloadHandler.text);
            }
        }
    }

    // Retrieves all POIs from the backend and loads associated audio clips
    public IEnumerator RetrievePOIs(Action<List<PinData>> onComplete)
    {
        string url = $"{baseUrl}/poi";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            // Send request
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("POI retrieval error: " + www.error);
            }
            else
            {
                // Parse response to get POIs
                string responseText = www.downloadHandler.text;
                Debug.Log("POI retrieval response: " + responseText);
                
                try
                {
                    // Wrap the JSON array with a container object since JsonUtility
                    // doesn't directly support deserializing JSON arrays
                    string wrappedJson = "{ \"items\": " + responseText + "}";
                    POIListWrapper poiListWrapper = JsonUtility.FromJson<POIListWrapper>(wrappedJson);
                    
                    // Convert to PinData objects
                    List<PinData> pinDataList = new List<PinData>();
                    
                    // Track how many audio retrievals are in progress
                    int pendingAudioRetrievals = 0;
                    
                    foreach (POIResponse poiResponse in poiListWrapper.items)
                    {
                        // Check if this POI has audio
                        if (poiResponse.audioId == null)
                        {
                            // No audio, create PinData without audio clip
                            PinData pinData = new PinData(
                                poiResponse.x, 
                                poiResponse.y,
                                poiResponse.name,
                                poiResponse.tags,
                                poiResponse.description,
                                poiResponse.type,
                                poiResponse.audioId,
                                null
                            );
                            pinDataList.Add(pinData);
                        }
                        else
                        {
                            // Has audio, need to retrieve it
                            pendingAudioRetrievals++;
                            
                            // Create a local copy of the POI data for the closure
                            POIResponse localPoiResponse = poiResponse;
                            
                            // Start audio retrieval
                            StartCoroutine(RetrieveAudio(poiResponse.audioId, 
                                (audioClip) => {
                                    // Create PinData with the retrieved audio clip
                                    PinData pinData = new PinData(
                                        localPoiResponse.x, 
                                        localPoiResponse.y,
                                        localPoiResponse.name,
                                        localPoiResponse.tags,
                                        localPoiResponse.description,
                                        localPoiResponse.type,
                                        localPoiResponse.audioId,
                                        audioClip
                                    );
                                    pinDataList.Add(pinData);
                                    
                                    // Decrease the counter
                                    pendingAudioRetrievals--;
                                    
                                    // If all audio retrievals are done, return the list
                                    if (pendingAudioRetrievals == 0)
                                    {
                                        onComplete?.Invoke(pinDataList);
                                    }
                                }
                            ));
                        }
                    }
                    
                    // If no audio retrievals were started or all POIs had audioId = 0
                    if (pendingAudioRetrievals == 0)
                    {
                        onComplete?.Invoke(pinDataList);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to parse POI response: " + e.Message);
                }
            }
        }
    }

    // Retrieves an audio file by ID
    public IEnumerator RetrieveAudio(int audioId, Action<AudioClip> onComplete)
    {
        string url = $"{baseUrl}/audio/{audioId}";
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            // Send request
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Audio retrieval error: " + www.error);
                onComplete?.Invoke(null);
            }
            else
            {
                // Get the audio clip
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                Debug.Log("Audio retrieved successfully: " + clip.name);
                onComplete?.Invoke(clip);
            }
        }
    }
}

// Helper classes for JSON serialization/deserialization
[Serializable]
public class AudioUploadResponse
{
    public string filename;
    public int id;
}

[Serializable]
public class POIRequest
{
    public string name;
    public float x;
    public float y;
    public string[] tags;
    public string description;
    public string type;
    public int audioId;
}

[Serializable]
public class POIResponse
{
    public int id;
    public string name;
    public float x;
    public float y;
    public string[] tags;
    public string description;
    public string type;
    public int audioId;
}

[Serializable]
public class POIListWrapper
{
    public POIResponse[] items;
}

// Simple utility to convert AudioClip to WAV data
public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        using (var memoryStream = new System.IO.MemoryStream(44 + samples.Length * 2))
        {
            var writer = new System.IO.BinaryWriter(memoryStream);
            
            // RIFF header
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + samples.Length * 2); // File size
            writer.Write("WAVE".ToCharArray());

            // Format chunk
            writer.Write("fmt ".ToCharArray());
            writer.Write(16); // Chunk size
            writer.Write((ushort)1); // Audio format (1 = PCM)
            writer.Write((ushort)clip.channels); // Channels
            writer.Write(clip.frequency); // Sample rate
            writer.Write(clip.frequency * clip.channels * 2); // Byte rate
            writer.Write((ushort)(clip.channels * 2)); // Block align
            writer.Write((ushort)16); // Bits per sample

            // Data chunk
            writer.Write("data".ToCharArray());
            writer.Write(samples.Length * 2); // Chunk size
            
            // Convert and write sample data
            foreach (var sample in samples)
            {
                writer.Write((short)(sample * short.MaxValue));
            }

            return memoryStream.ToArray();
        }
    }
}