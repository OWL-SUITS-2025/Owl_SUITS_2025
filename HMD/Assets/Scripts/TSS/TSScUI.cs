// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;
// using Microsoft.MixedReality.Toolkit.UX; // MRTK3 UX namespace
// using UnityEngine.Events; // For Unity events

// // MRTK3 uses PressableButton or StatefulInteractable from the MRTK UX namespace
// public class TSScUI : StatefulInteractable
// {
//     [Header("Connection")]
//     [SerializeField]
//     private TSScConnection TSSc;

//     [Header("UI References")]
//     [SerializeField]
//     private TMP_Text InputFieldUrl;

//     [Header("Scene Management")]
//     [SerializeField]
//     private List<GameObject> objectsToActivate;
//     [SerializeField]
//     private GameObject objectToDeactivate;
//     [SerializeField]
//     private float deactivationDelay = 1.0f;

//     // Change to UnityEvent for proper compatibility
//     public UnityEvent OnClickedEvent = new UnityEvent();
    
//     protected override void OnEnable()
//     {
//         base.OnEnable();
//         // In MRTK3, we use the OnClicked event from StatefulInteractable
//         this.OnClicked.AddListener(Connect_Button);
//     }
    
//     protected void OnMouseDown()
//     {
//         // Keep mouse click support for non-MR testing
//         Connect_Button();
//         OnClickedEvent?.Invoke();
//     }
    
//     protected override void OnDisable()
//     {
//         base.OnDisable();
//         // Remove the listener to prevent memory leaks
//         this.OnClicked.RemoveListener(Connect_Button);
//     }

//     private void Connect_Button()
//     {
//         if (TSSc == null)
//         {
//             Debug.LogError($"{nameof(TSScUI)}: TSScConnection reference is missing!");
//             return;
//         }

//         // Start connection process
//         StartCoroutine(HandleConnection());
//     }

//     private IEnumerator HandleConnection()
//     {
//         // Get URL from input field or use default
//         string host = "127.0.0.1"; // Local default
        
//         Debug.Log($"Connecting to: {host}");
//         TSSc.ConnectToHost(host, 6);

//         // Activate specified objects
//         foreach (GameObject obj in objectsToActivate)
//         {
//             if (obj != null)
//                 obj.SetActive(true);
//         }

//         // Wait for deactivation delay
//         if (objectToDeactivate != null)
//         {
//             yield return new WaitForSeconds(deactivationDelay);
//             objectToDeactivate.SetActive(false);
//         }
//     }
// }