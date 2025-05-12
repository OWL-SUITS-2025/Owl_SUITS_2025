using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class GeoHandler : MonoBehaviour
{
    // Singleton pattern
    public static GeoHandler Instance { get; private set; }
    
    // References to all data handlers
    public SPECDataHandler specDataHandler;
    public IMUDataHandler imuDataHandler;
    public TELEMETRYDataHandler telemetryDataHandler;
    public TSScConnection tsscConnection;
    public EVANumberHandler evaNumberHandler;
    
    // Add any other shared resources here
    
    private void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Makes this object persistent across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
}