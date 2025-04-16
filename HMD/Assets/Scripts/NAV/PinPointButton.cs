using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows.Speech;
using TMPro;


public class PinPointButton : MonoBehaviour
{
    [Header("Pin Prefabs")]
    [SerializeField] private GameObject pinPointIconPrefab;
    [SerializeField] private GameObject hazardPinPrefab;
    [SerializeField] private GameObject samplePinPrefab;

    [Header("UI Backplates")]
    [SerializeField] private GameObject generalPinBackplate;
    [SerializeField] private GameObject hazardPinBackplate;
    [SerializeField] private GameObject samplePinBackplate;

    [Header("Ray Interactor")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor; // Drag the right hand or desired ray interactor here

    private int samplePinCount = 0;
    private string labelText;

    [Header("Events")]
    public UnityEvent onGeneralPinClick = new UnityEvent();
    public UnityEvent onHazardPinClick = new UnityEvent();
    public UnityEvent onSamplePinClick = new UnityEvent();

    private bool isGeneralPinButtonPressed = false;
    private bool isHazardPinButtonPressed = false;
    private bool isSamplePinButtonPressed = false;
    private const float VISIBILITY_DISTANCE = 50f;

    private KeywordRecognizer keywordRecognizer;
    private readonly string[] keywords = new string[] { "place pin" };

    private void Start()
    {
        

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
            labelText = "";
            PlacePin(pinPointIconPrefab);
            isGeneralPinButtonPressed = false;
        }
        else if (isHazardPinButtonPressed)
        {
            labelText = "";
            PlacePin(hazardPinPrefab);
            isHazardPinButtonPressed = false;
        }
        else if (isSamplePinButtonPressed)
        {
            samplePinCount++;
            labelText = $"Sample {samplePinCount}: ";
            PlacePin(samplePinPrefab);
            isSamplePinButtonPressed = false;
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

    private void HighlightButton(GameObject backplate)
    {
        if (backplate != null) backplate.SetActive(true);
        Debug.Log("we are hightlighting" + backplate);
    }

    private void UnhighlightButton(GameObject backplate)
    {
        if (backplate != null) backplate.SetActive(false);
        Debug.Log("we are not highlighting" + backplate);
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
