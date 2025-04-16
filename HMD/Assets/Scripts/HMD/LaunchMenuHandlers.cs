using UnityEngine;
using TMPro;
using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
// using Microsoft.MixedReality.Toolkit.Input;

public class LaunchMenuHandlers : MonoBehaviour
{
    [Header("Button Colors")]
    public Color selectedColor = Color.green;
    public Color unselectedColor = Color.white;

    
     [Header("Text References")]
    public TextMeshPro EV1Text;
    public TextMeshPro EV2Text;

    // TSSc Connection
    [Header("Connection References")]
    public TSScConnection TSSc;
    public EVANumberHandler EVAnum;
    // UI Input
    public TMP_Text InputFieldUrl;


    [Header("Scene Management")]
    // Reference to the game objects you want to activate
    public GameObject[] objectsToActivate;

    // Reference to the game object you want to deactivate
    public GameObject objectToDeactivate;

    // Delay in seconds before deactivating the object
    public float deactivationDelay = 1.0f;

    // Maximum time to wait for the connection (in seconds)
    public float connectionTimeout = 5.0f;


    [Header("MRTK3 Button References")]
    public StatefulInteractable EV1Button;
    public StatefulInteractable EV2Button;
    public StatefulInteractable ConnectButton;
    private void Start()
    {
        // Initialize MRTK3 button events
        if (EV1Button != null)
            EV1Button.OnClicked.AddListener(() => EV1_Pressed());
        if (EV2Button != null)
            EV2Button.OnClicked.AddListener(() => EV2_Pressed());
        if (ConnectButton != null)
            ConnectButton.OnClicked.AddListener(() => Connect_Button());

    }
    public void EV1_Pressed()
    {
        EV1Text.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(selectedColor)}>EV1";
        EV2Text.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(unselectedColor)}>EV2";
    }
    public void EV2_Pressed()
    {
        EV2Text.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(selectedColor)}>EV2";
        EV1Text.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(unselectedColor)}>EV1";
    }
    // On Connect Button Press
    public void Connect_Button()
    {
        // Get URL in Text Field
        string host;

        int team;
        // Use this to HARDCODE the server address

        // Using NASA Rockyard IP:
        //host = "168.4.146.212";

        // Using Local Server:
        host = "127.0.0.1";
        
        // This is for TSS, make sure ur in team in web panel 
        // off by one, this will be team 7 on the panel
        team = 6;

        // Print Hostname to Logs
        Debug.Log("Button Pressed: " + host);

        // Connect to TSSc at that Host
        TSSc.ConnectToHost(host, team);

        // Start a coroutine to wait for the connection and deactivate the object
        StartCoroutine(WaitForConnectionAndDeactivate());
    }

    IEnumerator WaitForConnectionAndDeactivate()
    {
        float elapsedTime = 0f;

        // Wait until the connection is established or the timeout is reached
        while (!TSSc.IsConnected() && elapsedTime < connectionTimeout)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Check if the connection is successful
        if (TSSc.IsConnected())
        {
            while (EVAnum.getEVANumber() == 0)
            {
                yield return null;
            }
            // Start a coroutine to deactivate the object after a delay
            StartCoroutine(DeactivateObjectDelayed());

            // Activate the array of game objects
            ActivateObjects();
            
        }
        else
        {
            Debug.Log("Connection failed. Object will not be deactivated.");
        }
    }

    IEnumerator DeactivateObjectDelayed()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(deactivationDelay);

        // Deactivate the object
        if (objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
        }
    }

    void ActivateObjects()
    {
        // Loop through the array of game objects and activate each one
        foreach (GameObject obj in objectsToActivate)
        {
            obj.SetActive(true);
        }
    }

    public void Disconnect_Button()
    {
        // Disconnects from TSS when
        TSSc.DisconnectFromHost();
    }
    private void OnDestroy()
    {
        // Clean up event listeners
        if (EV1Button != null)
            EV1Button.OnClicked.RemoveListener(EV1_Pressed);
        if (EV2Button != null)
            EV2Button.OnClicked.RemoveListener(EV2_Pressed);
        if (ConnectButton != null)
            ConnectButton.OnClicked.RemoveListener(Connect_Button);
    }
}