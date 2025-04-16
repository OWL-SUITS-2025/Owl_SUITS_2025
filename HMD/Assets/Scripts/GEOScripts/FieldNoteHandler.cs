using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.UX.Experimental;

public class FieldNoteHandler : MonoBehaviour
{
    // References for Spec Results functionality
    public SPECDataHandler specDataHandler;
    public TextMeshProUGUI specResultsText;
    
    // References for Field Note Info functionality
    public IMUDataHandler imuDataHandler;
    public TELEMETRYDataHandler telemetryDataHandler;
    public TextMeshProUGUI dateTimeLocationText;
    
    // Shared references
    public TSScConnection tsscConnection;
    public EVANumberHandler evaNumberHandler;
    
    // Rock Type buttons
    public PressableButton rockButton;
    public PressableButton regolithButton;

    public PressableButton closeButton;

    // Sliders for physical characteristics
    public Slider grainSizeSlider;
    public Slider sortingSlider;
    public Slider durabilitySlider;
    
    // Edit button reference
    public PressableButton editButton;
    
    // Edit mode state
    private bool isEditModeEnabled = false;
    
    // Flag to track if we just entered edit mode and need to synchronize data
    private bool justEnteredEditMode = false;

    // Complete Button Reference
    public PressableButton completeButton;

    // Reference to the NoteCard prefab
    public GameObject noteCardPrefab;

    // Reference to the VirtualizedScrollRectList component
    public VirtualizedScrollRectList virtualizedScrollRectList;

    // reference itself for the note card
    public GameObject filedNoteObject;

    // List to store note card data during runtime
    private static List<FieldNoteData> noteCards = new List<FieldNoteData>();

    // Add a unique identifier for each handler instance
    private static int nextHandlerId = 0;
    private int handlerId;


    // Variables to store the data
    private string rockType = "";
    private float grainSize = 0f;
    private float sorting = 0f;
    private float durability = 0f;

    // Variable to store complete field note data
    private FieldNoteData currentFieldNoteData;
    
    // Local copies of spec data for comparison
    private string localSampleName = "";
    private float localSampleId = 0f;
    private Dictionary<string, float> localMineralComposition = new Dictionary<string, float>();
    
    // Local copies of field note info for comparison
    private Vector3 localLocation = Vector3.zero;
    private float localEvaTimeSeconds = 0f;
    private string localDateTime = "";

    // Register the note cards
    private bool callbacksRegistered = false;

    // Data structure to hold all field note information
    [System.Serializable]
    public class FieldNoteData
    {
        public string rockType;
        public float grainSize;
        public float sorting;
        public float durability;
        public string sampleName;
        public float sampleId;
        public Dictionary<string, float> mineralComposition;
        public string dateTime;
        public float evaTimeSeconds;
        public Vector3 location; // x, y, heading

        public GameObject fieldNoteObject;
        
        public FieldNoteData()
        {
            mineralComposition = new Dictionary<string, float>();
        }
    }
    
