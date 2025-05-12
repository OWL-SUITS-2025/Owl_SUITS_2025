using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// IMPORTANT: The ErrorWrapper class is used to handle the additional
// wrapper layer in the received JSON string. The JSON structure includes
// an outer layer named "error," and the ErrorWrapper helps to
// unwrap and deserialize the data within that layer.
[System.Serializable]
public class ERRORWrapper
{
    // The error field represents the inner data structure containing
    // the actual error information we want to extract and use.
    public ERRORData error;
}

[System.Serializable]
public class ERRORData
{
    public bool fan_error;
    public bool oxy_error;
    public bool pump_error;
}

public class ERRORDataHandler : MonoBehaviour
{
    // Reference to the TSSc Data Handler script
    public TSScDataHandler TSS;
    public ERRORWrapper errorWrapper;

    // Start is called before the first frame update
    void Start()
    {
        errorWrapper = new ERRORWrapper();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateERRORData();
    }

    void UpdateERRORData()
    {
        // Store the JSON string for parsing
        string errorJsonString = TSS.GetERRORData();

        // Parse JSON string into ErrorWrapper object
        // This stores the JSON value into the ErrorWrapper object
        errorWrapper = JsonUtility.FromJson<ERRORWrapper>(errorJsonString);

        // Access specific values
        // Debug.Log($"fan_error: {GetFanError()}");
        // Debug.Log($"oxy_error: {GetOxyError()}");
        // Debug.Log($"pump_error: {GetPumpError()}");
    }

    // Use these functions in other scripts to access the live error data
    public bool GetFanError()
    {
        return errorWrapper.error.fan_error;
    }
    
    public bool GetOxyError()
    {
        return errorWrapper.error.oxy_error;
    }
    
    public bool GetPumpError()
    {
        return errorWrapper.error.pump_error;
    }
}