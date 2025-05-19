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

public class MapUIController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The panel (or GameObject) that contains your 2D map.")]
    [SerializeField] private GameObject mapPanel;

    [Tooltip("Button that opens the 2D map.")]
    [SerializeField] private StatefulInteractable accessMapButton;

    [Tooltip("Button that closes the 2D map.")]
    [SerializeField] private StatefulInteractable backButton;

   

    [Header("Other Objects to Hide")]
    [Tooltip("List every GameObject (or parent container) you want hidden when viewing the map.")]
    [SerializeField] private GameObject[] objectsToHide;

    private void Awake()
    {
        // initial state
        mapPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
        accessMapButton.gameObject.SetActive(true);

        accessMapButton.OnClicked.AddListener(() => ShowMap()); 
        backButton.OnClicked.AddListener(() => HideMap()); 
    }

    private void ShowMap()
    {
        // show map + back, hide access button
        mapPanel.SetActive(true);
        backButton.gameObject.SetActive(true);
        accessMapButton.gameObject.SetActive(false);

        // hide everything else in the list
        foreach (var go in objectsToHide)
            go.SetActive(false);

    }

    private void HideMap()
    {
        // hide map + back, show access button
        mapPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
        accessMapButton.gameObject.SetActive(true);

        // restore visibility of everything else
        foreach (var go in objectsToHide)
            go.SetActive(true);
    }
}
