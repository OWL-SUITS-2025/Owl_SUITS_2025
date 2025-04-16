using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSScDataHandler : MonoBehaviour
{
    // Reference to the TSSc Connection script
    public TSScConnection TSSc;

    // Variables to store updated JSON data
    private string UIAData;
    private string DCUData;
    private string ROVERData;
    private string SPECData;
    private string TELEMETRYData;
    private string COMMData;
    private string IMUData;

    private string ROVER_TELEMETRYData;
    private string ERRORData;
    private string EVAData;

    // Start is called before the first frame update
    void Start()
    {
        // Check if TSSc reference is set
        if (TSSc == null)
        {
            Debug.LogError("TSSc reference is not set. Please assign the TSScConnection script.");
        }
        else
        {
            Debug.Log("TSScDataHandler script is connected to TSScConnection script.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the UIA data has been updated
        if (TSSc.isUIAUpdated())
        {
            UIAData = TSSc.GetUIAJsonString();
            //Debug.Log("UIA Data Updated: " + UIAData);
        }

        // Check if the DCU data has been updated
        if (TSSc.isDCUUpdated())
        {
            DCUData = TSSc.GetDCUJsonString();
            //Debug.Log("DCU Data Updated: " + DCUData);
        }

        // Check if the ROVER data has been updated
        if (TSSc.isROVERUpdated())
        {
            ROVERData = TSSc.GetROVERJsonString();
            //Debug.Log("ROVER Data Updated: " + ROVERData);
        }

        // Check if the SPEC data has been updated
        if (TSSc.isSPECUpdated())
        {
            SPECData = TSSc.GetSPECJsonString();
            //Debug.Log("SPEC Data Updated: " + SPECData);
        }

        // Check if the TELEMETRY data has been updated
        if (TSSc.isTELEMETRYUpdated())
        {
            TELEMETRYData = TSSc.GetTELEMETRYJsonString();
            // Debug.Log("TELEMETRY Data Updated: " + TELEMETRYData);
        }

        // Check if the COMM data has been updated
        if (TSSc.isCOMMUpdated())
        {
            COMMData = TSSc.GetCOMMJsonString();
            // Debug.Log("COMM Data Updated: " + COMMData);
        }

        // Check if the IMU data has been updated
        if (TSSc.isIMUUpdated())
        {
            IMUData = TSSc.GetIMUJsonString();
            // Debug.Log("IMU Data Updated: " + IMUData);
        }

        // Check if Rover_Telemetry data has been updated
        if (TSSc.isROVER_TELEMETRYUpdated())
        {
            ROVER_TELEMETRYData = TSSc.GetROVER_TELEMETRYJsonString();
            // Debug.Log("ROVER_TELEMETRY Data Updated: " + ROVER_TELEMETRYData);
        }

        // Check if ERROR data has been updated
        if (TSSc.isERRORUpdated())
        {
            ERRORData = TSSc.GetERRORJsonString();
            // Debug.Log("ERROR Data Updated: " + ERRORData);
        }

        // Check if EVA data has been updated
        if (TSSc.isEVAUpdated())
        {
            EVAData = TSSc.GetEVAJsonString();
            // Debug.Log("EVA Data Updated: " + EVAData);
        }
    }

    // Accessors for the stored JSON data

    // Get UIA data
    public string GetUIAData()
    {
        return UIAData;
    }

    // Get DCU data
    public string GetDCUData()
    {
        return DCUData;
    }

    // Get ROVER data
    public string GetROVERData()
    {
        return ROVERData;
    }

    // Get SPEC data
    public string GetSPECData()
    {
        return SPECData;
    }

    // Get TELEMETRY data
    public string GetTELEMETRYData()
    {
        return TELEMETRYData;
    }

    // Get COMM data
    public string GetCOMMData()
    {
        return COMMData;
    }

    // Get IMU data
    public string GetIMUData()
    {
        return IMUData;
    }

    // Get ROVER_TELEMETRY data
    public string GetROVER_TELEMETRYData()
    {
        return ROVER_TELEMETRYData;
    }

    // Get ERROR data
    public string GetERRORData()
    {
        return ERRORData;
    }

    public string GetEVAData()
    {  
        return EVAData;
    }
}