    // Add at the class level before Start():
    void Start()
    {
        // Assign a unique ID to this handler
        handlerId = nextHandlerId++;
        Debug.Log($"FieldNoteHandler {handlerId} initialized");

        // Find the GeoHANDLER GameObject and get the GeoHandler component
        GameObject geoHandlerObj = GameObject.Find("GeoHANDLER");
        if (geoHandlerObj != null)
        {
            GeoHandler geoHandler = geoHandlerObj.GetComponent<GeoHandler>();
            if (geoHandler != null)
            {
                // Access data handlers through the GeoHandler instance
                specDataHandler = geoHandler.specDataHandler;
                imuDataHandler = geoHandler.imuDataHandler;
                telemetryDataHandler = geoHandler.telemetryDataHandler;
                tsscConnection = geoHandler.tsscConnection;
                evaNumberHandler = geoHandler.evaNumberHandler;
            }
            else
            {
                Debug.LogError("GeoHandler component not found on GeoHANDLER GameObject.");
            }
        }
        else
        {
            Debug.LogError("GeoHANDLER GameObject not found in the scene.");
        }

        // Find the VirtualizedScrollRectList at runtime
        if (virtualizedScrollRectList == null)
        {
            // Find the FieldNoteBook object in the scene
            GameObject fieldNoteBook = GameObject.Find("FieldNoteBook");
            
            if (fieldNoteBook != null)
            {
                // Get the VirtualizedScrollRectList component from it
                virtualizedScrollRectList = fieldNoteBook.GetComponent<VirtualizedScrollRectList>();
                
                if (virtualizedScrollRectList == null)
                {
                    // If not on the root, try to find it in children
                    virtualizedScrollRectList = fieldNoteBook.GetComponentInChildren<VirtualizedScrollRectList>();
                }
                
                Debug.Log("Found VirtualizedScrollRectList at runtime: " + (virtualizedScrollRectList != null));
            }
            else
            {
                Debug.LogError("FieldNoteBook GameObject not found in the scene.");
            }
        }

        if (specDataHandler == null)
        {
            Debug.LogError("SPECDataHandler reference is not set. Please assign it in the inspector.");
        }
        
        if (specResultsText == null)
        {
            Debug.LogError("SpecResultsText reference is not set. Please assign the TextMeshProUGUI component in the Spec Results Placeholder.");
        }
        
        // Verify references for Field Note Info
        if (imuDataHandler == null)
        {
            Debug.LogError("IMUDataHandler reference is not set. Please assign it in the inspector.");
        }
        
        if (telemetryDataHandler == null)
        {
            Debug.LogError("TELEMETRYDataHandler reference is not set. Please assign it in the inspector.");
        }
        
        if (dateTimeLocationText == null)
        {
            Debug.LogError("TextMeshProUGUI reference is not set. Please assign the date/time/location text field in the inspector.");
        }
        
        // Verify shared references
        if (tsscConnection == null)
        {
            Debug.LogError("TSScConnection reference is not set. Please assign it in the inspector.");
        }
        
        if (evaNumberHandler == null)
        {
            Debug.LogError("EVANumberHandler reference is not set. Please assign it in the inspector.");
        }
        
        // Verify rock type buttons
        if (rockButton == null)
        {
            Debug.LogError("Rock Button reference is not set. Please assign it in the inspector.");
        }
        if (regolithButton == null)
        {
            Debug.LogError("Regolith Button reference is not set. Please assign it in the inspector.");
        }

        // Verify close button
        if (closeButton == null)
        {
            Debug.LogError("Rock Button reference is not set. Please assign it in the inspector.");
        }

        // Verify sliders
        if (grainSizeSlider == null)
        {
            Debug.LogError("GrainSizeSlider reference is not set. Please assign it in the inspector.");
        }
        if (sortingSlider == null)
        {
            Debug.LogError("SortingSlider reference is not set. Please assign it in the inspector.");
        }
        if (durabilitySlider == null)
        {
            Debug.LogError("DurabilitySlider reference is not set. Please assign it in the inspector.");
        }
        
        // Verify edit button
        if (editButton == null)
        {
            Debug.LogError("Edit Button reference is not set. Please assign it in the inspector.");
        }
        else
        {
            // Register for edit button click events
            editButton.OnClicked.AddListener(ToggleEditMode);
        }

        // Verify references for Complete button
        if (completeButton == null)
        {
            Debug.LogError("Complete Button reference is not set. Please assign it in the inspector.");
        }
        else
        {
            // Register for complete button click events
            completeButton.OnClicked.AddListener(OnCompleteButtonClicked);
        }
        
        // Verify reference for NoteCard prefab
        if (noteCardPrefab == null)
        {
            Debug.LogError("NoteCard prefab reference is not set. Please assign it in the inspector.");
        }
        
        // Verify reference for VirtualizedScrollRectList
        if (virtualizedScrollRectList == null)
        {
            Debug.LogError("VirtualizedScrollRectList reference is not set. Please assign it in the inspector.");
        }
        
        // Initialize the field note data
        currentFieldNoteData = new FieldNoteData();
        // Register event listeners for buttons and sliders
        RegisterEventListeners();
        
        // Register virtualized list callbacks if the list exists
        if (virtualizedScrollRectList != null)
        {
            RegisterVirtualizedListCallbacks();
        }
    }
    
