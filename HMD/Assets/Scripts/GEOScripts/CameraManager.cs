using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class CameraManager : MonoBehaviour
{
    // Singleton instance
    private static CameraManager instance;
    public static CameraManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Try to find existing instance in scene
                instance = FindObjectOfType<CameraManager>();
                
                // If none found, create a new GameObject with CameraManager
                if (instance == null)
                {
                    GameObject cameraManagerObj = new GameObject("CameraManager");
                    instance = cameraManagerObj.AddComponent<CameraManager>();
                    DontDestroyOnLoad(cameraManagerObj);
                    Debug.Log("CameraManager: New singleton instance created.");
                }
            }
            return instance;
        }
    }

    // WebCam components
    private WebCamTexture webCamTexture;
    private bool isWebCamInitialized = false;
    private string cameraDeviceName;

    // Reference counting for active field notes
    private int activeFieldNoteCount = 0;

    // Camera settings
    private const int CAMERA_WIDTH = 1280;
    private const int CAMERA_HEIGHT = 720;
    private const int CAMERA_FPS = 30;

    void Awake()
    {
        // Ensure only one instance exists
        if (instance != null && instance != this)
        {
            Debug.LogWarning("CameraManager: Duplicate instance detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("CameraManager: Singleton instance initialized.");
    }

    /// <summary>
    /// Register a field note as needing camera access
    /// </summary>
    public void RegisterFieldNote()
    {
        activeFieldNoteCount++;
        Debug.Log($"CameraManager: Field note registered. Active count: {activeFieldNoteCount}");

        // Initialize camera if this is the first field note
        if (activeFieldNoteCount == 1 && !isWebCamInitialized)
        {
            InitializeWebCam();
        }
    }

    /// <summary>
    /// Unregister a field note (no longer needs camera access)
    /// </summary>
    public void UnregisterFieldNote()
    {
        activeFieldNoteCount = Mathf.Max(0, activeFieldNoteCount - 1);
        Debug.Log($"CameraManager: Field note unregistered. Active count: {activeFieldNoteCount}");

        // Stop camera if no field notes are active
        if (activeFieldNoteCount == 0 && isWebCamInitialized)
        {
            StopWebCam();
        }
    }

    /// <summary>
    /// Initialize the WebCam for photo capture
    /// </summary>
    private void InitializeWebCam()
    {
        try
        {
            // Get available camera devices
            WebCamDevice[] devices = WebCamTexture.devices;
            
            if (devices.Length == 0)
            {
                Debug.LogWarning("CameraManager: No camera devices found. Photo capture will not be available.");
                return;
            }

            // Use the first available camera (HoloLens typically has one main camera)
            cameraDeviceName = devices[0].name;
            Debug.Log($"CameraManager: Initializing camera: {cameraDeviceName}");

            // Create WebCamTexture with reasonable resolution for HoloLens
            webCamTexture = new WebCamTexture(cameraDeviceName, CAMERA_WIDTH, CAMERA_HEIGHT, CAMERA_FPS);
            
            // Start the camera
            webCamTexture.Play();
            isWebCamInitialized = true;
            
            Debug.Log("CameraManager: WebCam initialized successfully for photo capture.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CameraManager: Failed to initialize WebCam: {e.Message}");
            isWebCamInitialized = false;
        }
    }

    /// <summary>
    /// Stop and cleanup the WebCam
    /// </summary>
    private void StopWebCam()
    {
        try
        {
            if (webCamTexture != null)
            {
                if (webCamTexture.isPlaying)
                {
                    webCamTexture.Stop();
                    Debug.Log("CameraManager: WebCam stopped.");
                }
                
                DestroyImmediate(webCamTexture);
                webCamTexture = null;
                Debug.Log("CameraManager: WebCam texture destroyed.");
            }
            
            isWebCamInitialized = false;
            cameraDeviceName = null;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CameraManager: Error stopping WebCam: {e.Message}");
        }
    }

    /// <summary>
    /// Capture a photo from the camera and return a unique Texture2D
    /// </summary>
    /// <returns>New Texture2D with the captured image, or null if capture failed</returns>
    public Texture2D CapturePhoto()
    {
        if (!isWebCamInitialized || webCamTexture == null || !webCamTexture.isPlaying)
        {
            Debug.LogError("CameraManager: Cannot capture photo - WebCam not initialized or not playing.");
            return null;
        }

        try
        {
            // Get the current frame from the WebCam
            Color32[] pixels = webCamTexture.GetPixels32();
            
            // Create a new Texture2D with the same dimensions as the WebCam
            Texture2D capturedTexture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
            
            // Set the pixels from the WebCam to our new texture
            capturedTexture.SetPixels32(pixels);
            capturedTexture.Apply();

            Debug.Log($"CameraManager: Photo captured successfully. Resolution: {capturedTexture.width}x{capturedTexture.height}");
            return capturedTexture;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CameraManager: Error capturing photo: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Check if camera is ready for photo capture
    /// </summary>
    /// <returns>True if camera is ready, false otherwise</returns>
    public bool IsCameraReady()
    {
        return isWebCamInitialized && webCamTexture != null && webCamTexture.isPlaying;
    }

    /// <summary>
    /// Get camera status information for debugging
    /// </summary>
    /// <returns>String with camera status information</returns>
    public string GetCameraStatus()
    {
        if (!isWebCamInitialized)
            return "Camera not initialized";
        
        if (webCamTexture == null)
            return "WebCamTexture is null";
        
        if (!webCamTexture.isPlaying)
            return "Camera not playing";
        
        return $"Camera ready - {cameraDeviceName} ({webCamTexture.width}x{webCamTexture.height} @ {CAMERA_FPS}fps)";
    }

    void OnDestroy()
    {
        // Clean up camera resources when manager is destroyed
        if (webCamTexture != null)
        {
            StopWebCam();
        }
        
        // Clear singleton reference
        if (instance == this)
        {
            instance = null;
        }
        
        Debug.Log("CameraManager: Instance destroyed and resources cleaned up.");
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Handle application pause/resume for camera
        if (isWebCamInitialized && webCamTexture != null)
        {
            if (pauseStatus)
            {
                // Pause camera when application is paused
                if (webCamTexture.isPlaying)
                {
                    webCamTexture.Pause();
                    Debug.Log("CameraManager: Camera paused due to application pause.");
                }
            }
            else
            {
                // Resume camera when application resumes
                if (!webCamTexture.isPlaying)
                {
                    webCamTexture.Play();
                    Debug.Log("CameraManager: Camera resumed after application resume.");
                }
            }
        }
    }
}