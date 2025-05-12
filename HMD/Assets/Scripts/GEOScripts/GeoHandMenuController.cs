using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class HandMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject fieldNotebookObject;
    [SerializeField] private GameObject procedureChecklistObject;
    [SerializeField] private ProcedureChecklistManager procedureManager;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject fieldNotePrefab;
    
    [Header("Button References")]
    [SerializeField] private PressableButton openNotebookButton;
    [SerializeField] private PressableButton startSampleButton;
    
    private void Start()
    {
        // Setup button event handlers
        SetupButtonHandlers();
    }
    
    private void SetupButtonHandlers()
    {
        // Register click events for direct button references
        if (openNotebookButton != null)
        {
            openNotebookButton.OnClicked.AddListener(OnOpenNotebookClicked);
        }
        else
        {
            Debug.LogWarning("Open Notebook button reference is missing!");
        }
        
        if (startSampleButton != null)
        {
            startSampleButton.OnClicked.AddListener(OnStartNewSampleClicked);
        }
        else
        {
            Debug.LogWarning("Start Sample button reference is missing!");
        }
    }
    
    public void OnOpenNotebookClicked()
    {
        if (fieldNotebookObject != null)
        {
            // Toggle the notebook object active
            fieldNotebookObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Field Notebook object reference is missing!");
        }
    }
    
    public void OnStartNewSampleClicked()
    {
        // 1. Make sure the fieldnotebook object is toggled to active
        if (fieldNotebookObject != null)
        {
            fieldNotebookObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Field Notebook object reference is missing!");
        }
        
        // 2. Spawn a fieldnote object
        SpawnPrefab(fieldNotePrefab);
        
        // 3. Set the existing procedure checklist to active
        if (procedureChecklistObject != null)
        {
            procedureChecklistObject.SetActive(true);
            
            // 4. Reset and initialize the procedure manager
            if (procedureManager != null)
            {
                // Ensure the manager resets to the first step and fires the appropriate events
                // We need to access the manager after the checklist is active
                // The ProcedureChecklistManager's Start method will initialize the UI and fire the initial step event
            }
            else
            {
                Debug.LogWarning("Procedure Checklist Manager reference is missing!");
            }
        }
        else
        {
            Debug.LogWarning("Procedure checklist object reference is missing!");
        }
    }
    
    private GameObject SpawnPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab reference is missing!");
            return null;
        }
        
        // Simply instantiate the prefab - positioning is handled by the prefab's own logic
        // that tracks the fieldnotebook object
        GameObject spawnedObject = Instantiate(prefab);
        
        return spawnedObject;
    }
}