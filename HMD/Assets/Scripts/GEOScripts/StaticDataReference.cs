using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class DataHandlerManager : MonoBehaviour
{
    // Singleton instance
    public static DataHandlerManager Instance { get; private set; }
    
    // References to all data handlers
    public SPECDataHandler specDataHandler;
    public IMUDataHandler imuDataHandler;
    public TELEMETRYDataHandler telemetryDataHandler;
    public TSScConnection tsscConnection;
    public EVANumberHandler evaNumberHandler;
    
    private void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}