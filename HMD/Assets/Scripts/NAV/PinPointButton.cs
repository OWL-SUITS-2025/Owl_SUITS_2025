using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Windows.Speech;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.Subsystems;


public class PinPointButton : MonoBehaviour
{
    [Header("Pin Prefabs")]
    [SerializeField] private GameObject pinPointIconPrefab;
    [SerializeField] private GameObject hazardPinPrefab;
    [SerializeField] private GameObject samplePinPrefab;
    [SerializeField] private Transform parentObject; // The parent object to which the pin will be attached

    [Header("Buttons")]
    [SerializeField] private StatefulInteractable generalPinBackplate;
    [SerializeField] private StatefulInteractable hazardPinBackplate;
    [SerializeField] private StatefulInteractable samplePinBackplate;



    private int samplePinCount = 0;
    private int generalPinCount = 0;
    private int hazardPinCount = 0;
    private string labelText;

    [SerializeField] private ROVERDataHandler roverDataHandler;

    [Header("Events")]
    public UnityEvent onGeneralPinClick = new UnityEvent();
    public UnityEvent onHazardPinClick = new UnityEvent();
    public UnityEvent onSamplePinClick = new UnityEvent();

    public IMUDataHandler imuDataHandler;
    [SerializeField] private EVANumberHandler evaNumberHandler;

    private bool isGeneralPinButtonPressed = false;
    private bool isHazardPinButtonPressed = false;
    private bool isSamplePinButtonPressed = false;
    private const float VISIBILITY_DISTANCE = 50f;
    private string type;
    private bool isPing = false;
    private bool poiCreated = false;



    public string keyword = "Place Pin";


    private KeywordRecognitionSubsystem keywordRecognitionSubsystem = null;

    bool recording;
    string micDevice;
    AudioClip clip;

    public void OnRecordButtonPressed()
    {
        if (!recording) StartRec(); else StopRec();
    }

