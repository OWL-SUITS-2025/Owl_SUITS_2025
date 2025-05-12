using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// IMPORTANT: The EVAWrapper class is used to handle the additional
// wrapper layer in the received JSON string. The JSON structure includes
// an outer layer named "eva," and the EVAWrapper helps to
// unwrap and deserialize the data within that layer.
[System.Serializable]
public class EVAWrapper
{
    // The eva field represents the inner data structure containing
    // the actual EVA information we want to extract and use.
    public EVAData eva;
}

[System.Serializable]
public class EVAData
{
    public bool started;
    public bool paused;
    public bool completed;
    public int total_time;
    public EVAComponentData uia;
    public EVAComponentData dcu;
    public EVAComponentData rover;
    public EVAComponentData spec;
}

[System.Serializable]
public class EVAComponentData
{
    public bool started;
    public bool completed;
    public int time;
}

public class EVADataHandler : MonoBehaviour
{
    // Reference to the TSSc Data Handler script
    public TSScDataHandler TSS;
    public EVAWrapper evaWrapper;

    // Start is called before the first frame update
    void Start()
    {
        evaWrapper = new EVAWrapper();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEVAData();
    }

    void UpdateEVAData()
    {
        // Store the JSON string for parsing
        string evaJsonString = TSS.GetEVAData();
        
        // Parse JSON string into EVAWrapper object
        // This stores the JSON value into the EVAWrapper object
        evaWrapper = JsonUtility.FromJson<EVAWrapper>(evaJsonString);

        // Access specific values
        // Debug.Log($"EVA Started: {GetEVAStarted()}");
        // Debug.Log($"EVA Paused: {GetEVAPaused()}");
        // Debug.Log($"EVA Completed: {GetEVACompleted()}");
        // Debug.Log($"EVA Total Time: {GetEVATotalTime()}");
        // Debug.Log($"UIA Started: {GetComponentStarted("uia")}");
        // Debug.Log($"UIA Completed: {GetComponentCompleted("uia")}");
        // Debug.Log($"UIA Time: {GetComponentTime("uia")}");
        // Debug.Log($"DCU Started: {GetComponentStarted("dcu")}");
        // Debug.Log($"DCU Completed: {GetComponentCompleted("dcu")}");
        // Debug.Log($"DCU Time: {GetComponentTime("dcu")}");
        // Debug.Log($"Rover Started: {GetComponentStarted("rover")}");
        // Debug.Log($"Rover Completed: {GetComponentCompleted("rover")}");
        // Debug.Log($"Rover Time: {GetComponentTime("rover")}");
        // Debug.Log($"Spec Started: {GetComponentStarted("spec")}");
        // Debug.Log($"Spec Completed: {GetComponentCompleted("spec")}");
        // Debug.Log($"Spec Time: {GetComponentTime("spec")}");
    }

    // Use these functions in other scripts to access the live EVA data
    public bool GetEVAStarted()
    {
        return evaWrapper.eva.started;
    }

    public bool GetEVAPaused()
    {
        return evaWrapper.eva.paused;
    }

    public bool GetEVACompleted()
    {
        return evaWrapper.eva.completed;
    }

    public int GetEVATotalTime()
    {
        return evaWrapper.eva.total_time;
    }

    public bool GetComponentStarted(string component)
    {
        if (component == "uia" || component == "dcu" || component == "rover" || component == "spec")
        {
            // Check which component data is requested
            EVAComponentData componentData = null;
            
            switch(component)
            {
                case "uia":
                    componentData = evaWrapper.eva.uia;
                    break;
                case "dcu":
                    componentData = evaWrapper.eva.dcu;
                    break;
                case "rover":
                    componentData = evaWrapper.eva.rover;
                    break;
                case "spec":
                    componentData = evaWrapper.eva.spec;
                    break;
            }

            return componentData.started;
        }
        else
        {
            Debug.LogWarning("Invalid component specified. Use 'uia', 'dcu', 'rover', or 'spec'.");
            return false; // Or another default value
        }
    }

    public bool GetComponentCompleted(string component)
    {
        if (component == "uia" || component == "dcu" || component == "rover" || component == "spec")
        {
            // Check which component data is requested
            EVAComponentData componentData = null;
            
            switch(component)
            {
                case "uia":
                    componentData = evaWrapper.eva.uia;
                    break;
                case "dcu":
                    componentData = evaWrapper.eva.dcu;
                    break;
                case "rover":
                    componentData = evaWrapper.eva.rover;
                    break;
                case "spec":
                    componentData = evaWrapper.eva.spec;
                    break;
            }

            return componentData.completed;
        }
        else
        {
            Debug.LogWarning("Invalid component specified. Use 'uia', 'dcu', 'rover', or 'spec'.");
            return false; // Or another default value
        }
    }

    public int GetComponentTime(string component)
    {
        if (component == "uia" || component == "dcu" || component == "rover" || component == "spec")
        {
            // Check which component data is requested
            EVAComponentData componentData = null;
            
            switch(component)
            {
                case "uia":
                    componentData = evaWrapper.eva.uia;
                    break;
                case "dcu":
                    componentData = evaWrapper.eva.dcu;
                    break;
                case "rover":
                    componentData = evaWrapper.eva.rover;
                    break;
                case "spec":
                    componentData = evaWrapper.eva.spec;
                    break;
            }

            return componentData.time;
        }
        else
        {
            Debug.LogWarning("Invalid component specified. Use 'uia', 'dcu', 'rover', or 'spec'.");
            return 0; // Or another default value
        }
    }
}