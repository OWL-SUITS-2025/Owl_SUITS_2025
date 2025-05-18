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

    [Header("Buttons")]
    [SerializeField] private StatefulInteractable generalPinBackplate;
    [SerializeField] private StatefulInteractable hazardPinBackplate;
    [SerializeField] private StatefulInteractable samplePinBackplate;

    [Header("Ray Interactor")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor; // Drag the right hand or desired ray interactor here

    private int samplePinCount = 0;
    private int generalPinCount = 0;
    private int hazardPinCount = 0;
    private string labelText;

    public TextMeshProUGUI namePin; // text box (name for pin)

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
    private string tags;

 

    public string keyword = "Place Pin";


    private KeywordRecognitionSubsystem keywordRecognitionSubsystem = null;

    [SerializeField] TextMeshPro recordLabel; // “Record / Stop” text
    [SerializeField] TextMeshPro plate;  // icon

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

        if (recordLabel) recordLabel.text = "Stop";
    }

    void StopRec()
    {
        Microphone.End(micDevice);
        recording = false;

        if (recordLabel) recordLabel.text = "Record";

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
            if (namePin != null)
            {
                labelText = namePin.text;
            }
            else
            {
                labelText = $"General {generalPinCount}: ";
            }
            tags = "General";
            PlacePin(pinPointIconPrefab);
            isGeneralPinButtonPressed = false;
        }
        else if (isHazardPinButtonPressed)
        {
            hazardPinCount++;
            if (namePin != null)
            {
                labelText = namePin.text;
            }
            else
            {
                labelText = $"Hazard {hazardPinCount}: ";
            }
            tags = "Hazard";
            PlacePin(hazardPinPrefab);
            isHazardPinButtonPressed = false;
        }
        else if (isSamplePinButtonPressed)
        {
            samplePinCount++;
            if (namePin != null)
            {
                labelText = namePin.text;
            }
            else
            {
                labelText = $"Sample {samplePinCount}: ";
            }
           
            tags = "Sample";
            PlacePin(samplePinPrefab);
            isSamplePinButtonPressed = false;
        }
        
    }

    public void OnGeneralPinButtonPressed()
    {
        isGeneralPinButtonPressed = true;
        isHazardPinButtonPressed = false;
        isSamplePinButtonPressed = false;
        PinRegistry.AddPin(new PinData(-5879, -10000, labelText, new string[0], "", "general", 0, clip));
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
        if (recording)
        {
            StopRec();
        }
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
        if (rayInteractor == null)
        {
            Debug.LogWarning("Ray Interactor not assigned.");
            return;
        }

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Vector3 hitPoint = hit.point;
            float headsetHeight = Camera.main.transform.position.y;
            Vector3 pinPosition = new Vector3(hitPoint.x, headsetHeight - 0.5f, hitPoint.z);

            if (!IsPinTooCloseToExistingPin(pinPosition))
            {
                GameObject pin = Instantiate(pinPrefab, pinPosition, Quaternion.identity);
                pin.tag = "Pin";

                TMP_Text distanceText = pin.GetComponentInChildren<TMP_Text>();

                if (pinPrefab == pinPointIconPrefab) UnhighlightButton(generalPinBackplate);
                if (pinPrefab == hazardPinPrefab) UnhighlightButton(hazardPinBackplate);
                if (pinPrefab == samplePinPrefab) UnhighlightButton(samplePinBackplate);



     

                int evaNumber = evaNumberHandler != null ? evaNumberHandler.getEVANumber() : 0;
                string evaKey = "eva" + evaNumber;

                // Only proceed if we have a valid EVA number
                if (evaNumber == 1 || evaNumber == 2)
                {
                    float x = imuDataHandler.GetPosx(evaKey) + hitPoint.x;          // or just imu.getX()
                    float y = imuDataHandler.GetPosy(evaKey) + hitPoint.y;          // same idea for Y

                    PinRegistry.AddPin(new PinData(x, y, labelText, new string[0], "", tags, 0, clip));
                    distanceText.text = $"Name: {labelText}\nType: {tags}X: {x}\nY: {y} ";



                }
            }
        }
    }



    private bool IsPinTooCloseToExistingPin(Vector3 pinPosition)
    {
        float minDistance = 0.5f;
        foreach (GameObject existingPin in GameObject.FindGameObjectsWithTag("Pin"))
        {
            if (Vector3.Distance(pinPosition, existingPin.transform.position) < minDistance)
            {
                return true;
            }
        }
        return false;
    }
}