    void StartRec()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphone found"); return;
        }

        micDevice = Microphone.devices[0];
        clip = Microphone.Start(micDevice, false, 300, 44100); // up to 5 min
        recording = true;

    }

    void StopRec()
    {
        Microphone.End(micDevice);
        recording = false;


    }

    private void Update()
    {
        isPing = roverDataHandler.GetPing();
        // Only proceed if nav is open, ping is true, and we haven't created yet
        if (isPing && !poiCreated)
        {
            PlacePOI("POI 1", new Vector2(roverDataHandler.GetPOI1x(), roverDataHandler.GetPOI1y()));
            PlacePOI("POI 2", new Vector2(roverDataHandler.GetPOI2x(), roverDataHandler.GetPOI3y()));
            PlacePOI("POI 3", new Vector2(roverDataHandler.GetPOI3x(), roverDataHandler.GetPOI3y()));
            poiCreated = true;
        }
    }

    private void Start()
    {
        if (generalPinBackplate != null)
            generalPinBackplate.OnClicked.AddListener(() => OnGeneralPinButtonPressed());
        if (samplePinBackplate != null)
            samplePinBackplate.OnClicked.AddListener(() => OnSamplePinButtonPressed());
        if (hazardPinBackplate != null)
            hazardPinBackplate.OnClicked.AddListener(() => OnHazardPinButtonPressed());


        keywordRecognitionSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<KeywordRecognitionSubsystem>();

        if (keywordRecognitionSubsystem != null)
        {
            keywordRecognitionSubsystem.CreateOrGetEventForKeyword(keyword)
                                     .AddListener(OnPhraseRecognized);
            Debug.Log($"PinPointSpeechCommands: Listener added for keyword '{keyword}' on {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("PinPointSpeechCommands: KeywordRecognitionSubsystem not found. Speech commands will not work.");
        }



    }

    private void PlacePOI(string labelText, Vector2 POImap)
    {


        int evaNumber = evaNumberHandler != null ? evaNumberHandler.getEVANumber() : 0;
        string evaKey = "eva" + evaNumber;

        // Only proceed if we have a valid EVA number
        if (evaNumber == 1 || evaNumber == 2)
        {
            float x = imuDataHandler.GetPosx(evaKey);
            float y = imuDataHandler.GetPosy(evaKey);


            // Get the camera's position (user's head)
            Vector3 currUnity = Camera.main.transform.position;
            Vector2 currUnity2 = new Vector2(currUnity.x, currUnity.y);
            Vector2 currMap = new Vector2(x, y);

            Vector2 POIunity = (POImap - currMap) + currUnity2;

            Vector3 pinPosition = new Vector3(POIunity.x, POIunity.y, currUnity.z);



            GameObject pin = Instantiate(pinPointIconPrefab, pinPosition, Quaternion.identity);
            pin.tag = "Pin";
            pin.transform.SetParent(parentObject, worldPositionStays: true); // Set parent

            TMP_Text distanceText = pin.GetComponentInChildren<TMP_Text>();


            distanceText.text = $"Name:{labelText}\nType: {type}\nX: {pinPosition.x}\nY: {pinPosition.y} ";



        }
    }

    private void OnDestroy()
    {
        if (keywordRecognitionSubsystem != null && !string.IsNullOrEmpty(keyword))
        {
            Debug.Log($"PinPointSpeechCommands for '{keyword}' on {gameObject.name} is being disabled. Listener cleanup will rely on the subsystem or object destruction.");
        }
        keywordRecognitionSubsystem = null;
    }

    private void OnPhraseRecognized()
    {
        Debug.Log($"PinPointSpeechCommands: Keyword '{keyword}' recognized on {gameObject.name}! Calling PinPointButton.");
        OnPlacePinCommand();
    }

    public void OnPlacePinCommand()
    {
        if (isGeneralPinButtonPressed)
        {


            generalPinCount++;

            labelText = $"General {generalPinCount}: ";

            type = "General";
            PlacePin(pinPointIconPrefab);
            isGeneralPinButtonPressed = false;
        }
        else if (isHazardPinButtonPressed)
        {
            hazardPinCount++;

            labelText = $"Hazard {hazardPinCount}: ";

            type = "Hazard";
            PlacePin(hazardPinPrefab);
            isHazardPinButtonPressed = false;
        }
        else if (isSamplePinButtonPressed)
        {
            samplePinCount++;

            labelText = $"Sample {samplePinCount}: ";


            type = "Sample";
            PlacePin(samplePinPrefab);
            isSamplePinButtonPressed = false;
        }
        else
        {
            Debug.LogWarning("No pin type selected. Please select a pin type before placing a pin.");
        }

    }

    public void OnGeneralPinButtonPressed()
    {
        isGeneralPinButtonPressed = true;
        isHazardPinButtonPressed = false;
        isSamplePinButtonPressed = false;

        HighlightButton(generalPinBackplate);
        UnhighlightButton(hazardPinBackplate);
        UnhighlightButton(samplePinBackplate);
    }

    public void OnHazardPinButtonPressed()
    {
        isGeneralPinButtonPressed = false;
        isHazardPinButtonPressed = true;
        isSamplePinButtonPressed = false;
        HighlightButton(hazardPinBackplate);
        UnhighlightButton(generalPinBackplate);
        UnhighlightButton(samplePinBackplate);
    }

    public void OnSamplePinButtonPressed()
    {
        isGeneralPinButtonPressed = false;
        isHazardPinButtonPressed = false;
        isSamplePinButtonPressed = true;
        HighlightButton(samplePinBackplate);
        UnhighlightButton(generalPinBackplate);
        UnhighlightButton(hazardPinBackplate);
    }

    public void OnCancelPressed()
    {
        isGeneralPinButtonPressed = false;
        isHazardPinButtonPressed = false;
        isSamplePinButtonPressed = false;
        UnhighlightButton(samplePinBackplate);
        UnhighlightButton(generalPinBackplate);
        UnhighlightButton(hazardPinBackplate);
    }

    private void HighlightButton(StatefulInteractable backplate)
    {
        if (backplate != null) backplate.ForceSetToggled(true);

    }

    private void UnhighlightButton(StatefulInteractable backplate)
    {
        if (backplate != null) backplate.ForceSetToggled(false);

    }

    private void PlacePin(GameObject pinPrefab)
    {


        // Get the camera's position (user's head)
        Vector3 headPosition = Camera.main.transform.position;

        // Use the exact head position
        Vector3 pinPosition = headPosition + Camera.main.transform.forward * 0.5f;


        GameObject pin = Instantiate(pinPrefab, pinPosition, Quaternion.identity);
        Debug.LogWarning("creating pin..");
        pin.tag = "Pin";
        pin.transform.SetParent(parentObject, worldPositionStays: true); // Set parent

        if (pinPrefab == pinPointIconPrefab) UnhighlightButton(generalPinBackplate);
        if (pinPrefab == hazardPinPrefab) UnhighlightButton(hazardPinBackplate);
        if (pinPrefab == samplePinPrefab) UnhighlightButton(samplePinBackplate);

        TMP_Text distanceText = pin.GetComponentInChildren<TMP_Text>();
        int evaNumber = evaNumberHandler != null ? evaNumberHandler.getEVANumber() : 0;
        string evaKey = "eva" + evaNumber;

        // Only proceed if we have a valid EVA number
        if (evaNumber == 1 || evaNumber == 2)
        {
            float x = imuDataHandler.GetPosx(evaKey) + pinPosition.x;
            float y = imuDataHandler.GetPosy(evaKey) + pinPosition.y;


            PinRegistry.AddPin(new PinData(x, y, labelText, new string[0], "", type, 0, clip));


            distanceText.text = $"Name:{labelText}\nType: {type}\nX: {x}\nY: {y} ";


        }

    }


}