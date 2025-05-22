using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.UX.Experimental;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;


// Add RequireComponent to ensure AudioSource is present, which is good practice for new features.
[RequireComponent(typeof(AudioSource))]
public class FieldNoteHandler : MonoBehaviour
{
    // References for Spec Results functionality
    public SPECDataHandler specDataHandler;
    public TextMeshProUGUI specResultsText;

    // Reference for significance indicator
    public TextMeshProUGUI sigIndicator;

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
    public MixedReality.Toolkit.UX.Slider grainSizeSlider;
    public MixedReality.Toolkit.UX.Slider sortingSlider;
    public MixedReality.Toolkit.UX.Slider durabilitySlider;

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

    // Dictionary to store threshold values for mineral composition - cache for performance
    private readonly Dictionary<string, Threshold> thresholds = new Dictionary<string, Threshold>();

    // Reference to the Content GameObject where note cards will be spawned
    private GameObject contentGameObject;

    //voice recording
    private AudioSource audioSource;
    private string microphoneDevice;
    private const int MAX_RECORDING_LENGTH_SECONDS = 600; // 5 minutes, can be adjusted
    public PressableButton playPauseToggleButton;

    // Slider for audio playback position
    public MixedReality.Toolkit.UX.Slider playbackSlider;
    private bool isUpdatingPlaybackSlider = false;
    private bool userIsScrubbing = false;
    public TextMeshProUGUI playbackTimeText; // Optional: Text to show playback time

    // photos
    public Image samplePhotoDisplay;
    private bool isCapturingPhoto = false;

    // Performance optimization: Update interval for checks that don't need to run every frame
    private const float UPDATE_INTERVAL = 0.5f; // Check every half second
    private float updateTimer = 0f;

    // Cached components
    private Transform cachedTransform;
    private string evaKey = "";

    // String builders for better string concatenation performance
    private System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(256);

    // Struct to store threshold values for determining scientific significance
    private struct Threshold
    {
        public float value;
        public bool isGreaterThan;  // true if significant when greater than threshold, false if significant when less than threshold

        public Threshold(float val, bool greater)
        {
            value = val;
            isGreaterThan = greater;
        }
    }

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
        public bool isScientificallySignificant;
        public GameObject fieldNoteObject;
        public AudioClip recordedAudioClip; // To store the voice recording
        public Texture2D capturedSampleImage;

