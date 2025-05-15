using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Windows.Speech;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;


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

    private bool isGeneralPinButtonPressed = false;
    private bool isHazardPinButtonPressed = false;
    private bool isSamplePinButtonPressed = false;
    private const float VISIBILITY_DISTANCE = 50f;
    private string tags;

    private KeywordRecognizer keywordRecognizer;
    private readonly string[] keywords = new string[] { "place pin" };

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


        keywordRecognizer = new KeywordRecognizer(keywords);
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

    }

    private void OnDestroy()
    {
        if (keywordRecognizer != null)
        {
            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (args.text.ToLower() == "place pin")
        {
            OnPlacePinCommand();
        }
    }

    public void OnPlacePinCommand()
    {
        if (isGeneralPinButtonPressed)
        {
            
            PlacePin(pinPointIconPrefab);
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
        float posx = imuDataHandler.GetPosx("eva1");
        float posy = imuDataHandler.GetPosy("eva1");
        PinRegistry.AddPin(new PinData("pin",posx, posy, labelText, tags, clip));
    }

    public void OnGeneralPinButtonPressed()
    {
        isGeneralPinButtonPressed = true;
        isHazardPinButtonPressed = false;
        isSamplePinButtonPressed = false;
        PinRegistry.AddPin(new PinData("pin", -5879, -10000, labelText, "general", null));
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

                StartCoroutine(UpdatePinState(pin.transform, distanceText, labelText));
            }
        }
    }

    private IEnumerator UpdatePinState(Transform pinTransform, TMP_Text distanceText, string pinlabelText)
    {
        Renderer pinRenderer = pinTransform.GetComponentInChildren<Renderer>();

        while (true)
        {
            float distance = Vector3.Distance(pinTransform.position, Camera.main.transform.position);

            distanceText.text = string.IsNullOrEmpty(pinlabelText) ?
                $"{distance:F2}m" :
                $"{pinlabelText}  {distance:F2}m";

            distanceText.transform.rotation = Quaternion.LookRotation(distanceText.transform.position - Camera.main.transform.position);
            pinRenderer.enabled = (distance <= VISIBILITY_DISTANCE);

            yield return null;
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
