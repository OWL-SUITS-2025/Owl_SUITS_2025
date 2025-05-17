using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public class VoiceRecordingHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private PressableButton playPauseToggle;
    
    // Audio recording variables
    private AudioClip recordedClip;
    private bool isRecording = false;
    private bool isPlaying = false;
    private AudioSource audioSource;
    
    // Recording settings
    private int recordingFrequency = 44100; // Standard audio frequency
    private string microphoneName;
    
    // Event to notify when recording starts and stops
    public event Action OnRecordingStarted;
    public event Action OnRecordingStopped;
    
    private void Start()
    {
        // Initialize AudioSource for playback
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Get the default microphone device
        if (Microphone.devices.Length > 0)
        {
            microphoneName = Microphone.devices[0];
            Debug.Log($"Using microphone: {microphoneName}");
        }
        else
        {
            Debug.LogError("No microphone device found!");
        }
        
        // Setup button event listener
        if (playPauseToggle != null)
        {
            playPauseToggle.OnClicked.AddListener(TogglePlayPause);
        }
        else
        {
            Debug.LogError("Play/Pause toggle button reference not set!");
        }
    }
    
    // Start recording audio from the microphone
    public void StartRecording()
    {
        if (isRecording || microphoneName == null)
        {
            return;
        }
        
        Debug.Log("Starting voice recording...");
        
        // Start recording audio - null means record without time limit
        recordedClip = Microphone.Start(microphoneName, false, 300, recordingFrequency); // Max 5 minutes
        isRecording = true;
        
        // Notify listeners that recording has started
        OnRecordingStarted?.Invoke();
    }
    
    // Stop recording and save the clip
    public void StopRecording()
    {
        if (!isRecording)
        {
            return;
        }
        
        Debug.Log("Stopping voice recording...");
        
        // Get the position where the recording stopped
        int position = Microphone.GetPosition(microphoneName);
        
        // Stop the microphone
        Microphone.End(microphoneName);
        isRecording = false;
        
        // If no audio was recorded, don't proceed
        if (position <= 0)
        {
            Debug.LogWarning("No audio was recorded or recording failed.");
            recordedClip = null;
            return;
        }
        
        // Create a new clip with just the recorded data
        AudioClip originalClip = recordedClip;
        recordedClip = AudioClip.Create(originalClip.name, position, originalClip.channels, recordingFrequency, false);
        
        // Copy the recorded samples to the new clip
        float[] samples = new float[position * originalClip.channels];
        originalClip.GetData(samples, 0);
        recordedClip.SetData(samples, 0);
        
        // Set the clip to the audio source for playback
        audioSource.clip = recordedClip;
        
        Debug.Log($"Voice recording complete. Length: {recordedClip.length:F2} seconds");
        
        // Notify listeners that recording has stopped
        OnRecordingStopped?.Invoke();
    }
    
    // Toggle between playing and pausing the recorded audio
    public void TogglePlayPause()
    {
        if (recordedClip == null)
        {
            Debug.LogWarning("No recording available to play.");
            return;
        }
        
        if (isPlaying)
        {
            // Pause the audio
            audioSource.Pause();
            isPlaying = false;
            Debug.Log("Voice recording paused.");
        }
        else
        {
            // If we were at the end, restart from the beginning
            if (audioSource.time >= recordedClip.length - 0.1f)
            {
                audioSource.time = 0;
            }
            
            // Play the audio
            audioSource.Play();
            isPlaying = true;
            Debug.Log("Voice recording playing.");
        }
        
        // Update the button's visual state if needed
        if (playPauseToggle != null)
        {
            StatefulInteractable statefulInteractable = playPauseToggle.GetComponent<StatefulInteractable>();
            if (statefulInteractable != null)
            {
                statefulInteractable.ForceSetToggled(isPlaying);
            }
        }
    }
    
    // Check if a recording exists
    public bool HasRecording()
    {
        return recordedClip != null;
    }
    
    // Clean up when this object is destroyed
    private void OnDestroy()
    {
        // Make sure to stop recording if this object is destroyed while recording
        if (isRecording)
        {
            Microphone.End(microphoneName);
        }
    }
}