        public FieldNoteData()
        {
            mineralComposition = new Dictionary<string, float>();
        }
    }

    void Awake()
    {
        // Cache components and setup essential data early
        cachedTransform = transform;
        audioSource = GetComponent<AudioSource>();

        // Initialize current field note data early
        currentFieldNoteData = new FieldNoteData();

        // Initialize threshold values based on the reference table
        InitializeThresholds();
    }

    void Start()
    {
        // Voice Recording Initialization
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
        }
        else
        {
            Debug.LogWarning("FieldNoteHandler: No microphone devices found. Voice recording will not be available.");
        }

        if (playPauseToggleButton != null)
        {
            playPauseToggleButton.OnClicked.AddListener(TogglePlayback);
        }
        else
        {
            Debug.LogWarning("FieldNoteHandler: PlayPauseToggleButton is not assigned in the Inspector.");
        }

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

        // Find the Content GameObject where note cards will be spawned
        FindContentGameObject();

        // Find the FieldNoteBook GameObject and set it as the solver handler override
        GameObject fieldNoteBook = GameObject.Find("FieldNoteBook");
        if (fieldNoteBook != null)
        {
            // Get the SolverHandler component on this GameObject
            SolverHandler solverHandler = GetComponent<SolverHandler>();
            if (solverHandler != null)
            {
                // Set the TransformOverride to the FieldNoteBook transform
                solverHandler.TransformOverride = fieldNoteBook.transform;
                Debug.Log("SolverHandler TransformOverride set to FieldNoteBook");
            }
            else
            {
                Debug.LogWarning("SolverHandler component not found on this GameObject");
            }
        }
        else
        {
            Debug.LogError("FieldNoteBook GameObject not found in the scene");
        }

        ValidateReferences();

        // Verify reference for playback slider
        if (playbackSlider != null)
        {
            // Register for the slider's ValueUpdated event
            playbackSlider.OnValueUpdated.AddListener(OnPlaybackSliderChanged);

            // Use the StatefulInteractable's events for detecting interaction
            StatefulInteractable sliderInteractable = playbackSlider.GetComponent<StatefulInteractable>();
            if (sliderInteractable != null)
            {
                // Use lambda expressions to connect the events properly
                sliderInteractable.firstSelectEntered.AddListener((args) => OnPlaybackSliderInteractionStarted());
                sliderInteractable.lastSelectExited.AddListener((args) => OnPlaybackSliderInteractionEnded());
            }
            else
            {
                Debug.LogWarning("Could not find StatefulInteractable on playbackSlider. Scrubbing detection will not work properly.");
            }
        }

        // Register event listeners for buttons and sliders
        RegisterEventListeners();

        // Register with camera manager for photo capture
        CameraManager.Instance.RegisterFieldNote();

        // Initial update of field note data
        if (evaNumberHandler != null)
        {
            int evaNumber = evaNumberHandler.getEVANumber();
            evaKey = "eva" + evaNumber;
        }
    }

    private void ValidateReferences()
    {
        // Validate critical references
        if (specDataHandler == null) Debug.LogError("SPECDataHandler reference is not set.");
        if (specResultsText == null) Debug.LogError("SpecResultsText reference is not set.");
        if (sigIndicator == null) Debug.LogError("SigIndicator reference is not set.");
        else sigIndicator.color = Color.white; // Set default color

        if (imuDataHandler == null) Debug.LogError("IMUDataHandler reference is not set.");
        if (telemetryDataHandler == null) Debug.LogError("TELEMETRYDataHandler reference is not set.");
        if (dateTimeLocationText == null) Debug.LogError("DateTimeLocationText reference is not set.");

        if (tsscConnection == null) Debug.LogError("TSScConnection reference is not set.");
        if (evaNumberHandler == null) Debug.LogError("EVANumberHandler reference is not set.");

        if (rockButton == null) Debug.LogError("Rock Button reference is not set.");
        if (regolithButton == null) Debug.LogError("Regolith Button reference is not set.");
        if (closeButton == null) Debug.LogError("Close Button reference is not set.");

        if (grainSizeSlider == null) Debug.LogError("GrainSizeSlider reference is not set.");
        if (sortingSlider == null) Debug.LogError("SortingSlider reference is not set.");
        if (durabilitySlider == null) Debug.LogError("DurabilitySlider reference is not set.");

        if (editButton == null) Debug.LogError("Edit Button reference is not set.");
        else editButton.OnClicked.AddListener(ToggleEditMode);

        if (completeButton == null) Debug.LogError("Complete Button reference is not set.");
        else completeButton.OnClicked.AddListener(OnCompleteButtonClicked);

        if (noteCardPrefab == null) Debug.LogError("NoteCard prefab reference is not set.");
        if (samplePhotoDisplay == null) Debug.LogWarning("SamplePhotoDisplay (UI.Image) is not assigned. Photo feature may not work.");
    }

    public void StartRecording()
    {
        if (string.IsNullOrEmpty(microphoneDevice))
        {
            Debug.LogWarning("FieldNoteHandler: Cannot start recording - no microphone device found.");
            return;
        }
        if (Microphone.IsRecording(microphoneDevice))
        {
            Microphone.End(microphoneDevice); // Stop previous if any
        }
        audioSource.clip = Microphone.Start(microphoneDevice, false, MAX_RECORDING_LENGTH_SECONDS, AudioSettings.outputSampleRate);
        Debug.Log("FieldNoteHandler: Voice recording started.");
    }

    public void StopRecording()
    {
        if (string.IsNullOrEmpty(microphoneDevice) || !Microphone.IsRecording(microphoneDevice))
        {
            return;
        }

        // Get the current recording position before stopping
        int position = Microphone.GetPosition(microphoneDevice);

        // Stop recording
        Microphone.End(microphoneDevice);

        if (audioSource.clip != null && position > 0)
        {
            // Create a new AudioClip with only the recorded portion (trimming silence)
            AudioClip originalClip = audioSource.clip;
            AudioClip trimmedClip = AudioClip.Create(
                originalClip.name,
                position,
                originalClip.channels,
                originalClip.frequency,
                false);

            // Get the audio data from the original clip
            float[] data = new float[position * originalClip.channels];
            originalClip.GetData(data, 0);

            // Set the data to the trimmed clip
            trimmedClip.SetData(data, 0);

            // Store the trimmed clip instead of the original
            currentFieldNoteData.recordedAudioClip = trimmedClip;
            audioSource.clip = trimmedClip;

            Debug.Log($"FieldNoteHandler: Voice recording stopped and stored. Clip length: {trimmedClip.length:F2}s");
        }
        UpdatePlaybackTimeText();
    }

    public void TogglePlayback()
    {
        if (audioSource == null || currentFieldNoteData == null || currentFieldNoteData.recordedAudioClip == null)
        {
            Debug.LogWarning("FieldNoteHandler: No audio clip recorded or AudioSource missing for playback.");
            if (playPauseToggleButton != null) playPauseToggleButton.ForceSetToggled(false); // Ensure toggle is off

            // Make sure playback slider is inactive
            if (playbackSlider != null)
            {
                playbackSlider.gameObject.SetActive(false);
            }

            return;
        }

        // Make sure playback slider is active whenever we have a valid clip
        if (playbackSlider != null)
        {
            playbackSlider.gameObject.SetActive(true);
        }

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("FieldNoteHandler: Audio playback paused.");
        }
        else
        {
            if (audioSource.clip == currentFieldNoteData.recordedAudioClip && audioSource.time > 0 && audioSource.time < audioSource.clip.length)
            {
                audioSource.UnPause(); // Resume if paused
                Debug.Log("FieldNoteHandler: Audio playback resumed.");
            }
            else
            {
                audioSource.clip = currentFieldNoteData.recordedAudioClip;
                audioSource.Play(); // Play from beginning
                Debug.Log("FieldNoteHandler: Audio playback started.");

                // Reset slider position when starting from beginning
                if (playbackSlider != null)
                {
                    isUpdatingPlaybackSlider = true;
                    playbackSlider.Value = 0;
                    isUpdatingPlaybackSlider = false;
                }
            }
        }
    }

    // Find the Content GameObject where note cards will be spawned - optimized version
    private void FindContentGameObject()
    {
        GameObject fieldNoteBook = GameObject.Find("FieldNoteBook");
        if (fieldNoteBook != null)
        {
            // PERFORMANCE OPTIMIZATION: Use direct transform.Find for better performance than recursive search
            Transform contentTransform = fieldNoteBook.transform.Find("Content");

            // If direct search fails, fall back to recursive search
            if (contentTransform == null)
            {
                contentTransform = FindExactChildRecursively(fieldNoteBook.transform, "Content");
            }

            if (contentTransform != null)
            {
                contentGameObject = contentTransform.gameObject;
                Debug.Log("Content GameObject found successfully: " + contentGameObject.name);
            }
            else
            {
                Debug.LogError("Content GameObject not found under FieldNoteBook");
            }
        }
        else
        {
            Debug.LogError("FieldNoteBook GameObject not found in the scene");
        }
    }

    // Helper method to find a child by exact name recursively through the hierarchy
    private Transform FindExactChildRecursively(Transform parent, string exactChildName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == exactChildName)
            {
                return child;
            }

            Transform found = FindExactChildRecursively(child, exactChildName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    // Initialize thresholds based on the reference table
    private void InitializeThresholds()
    {
        // Add threshold values from the reference table
        // Format: compound name, threshold value, is significant when greater than threshold (true) or less than threshold (false)
        thresholds.Add("SiO2", new Threshold(30.0f, false));  // <30 is significant
        thresholds.Add("TiO2", new Threshold(10.0f, true));   // >10 is significant
        thresholds.Add("Al2O3", new Threshold(25.0f, true));  // >25 is significant
        thresholds.Add("FeO", new Threshold(20.0f, true));    // >20 is significant
        thresholds.Add("MnO", new Threshold(0.5f, true));     // >0.5 is significant
        thresholds.Add("MgO", new Threshold(10.0f, true));    // >10 is significant
        thresholds.Add("CaO", new Threshold(5.0f, false));    // <5 is significant
        thresholds.Add("K2O", new Threshold(1.0f, true));     // >1 is significant
        thresholds.Add("P2O3", new Threshold(1.0f, true));    // >1 is significant
        thresholds.Add("other", new Threshold(50.0f, true));  // >50 is significant
    }

    // Check if a compound value is scientifically significant
    private bool IsScientificallySignificant(string compound, float value)
    {
        // Check if the compound exists in our thresholds dictionary
        if (thresholds.TryGetValue(compound, out Threshold threshold))
        {
            // Check if the value exceeds the threshold in the specified direction
            if (threshold.isGreaterThan)
            {
                // For "greater than" thresholds, value is significant if greater than threshold
                return value > threshold.value;
            }
            else
            {
                // For "less than" thresholds, value is significant if less than threshold
                return value < threshold.value;
            }
        }

        // If compound not found in thresholds, not significant
        return false;
    }

    void Update()
    {
        // PERFORMANCE OPTIMIZATION: Use timer to reduce frequency of checks
        updateTimer += Time.deltaTime;
        if (updateTimer >= UPDATE_INTERVAL)
        {
            updateTimer = 0f;

            // Check for updates to Spec Results only when needed
            if (isEditModeEnabled)
            {
                CheckForSpecResultsUpdate();
            }
        }

        // Update audio playback UI - this needs to run every frame for smoothness
        if (audioSource != null && audioSource.clip != null)
        {
            if (audioSource.isPlaying && !userIsScrubbing)
            {
                UpdatePlaybackSliderPosition();
                UpdatePlaybackTimeText();
            }
        }
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
            int evaNumber = evaNumberHandler != null ? evaNumberHandler.getEVANumber() : 0;
            evaKey = "eva" + evaNumber;

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
            // If we just entered edit mode, clear the flag and don't update yet
            if (justEnteredEditMode)
            {
                justEnteredEditMode = false;
                return;
            }

            // Get the current EVA number
            int evaNumber = evaNumberHandler?.getEVANumber() ?? 0;
            string currentEvaKey = "eva" + evaNumber;

            // Check if EVA number is valid
            if (evaNumber != 1 && evaNumber != 2)
            {
                return;
            }

            // Get current sample name and ID from data handler
            string currentSampleName = specDataHandler.GetName(currentEvaKey);
            float currentSampleId = specDataHandler.GetId(currentEvaKey);

            // PERFORMANCE: Quick check first for simple values before doing more complex checks
            bool hasChanged = (currentSampleName != localSampleName) || (currentSampleId != localSampleId);

            // If no change in name or ID, check mineral composition
            if (!hasChanged && !string.IsNullOrEmpty(currentSampleName))
            {
                // Cache the list of compounds
                string[] compounds = new string[] { "SiO2", "TiO2", "Al2O3", "FeO", "MnO", "MgO", "CaO", "K2O", "P2O3", "other" };

                // Check mineral composition but only when needed
                foreach (string compound in compounds)
                {
                    float currentValue = specDataHandler.GetCompoundData(currentEvaKey, compound);

                    // Check if the compound exists in local data
                    if (localMineralComposition.TryGetValue(compound, out float localValue))
                    {
                        // Compare values
                        if (Math.Abs(currentValue - localValue) > 0.001f) // Use epsilon for float comparison
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
                UpdateScanStepWithSignificance();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error checking for spec updates: " + e.Message);
        }
    }

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
            // Reset significance indicator to white (default state)
            if (sigIndicator != null)
            {
                sigIndicator.color = Color.white;
            }

            // Flag to track if any value is scientifically significant
            bool hasSignificantValue = false;

            // Get the current EVA number and convert to eva1/eva2 format
            int evaNumber = evaNumberHandler.getEVANumber();
            string currentEvaKey = "eva" + evaNumber;

            // Check if we have valid EVA number
            if (evaNumber != 1 && evaNumber != 2)
            {
                specResultsText.text = "Please select EVA 1 or EVA 2";
                return;
            }

            // Get sample name and ID
            string rockName = specDataHandler.GetName(currentEvaKey);
            float rockId = specDataHandler.GetId(currentEvaKey);

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

            // PERFORMANCE: Use StringBuilder instead of string concatenation
            stringBuilder.Clear();
            stringBuilder.AppendFormat("Sample Name: {0}\n", rockName);
            stringBuilder.AppendFormat("Sample ID: {0}\n\n", rockId);
            stringBuilder.Append("Mineral Composition:\n");

            // Create two columns of mineral data - cache these arrays
            string[] compounds = new string[] { "SiO2", "TiO2", "Al2O3", "FeO", "MnO", "MgO", "CaO", "K2O", "P2O3", "other" };

            // Clear previous mineral composition data
            currentFieldNoteData.mineralComposition.Clear();
            localMineralComposition.Clear();

            // Split into two arrays for the two columns
            int halfLength = compounds.Length / 2 + compounds.Length % 2; // Ceiling division

            // Format each row with the two columns
            for (int i = 0; i < halfLength; i++)
            {
                // Get left column data
                string leftCompound = compounds[i];
                float leftValue = specDataHandler.GetCompoundData(currentEvaKey, leftCompound);
                currentFieldNoteData.mineralComposition[leftCompound] = leftValue;
                localMineralComposition[leftCompound] = leftValue;

                // Check if the left value is scientifically significant
                bool leftSignificant = IsScientificallySignificant(leftCompound, leftValue);
                if (leftSignificant)
                {
                    hasSignificantValue = true;
                }

                // Format the left column with or without bold/underline formatting
                string leftColumnText = leftCompound + ": " + leftValue.ToString("F2") + "%";
                string leftColumn = leftSignificant ? $"<b><u><color=#FFFF00>{leftColumnText}</color></u></b>" : leftColumnText;

                // Add padding to make columns align (consider the rich text tags in the padding calculation)
                string leftPadding = new string(' ', Math.Max(1, 16 - leftColumnText.Length));

                // Add right column if available
                int rightIndex = i + halfLength;
                if (rightIndex < compounds.Length)
                {
                    string rightCompound = compounds[rightIndex];
                    float rightValue = specDataHandler.GetCompoundData(currentEvaKey, rightCompound);
                    currentFieldNoteData.mineralComposition[rightCompound] = rightValue;
                    localMineralComposition[rightCompound] = rightValue;

                    // Check if the right value is scientifically significant
                    bool rightSignificant = IsScientificallySignificant(rightCompound, rightValue);
                    if (rightSignificant)
                    {
                        hasSignificantValue = true;
                    }

                    // Format the right column with or without bold/underline formatting
                    string rightColumnText = rightCompound + ": " + rightValue.ToString("F2") + "%";
                    string rightColumn = rightSignificant ? $"<b><u><color=#FFFF00>{rightColumnText}</color></u></b>" : rightColumnText;

                    stringBuilder.Append(leftColumn).Append(leftPadding).Append(rightColumn).Append('\n');
                }
                else
                {
                    stringBuilder.Append(leftColumn).Append('\n');
                }
            }

            // Update the text component
            specResultsText.text = stringBuilder.ToString();

            // Update significance indicator color if there are significant values
            if (hasSignificantValue && sigIndicator != null)
            {
                sigIndicator.color = Color.yellow;
            }

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

        // Ensure edit mode is disabled
        isEditModeEnabled = false;

        // Update visual state of edit button to appear toggled off
        if (editButton != null)
        {
            // Force the edit button to update its visual state to match the disabled edit mode
            editButton.ForceSetToggled(false);
        }

        GenerateNoteCard();
    }

    private void GenerateNoteCard()
    {
        // Check if we have the Content GameObject reference
        if (contentGameObject == null)
        {
            // Try to find it again in case it wasn't found during Start
            FindContentGameObject();

            if (contentGameObject == null)
            {
                Debug.LogError("Cannot generate NoteCard: Content GameObject not found");
                return;
            }
        }

        // Check if we have the NoteCard prefab
        if (noteCardPrefab == null)
        {
            Debug.LogError("Cannot generate NoteCard: NoteCard prefab is missing");
            return;
        }

        try
        {
            // Check if any mineral composition values are scientifically significant
            bool hasSignificantValue = false;

            // Ensure mineralComposition is not null before iterating
            if (currentFieldNoteData.mineralComposition != null)
            {
                foreach (KeyValuePair<string, float> kvp in currentFieldNoteData.mineralComposition)
                {
                    if (IsScientificallySignificant(kvp.Key, kvp.Value))
                    {
                        hasSignificantValue = true;
                        break;
                    }
                }
            }

            // Store the current field note data in the list - OPTIMIZATION: Create a new object only when needed
            FieldNoteData newNoteData = new FieldNoteData
            {
                rockType = currentFieldNoteData.rockType,
                grainSize = currentFieldNoteData.grainSize,
                sorting = currentFieldNoteData.sorting,
                durability = currentFieldNoteData.durability,
                sampleName = currentFieldNoteData.sampleName,
                sampleId = currentFieldNoteData.sampleId,
                // Ensure mineralComposition is initialized before copying
                mineralComposition = currentFieldNoteData.mineralComposition != null ?
                    new Dictionary<string, float>(currentFieldNoteData.mineralComposition) :
                    new Dictionary<string, float>(),
                dateTime = currentFieldNoteData.dateTime,
                evaTimeSeconds = currentFieldNoteData.evaTimeSeconds,
                location = currentFieldNoteData.location,
                isScientificallySignificant = hasSignificantValue,
                fieldNoteObject = gameObject,
                recordedAudioClip = currentFieldNoteData.recordedAudioClip,
                capturedSampleImage = currentFieldNoteData.capturedSampleImage
            };

            // Add to the list for reference
            noteCards.Add(newNoteData);

            // Instantiate the NoteCard prefab as a child of the Content GameObject
            GameObject noteCardInstance = Instantiate(noteCardPrefab, contentGameObject.transform);

            // Find the NoteCardInformation component on the instantiated prefab
            NoteCardInformation noteCardInfo = noteCardInstance.GetComponent<NoteCardInformation>();
            if (noteCardInfo == null)
            {
                Debug.LogError("NoteCardInformation component missing on the instantiated prefab");
                return;
            }

            // Format location for display
            string formattedLocation = FormatLocation(newNoteData.location);

            // Set the data on the note card, including the EVA time seconds and the captured image
            noteCardInfo.SetCardData(
                newNoteData.dateTime,
                formattedLocation,
                newNoteData.sampleName,
                newNoteData.rockType,
                gameObject, // This is the FieldNote GameObject itself
                newNoteData.isScientificallySignificant,
                newNoteData.evaTimeSeconds, // Pass the EVA time seconds directly
                newNoteData.capturedSampleImage // Pass the captured image to the note card
            );

            Debug.Log("NoteCard successfully instantiated as a child of Content" +
                    (newNoteData.isScientificallySignificant ? " (SCIENTIFICALLY SIGNIFICANT)" : "") +
                    (newNoteData.capturedSampleImage != null ? " with image" : " without image"));

            // Disable edit mode
            isEditModeEnabled = false;
            Debug.Log("Edit mode is now disabled");

            // Disable this GameObject when completed
            gameObject.SetActive(false);
            Debug.Log("Field Note GameObject disabled");
        }
        catch (Exception e)
        {
            Debug.LogError("Error generating NoteCard: " + e.Message);
        }
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
            // PERFORMANCE: Use cached DateTime and a StringBuilder
            DateTime currentDate = DateTime.Now;
            stringBuilder.Clear();

            // Build the text content
            stringBuilder.AppendFormat("date: {0}\n", currentDate.ToString("yyyy-MM-dd"));

            // Store the date/time and update local copy
            string formattedDateTime = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
            currentFieldNoteData.dateTime = formattedDateTime;
            localDateTime = formattedDateTime;

            // Get the current EVA number and convert to eva1/eva2 format
            int evaNumber = evaNumberHandler.getEVANumber();
            string currentEvaKey = "eva" + evaNumber;

            // Get mission time from telemetry data
            float evaTimeSeconds = telemetryDataHandler.GetEVATime();

            // Store EVA time and update local copy
            currentFieldNoteData.evaTimeSeconds = evaTimeSeconds;
            localEvaTimeSeconds = evaTimeSeconds;

            // Convert seconds to TimeSpan for proper formatting
            TimeSpan missionTime = TimeSpan.FromSeconds(evaTimeSeconds);

            // Format mission time as hours:minutes:seconds
            stringBuilder.AppendFormat("time: {0:D2}:{1:D2}:{2:D2}\n",
                (int)missionTime.TotalHours, // Get total hours (not just hours component)
                missionTime.Minutes,
                missionTime.Seconds);

            // Add location line
            stringBuilder.Append("location: \n");

            // Check if we have valid EVA number
            if (evaNumber != 1 && evaNumber != 2)
            {
                stringBuilder.Append("Select EVA 1 or 2");
                // Store default location and update local copy
                Vector3 defaultLocation = new Vector3(0, 0, 0);
                currentFieldNoteData.location = defaultLocation;
                localLocation = defaultLocation;
            }
            else
            {
                // Get location data from IMUDataHandler
                float posX = imuDataHandler.GetPosx(currentEvaKey);
                float posY = imuDataHandler.GetPosy(currentEvaKey);
                float heading = imuDataHandler.GetHeading(currentEvaKey);

                // Store location data and update local copy
                Vector3 locationData = new Vector3(posX, posY, heading);
                currentFieldNoteData.location = locationData;
                localLocation = locationData;

                // Format location data
                stringBuilder.AppendFormat("X:{0:F1} Y:{1:F1} H:{2:F0}°", posX, posY, heading);
            }

            // Update the text
            dateTimeLocationText.text = stringBuilder.ToString();

            Debug.Log("Field Note Info updated based on spec data change");
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting field note info: " + e.Message);
            dateTimeLocationText.text = "date: Error\ntime: Error\nlocation: Error";
        }
    }

    public void ToggleFieldNoteOff()
    {
        // Simple method to deactivate this field note
        gameObject.SetActive(false);
        Debug.Log("Field Note toggled off via button press");
    }

    // --- PHOTO CAPTURE METHODS ---
    public void GeologicalPhotoCommandReceived()
    {
        // This method is intended to be called by MRTK's SpeechInputHandler
        // when "Take Geological Photo" is recognized.
        if (!isCapturingPhoto)
        {
            StartCoroutine(CaptureAndDisplayGeologicalPhoto());
        }
        else
        {
            Debug.Log("FieldNoteHandler: Photo capture already in progress.");
        }
    }

    private IEnumerator CaptureAndDisplayGeologicalPhoto()
    {
        isCapturingPhoto = true;
        Debug.Log("FieldNoteHandler: GeologicalPhotoCommandReceived. Capturing real-world view.");

        // Check if camera manager is ready
        if (!CameraManager.Instance.IsCameraReady())
        {
            Debug.LogError($"FieldNoteHandler: Camera not ready for capture. Status: {CameraManager.Instance.GetCameraStatus()}");
            isCapturingPhoto = false;
            yield break;
        }

        // Wait a frame to ensure we get the latest camera frame
        yield return null;

        try
        {
            // Capture photo from the shared camera manager
            Texture2D screenshotTexture = CameraManager.Instance.CapturePhoto();

            if (screenshotTexture != null)
            {
                Debug.Log("FieldNoteHandler: Real-world photo captured successfully.");
                if (currentFieldNoteData != null) // Ensure currentFieldNoteData is initialized
                {
                    currentFieldNoteData.capturedSampleImage = screenshotTexture; // Store in data
                }

                if (samplePhotoDisplay != null)
                {
                    // PERFORMANCE: Consider caching the sprite if photos will be taken multiple times
                    Sprite newSprite = Sprite.Create(screenshotTexture, new Rect(0, 0, screenshotTexture.width, screenshotTexture.height), new Vector2(0.5f, 0.5f));
                    samplePhotoDisplay.sprite = newSprite;
                    samplePhotoDisplay.color = Color.white; // Ensure it's fully opaque and visible
                    Debug.Log("FieldNoteHandler: Photo displayed on UI Image.");
                }
                else
                {
                    Debug.LogWarning("FieldNoteHandler: samplePhotoDisplay (UI.Image) is null. Cannot display photo.");
                }
            }
            else
            {
                Debug.LogError("FieldNoteHandler: Failed to capture photo from CameraManager.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FieldNoteHandler: Error capturing photo: {e.Message}");
        }

        isCapturingPhoto = false;
    }

    // --- AUDIO PLAYBACK UI METHODS ---
    // Updates the playback slider position based on current audio playback
    private void UpdatePlaybackSliderPosition()
    {
        if (playbackSlider == null || audioSource.clip == null) return;

        isUpdatingPlaybackSlider = true;
        playbackSlider.Value = audioSource.time / audioSource.clip.length;
        isUpdatingPlaybackSlider = false;
    }

    // Called when the user changes the playback slider value
    private void OnPlaybackSliderChanged(SliderEventData eventData)
    {
        if (isUpdatingPlaybackSlider || audioSource.clip == null) return;

        // Calculate position in seconds based on slider value
        float newPosition = eventData.NewValue * audioSource.clip.length;
        audioSource.time = newPosition;

        // If paused, we want to ensure the audio source position is updated
        // but not start playing automatically
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            audioSource.Pause();
        }
        UpdatePlaybackTimeText();
    }

    // Track when the user starts scrubbing the playback slider
    private void OnPlaybackSliderInteractionStarted()
    {
        userIsScrubbing = true;
    }

    // Track when the user finishes scrubbing the playback slider
    private void OnPlaybackSliderInteractionEnded()
    {
        userIsScrubbing = false;
    }

    // Format time as MM:SS
    private string FormatTimeMMSS(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Update timestamp display
    private void UpdatePlaybackTimeText()
    {
        if (playbackTimeText == null) return;

        if (audioSource == null || audioSource.clip == null)
        {
            playbackTimeText.text = "00:00 / 00:00";
            return;
        }

        float currentTime = audioSource.time;
        float totalTime = audioSource.clip.length;

        // PERFORMANCE: StringBuilder for string formatting
        stringBuilder.Clear();
        stringBuilder.Append(FormatTimeMMSS(currentTime));
        stringBuilder.Append(" / ");
        stringBuilder.Append(FormatTimeMMSS(totalTime));

        playbackTimeText.text = stringBuilder.ToString();
    }

    private void UpdateScanStepWithSignificance()
    {
        // Determine significance
        bool isSignificant = false;
        foreach (KeyValuePair<string, float> kvp in currentFieldNoteData.mineralComposition)
        {
            if (IsScientificallySignificant(kvp.Key, kvp.Value))
            {
                isSignificant = true;
                break;
            }
        }

        // Find the checklist manager and send the update
        ProcedureChecklistManager checklistManager = UnityEngine.Object.FindAnyObjectByType<ProcedureChecklistManager>();
        if (checklistManager != null)
        {
            checklistManager.UpdateScanStepText(isSignificant);
        }
    }
    void OnDestroy()
    {
        // Unregister from camera manager
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.UnregisterFieldNote();
            Debug.Log("FieldNoteHandler: Unregistered from CameraManager.");
        }
    }

    void OnDisable()
    {
        // Also unregister when field note is disabled (but not destroyed)
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.UnregisterFieldNote();
            Debug.Log("FieldNoteHandler: Unregistered from CameraManager due to disable.");
        }
    }

    void OnEnable()
    {
        // Re-register when field note is re-enabled
        if (CameraManager.Instance != null && gameObject.activeInHierarchy)
        {
            CameraManager.Instance.RegisterFieldNote();
            Debug.Log("FieldNoteHandler: Re-registered with CameraManager due to enable.");
        }
    }
}