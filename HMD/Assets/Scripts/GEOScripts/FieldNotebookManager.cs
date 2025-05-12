using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class FieldNotebookManager : MonoBehaviour
{
    // Reference to the Field Note prefab
    public GameObject fieldNotePrefab;
    
    // Reference to the Add Button
    public PressableButton addButton;
    
    // Reference to the Close Button
    public PressableButton closeButton;
    
    // Spacing between Field Notes
    public float horizontalSpacing = 0.3f;
    
    // List to track spawned Field Notes
    private List<GameObject> spawnedFieldNotes = new List<GameObject>();
    
    void Start()
    {
        // Verify references
        if (fieldNotePrefab == null)
        {
            Debug.LogError("Field Note Prefab is not assigned. Please assign it in the inspector.");
        }
        
        if (addButton == null)
        {
            Debug.LogError("Add Button is not assigned. Please assign it in the inspector.");
        }
        else
        {
            // Register Add Button click event
            addButton.OnClicked.AddListener(SpawnNewFieldNote);
        }
        
        if (closeButton == null)
        {
            Debug.LogError("Close Button is not assigned. Please assign it in the inspector.");
        }
        else
        {
            // Register Close Button click event
            closeButton.OnClicked.AddListener(CloseNotebook);
        }
    }
    
    public void SpawnNewFieldNote()
    {
        if (fieldNotePrefab == null) return;
        
        // Get the Field Notebook's position, rotation, and scale
        Vector3 notebookPosition = transform.position;
        Quaternion notebookRotation = transform.rotation;
        
        // Calculate position for the new Field Note (to the right of the notebook)
        // Add some base distance plus additional spacing per existing field note
        float baseDistance = 0.3f;  // Base distance from notebook
        float offsetDistance = baseDistance + (horizontalSpacing * spawnedFieldNotes.Count);
        
        // Position to the right of the notebook
        Vector3 spawnPosition = notebookPosition + (transform.right * offsetDistance);
        
        // Instantiate the new Field Note as a sibling of the FieldNoteBook
        GameObject newFieldNote = Instantiate(fieldNotePrefab, spawnPosition, notebookRotation, transform.parent);
        
        // Make sure the new field note has a unique name
        newFieldNote.name = "FieldNote_" + (spawnedFieldNotes.Count + 1);
        
        // Add to our list of spawned Field Notes
        spawnedFieldNotes.Add(newFieldNote);
        
        Debug.Log("New Field Note spawned. Total count: " + spawnedFieldNotes.Count);
    }
    
    public void CloseNotebook()
    {
        // Deactivate this GameObject when close button is pressed
        gameObject.SetActive(false);
        Debug.Log("Field Notebook closed.");
    }
}