    void Update()
    {
        // Check for updates to Spec Results only
        // The field note info will be updated only when spec data changes
        CheckForSpecResultsUpdate();
    }
    
    private void ToggleEditMode()
    {
        // Toggle the edit mode state
        isEditModeEnabled = !isEditModeEnabled;
        Debug.Log($"Edit mode is now {(isEditModeEnabled ? "enabled" : "disabled")}");
        
        // If edit mode was just enabled, synchronize local data with current data
        if (isEditModeEnabled)
        {
            justEnteredEditMode = true;
            SynchronizeLocalData();
            Debug.Log("Local data synchronized with current data. Waiting for next update.");
        }
    }
    
    // Method to synchronize local data with current data when edit mode is enabled
    private void SynchronizeLocalData()
    {
        try
        {
            // Get the current EVA number
            int evaNumber = evaNumberHandler.getEVANumber();
            string evaKey = "eva" + evaNumber;
            
            // Only proceed if we have a valid EVA number
            if (evaNumber == 1 || evaNumber == 2)
            {
                // Synchronize spec data
                localSampleName = specDataHandler.GetName(evaKey);
                localSampleId = specDataHandler.GetId(evaKey);
                
                // Synchronize mineral composition
                localMineralComposition.Clear();
                string[] compounds = new string[] { "SiO2", "TiO2", "Al2O3", "FeO", "MnO", "MgO", "CaO", "K2O", "P2O3", "other" };
                foreach (string compound in compounds)
                {
                    float value = specDataHandler.GetCompoundData(evaKey, compound);
                    localMineralComposition[compound] = value;
                }
                
                // Synchronize field note info
                localDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                localEvaTimeSeconds = telemetryDataHandler.GetEVATime();
                
                // Synchronize location
                float posX = imuDataHandler.GetPosx(evaKey);
                float posY = imuDataHandler.GetPosy(evaKey);
                float heading = imuDataHandler.GetHeading(evaKey);
                localLocation = new Vector3(posX, posY, heading);
            }
            else
            {
                // Set default values if EVA number is invalid
                localSampleName = "";
                localSampleId = 0f;
                localMineralComposition.Clear();
                localDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                localEvaTimeSeconds = 0f;
                localLocation = Vector3.zero;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error synchronizing local data: " + e.Message);
        }
    }
    
    private void RegisterEventListeners()
    {
        // Register for button events
        if (rockButton != null)
        {
            rockButton.OnClicked.AddListener(() => SetRockType("Rock"));
        }
        
        if (regolithButton != null)
        {
            regolithButton.OnClicked.AddListener(() => SetRockType("Regolith"));
        }
        
        // Register for slider events
        if (grainSizeSlider != null)
        {
            grainSizeSlider.OnValueUpdated.AddListener(OnGrainSizeChanged);
        }
        
        if (sortingSlider != null)
        {
            sortingSlider.OnValueUpdated.AddListener(OnSortingChanged);
        }
        
        if (durabilitySlider != null)
        {
            durabilitySlider.OnValueUpdated.AddListener(OnDurabilityChanged);
        }
    }
    
    private void SetRockType(string type)
    {
        // Only update if edit mode is enabled
        if (!isEditModeEnabled) 
        {
            Debug.Log("Cannot set rock type: Edit mode is disabled");
            return;
        }
        
        rockType = type;
        currentFieldNoteData.rockType = rockType;
        Debug.Log($"Rock type selected: {rockType}");
    }
    
    private void OnGrainSizeChanged(SliderEventData eventData)
    {
        // Only update if edit mode is enabled
        if (!isEditModeEnabled)
        {
            Debug.Log("Cannot update grain size: Edit mode is disabled");
            return;
        }
        
        grainSize = eventData.NewValue;
        currentFieldNoteData.grainSize = grainSize;
        Debug.Log($"Grain size updated: {grainSize}");
    }
    
    private void OnSortingChanged(SliderEventData eventData)
    {
        // Only update if edit mode is enabled
        if (!isEditModeEnabled)
        {
            Debug.Log("Cannot update sorting: Edit mode is disabled");
            return;
        }
        
        sorting = eventData.NewValue;
        currentFieldNoteData.sorting = sorting;
        Debug.Log($"Sorting updated: {sorting}");
    }
    
    private void OnDurabilityChanged(SliderEventData eventData)
    {
        // Only update if edit mode is enabled
        if (!isEditModeEnabled)
        {
            Debug.Log("Cannot update durability: Edit mode is disabled");
            return;
        }
        
        durability = eventData.NewValue;
        currentFieldNoteData.durability = durability;
        Debug.Log($"Durability updated: {durability}");
    }
    
    private void CheckForSpecResultsUpdate()
    {
        // Skip if essential references for Spec Results are missing or edit mode is disabled
        if (tsscConnection == null || specDataHandler == null || !isEditModeEnabled)
        {
            return;
        }
        
        try
        {
            // Get the current EVA number
            int evaNumber = evaNumberHandler.getEVANumber();
            string evaKey = "eva" + evaNumber;
            
            // Check if EVA number is valid
            if (evaNumber != 1 && evaNumber != 2)
            {
                return;
            }
            
            // If we just entered edit mode, clear the flag and don't update yet
            if (justEnteredEditMode)
            {
                // We've already synchronized the data in ToggleEditMode, so just clear the flag
                justEnteredEditMode = false;
                return;
            }
            
            // Get current sample name and ID from data handler
            string currentSampleName = specDataHandler.GetName(evaKey);
            float currentSampleId = specDataHandler.GetId(evaKey);
            
            // Check if sample name or ID has changed
            bool hasChanged = (currentSampleName != localSampleName) || (currentSampleId != localSampleId);
            
            // If no change in name or ID, check mineral composition
            if (!hasChanged && !string.IsNullOrEmpty(currentSampleName))
            {
                // Check mineral composition
                string[] compounds = new string[] { "SiO2", "TiO2", "Al2O3", "FeO", "MnO", "MgO", "CaO", "K2O", "P2O3", "other" };
                
                foreach (string compound in compounds)
                {
                    float currentValue = specDataHandler.GetCompoundData(evaKey, compound);
                    
                    // Check if the compound exists in local data
                    if (localMineralComposition.TryGetValue(compound, out float localValue))
                    {
                        // Compare values
                        if (currentValue != localValue)
                        {
                            hasChanged = true;
                            break;
                        }
                    }
                    else
                    {
                        // Compound doesn't exist in local data
                        hasChanged = true;
                        break;
                    }
                }
            }
            
            // If any data has changed, update the spec results
            if (hasChanged)
            {
                Debug.Log("Spec data has changed since last update. Updating spec results.");
                UpdateSpecResults();
                
                // Also update field note info when spec data changes
                UpdateFieldNoteInfo();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error checking for spec updates: " + e.Message);
        }
    }
    
    // Removed the CheckForFieldNoteInfoUpdate() method since we'll only update
    // field note info when spec data changes
    
    public void UpdateSpecResults()
    {
        if (specDataHandler == null || evaNumberHandler == null || specResultsText == null)
        {
            return;
        }
        
        // Only proceed if edit mode is enabled
        if (!isEditModeEnabled)
        {
            Debug.Log("Cannot update spec results: Edit mode is disabled");
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
            
            // Update local copies for future comparison
            localSampleName = rockName;
            localSampleId = rockId;
            
            // Store the sample data
            currentFieldNoteData.sampleName = rockName;
            currentFieldNoteData.sampleId = rockId;
            
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
            
            // Clear previous mineral composition data
            currentFieldNoteData.mineralComposition.Clear();
            localMineralComposition.Clear();
            
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
                // Store mineral composition data
                float leftValue = specDataHandler.GetCompoundData(evaKey, leftColumnCompounds[i]);
                currentFieldNoteData.mineralComposition[leftColumnCompounds[i]] = leftValue;
                localMineralComposition[leftColumnCompounds[i]] = leftValue;
                
                string leftColumn = $"{leftColumnCompounds[i]}: {leftValue:F2}%";
                
                // Add padding to make columns align
                leftColumn = leftColumn.PadRight(16);
                
                // Add right column if available
                if (i < rightColumnCompounds.Length)
                {
                    float rightValue = specDataHandler.GetCompoundData(evaKey, rightColumnCompounds[i]);
                    currentFieldNoteData.mineralComposition[rightColumnCompounds[i]] = rightValue;
                    localMineralComposition[rightColumnCompounds[i]] = rightValue;
                    
                    string rightColumn = $"{rightColumnCompounds[i]}: {rightValue:F2}%";
                    results += leftColumn + rightColumn + "\n";
                }
                else
                {
                    results += leftColumn + "\n";
                }
            }
            
            // Update the text component
            specResultsText.text = results;
            
            Debug.Log("Spec Results updated based on data change");
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting spectrometry data: " + e.Message);
            specResultsText.text = "Error getting spectrometry data";
        }
    }

    private void OnCompleteButtonClicked()
    {
        Debug.Log("Complete button clicked");
        GenerateNoteCard();
    }

    private void GenerateNoteCard()
    {
        // Check if we have the virtualized list reference
        if (virtualizedScrollRectList == null)
        {
            Debug.LogError("Cannot generate NoteCard: Missing VirtualizedScrollRectList reference");
            return;
        }
        
        try
        {
            // Store the current field note data in the list
            noteCards.Add(new FieldNoteData
            {
                rockType = currentFieldNoteData.rockType,
                grainSize = currentFieldNoteData.grainSize,
                sorting = currentFieldNoteData.sorting,
                durability = currentFieldNoteData.durability,
                sampleName = currentFieldNoteData.sampleName,
                sampleId = currentFieldNoteData.sampleId,
                mineralComposition = new Dictionary<string, float>(currentFieldNoteData.mineralComposition),
                dateTime = currentFieldNoteData.dateTime,
                evaTimeSeconds = currentFieldNoteData.evaTimeSeconds,
                location = currentFieldNoteData.location,
                fieldNoteObject = gameObject
            });
            
            // Update the virtualized list item count
            virtualizedScrollRectList.SetItemCount(noteCards.Count);
            
            // Register callbacks if they haven't been registered yet
            RegisterVirtualizedListCallbacks();
            
            Debug.Log($"NoteCard data added to list. Total cards: {noteCards.Count}");
            
            // Disable edit mode
            isEditModeEnabled = false;
            Debug.Log("Edit mode is now disabled");
            
            // Disable Game object when completed
            gameObject.SetActive(false);
            Debug.Log("Field Note GameObject disabled");
        }
        catch (Exception e)
        {
            Debug.LogError("Error generating NoteCard: " + e.Message);
        }
    }

    // Register callbacks for the virtualized list
    private void RegisterVirtualizedListCallbacks()
    {
        // Only register callbacks once
        if (!callbacksRegistered)
        {
            virtualizedScrollRectList.OnVisible += OnNoteCardVisible;
            virtualizedScrollRectList.OnInvisible += OnNoteCardInvisible;
            callbacksRegistered = true;
            Debug.Log("Virtualized list callbacks registered");
        }
    }

    // Callback methods for the virtualized list - FIXED to match Action<GameObject, int>
    private void OnNoteCardVisible(GameObject go, int index)
    {
        // First reset the RectTransform position to ensure proper centering
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        Vector3 currentPos = rectTransform.localPosition;
        rectTransform.localPosition = new Vector3(0, currentPos.y, 0);

        // Make sure the index is valid
        if (index < 0 || index >= noteCards.Count)
        {
            Debug.LogWarning($"Invalid index {index} for noteCards list with count {noteCards.Count}");
            return;
        }
        
        try
        {
            // Get the note card data for this index
            FieldNoteData data = noteCards[index];
            
            // Find the NoteCardInformation component on this GameObject
            NoteCardInformation noteCardInfo = go.GetComponent<NoteCardInformation>();
            if (noteCardInfo == null)
            {
                Debug.LogError($"NoteCardInformation component missing on prefab at index {index}");
                return;
            }
            
            // Format location for display
            string formattedLocation = FormatLocation(data.location);
            
            // Set the data on the note card, passing the stored field note GameObject
            noteCardInfo.SetCardData(
                data.dateTime,
                formattedLocation,
                data.sampleName,
                data.rockType,
                data.fieldNoteObject // Pass the stored field note reference
            );
            
            Debug.Log($"NoteCard at index {index} populated with data");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error setting up note card at index {index}: {e.Message}");
        }
    }

    // Keep parameter order as GameObject, int to match delegate
    private void OnNoteCardInvisible(GameObject go, int index)
    {
        // Clean up any resources if needed
        Debug.Log($"NoteCard at index {index} is now invisible");
    }

    // Helper method to find a child by name recursively through the hierarchy
    private Transform FindChildRecursively(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(childName))
            {
                return child;
            }
            
            Transform found = FindChildRecursively(child, childName);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    private string FormatLocation(Vector3 location)
    {
        return $"X:{location.x:F1} Y:{location.y:F1} H:{location.z:F0}°";
    }

    
    public void UpdateFieldNoteInfo()
    {
        if (imuDataHandler == null || telemetryDataHandler == null || evaNumberHandler == null || dateTimeLocationText == null)
        {
            return;
        }
        
        // Only proceed if edit mode is enabled
        if (!isEditModeEnabled)
        {
            Debug.Log("Cannot update field note info: Edit mode is disabled");
            return;
        }
        
        try
        {
            // Get the current date
            DateTime currentDate = DateTime.Now;
            string dateString = "date: " + currentDate.ToString("yyyy-MM-dd");
            
            // Store the date/time and update local copy
            string formattedDateTime = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
            currentFieldNoteData.dateTime = formattedDateTime;
            localDateTime = formattedDateTime;
            
            // Get the current EVA number and convert to eva1/eva2 format
            int evaNumber = evaNumberHandler.getEVANumber();
            string evaKey = "eva" + evaNumber;
            
            // Get mission time from telemetry data using the new method
            float evaTimeSeconds = telemetryDataHandler.GetEVATime();
            
            // Store EVA time and update local copy
            currentFieldNoteData.evaTimeSeconds = evaTimeSeconds;
            localEvaTimeSeconds = evaTimeSeconds;
            
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
                // Store default location and update local copy
                Vector3 defaultLocation = new Vector3(0, 0, 0);
                currentFieldNoteData.location = defaultLocation;
                localLocation = defaultLocation;
            }
            else
            {
                // Get location data from IMUDataHandler
                float posX = imuDataHandler.GetPosx(evaKey);
                float posY = imuDataHandler.GetPosy(evaKey);
                float heading = imuDataHandler.GetHeading(evaKey);
                
                // Store location data and update local copy
                Vector3 locationData = new Vector3(posX, posY, heading);
                currentFieldNoteData.location = locationData;
                localLocation = locationData;
                
                // Format location data
                locationString += $"X:{posX:F1} Y:{posY:F1} H:{heading:F0}°";
            }
            
            // Combine all three lines into one string
            string combinedText = dateString + "\n" + timeString + "\n" + locationString;
            
            // Update the text
            dateTimeLocationText.text = combinedText;
            
            Debug.Log("Field Note Info updated based on spec data change");
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting field note info: " + e.Message);
            dateTimeLocationText.text = "date: Error\ntime: Error\nlocation: Error";
        }
    }

    // Add this method to your FieldNoteHandler class
    public void ToggleFieldNoteOff()
    {
        // Simple method to deactivate this field note
        gameObject.SetActive(false);
        Debug.Log("Field Note toggled off via button press");
    }
}