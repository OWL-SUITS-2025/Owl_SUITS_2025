using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModeSwitching : MonoBehaviour
{
    public GameObject egressHUD;
    public GameObject geoHUD;
    public GameObject navigationHUD;
    public GameObject ingressHUD;

    public TextMeshPro modeTextMeshPro;

    private void Start()
    {
        // Disable all HUDs initially
        DisableAllHUDs();
    }

    private void DisableAllHUDs()   
    {
        egressHUD.SetActive(false);
        geoHUD.SetActive(false);
        navigationHUD.SetActive(false);
        ingressHUD.SetActive(false);
    }

    public void ToggleEgressMode()
    {   if (TaskManager.Instance != null)
        {
            TaskManager.Instance.SetCurrentTaskIndex(0); // Reset the task index for Ingress/Egress
        }
        else
        {
            Debug.LogError("TaskManager Instance is null.  Egress task index not reset.");
        }
        // TaskManager.Instance.SetCurrentTaskIndex(0); // Reset the task index for Ingress/Egress
        DisableAllHUDs();
        egressHUD.SetActive(true);
        
        UpdateModeText("EGRESS IN PROGRESS", Color.green);
    }

    public void ToggleGeoMode()
    {
        
        DisableAllHUDs();
        geoHUD.SetActive(true);
        UpdateModeText("GEO IN PROGRESS", Color.red);
    }

    public void ToggleNavigationMode()
    {
        DisableAllHUDs();
        navigationHUD.SetActive(true);
        UpdateModeText("NAVIGATION IN PROGRESS", Color.blue);
    }

    public void ToggleIngressMode()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.SetCurrentTaskIndex(0); // Reset the task index for Ingress/Egress
        }
        else
        {
            Debug.LogError("TaskManager Instance is null.  Egress task index not reset.");
        }
        // TaskManager.Instance.SetCurrentTaskIndex(0); // Reset the task index for Ingress/Egress
        DisableAllHUDs();
        ingressHUD.SetActive(true);
        UpdateModeText("INGRESS IN PROGRESS", Color.magenta);
    }

    private void UpdateModeText(string modeText, Color textColor)
    {
        if (modeTextMeshPro != null)
        {
            modeTextMeshPro.text = modeText;
            modeTextMeshPro.color = textColor;
        }
    }
}