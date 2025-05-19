using System.Collections.Generic;
using UnityEngine;

/// Central list that remembers every pin you drop.
/// Formatted to be compatible with the backend API
[System.Serializable]
public struct PinData
{
    public float x, y;           // 2‑D coords matching API format
    public string name;          // Name of the POI
    public string[] tags;        // Tags as string array
    public string description;   // Description of the POI 
    public string type;          // Type of the POI
    public int audioId;          // ID of the associated audio clip

    // Reference to the local AudioClip (not sent to API)
    [System.NonSerialized]
    public AudioClip localAudioClip;

    public PinData(float xPos, float yPos, string pinName, string[] pinTags, string pinDescription, string pinType, int pinAudioId, AudioClip clip = null)
    {
        x = xPos;
        y = yPos;
        name = pinName;
        tags = pinTags;
        description = pinDescription;
        type = pinType;
        audioId = pinAudioId;
        localAudioClip = clip;
    }

    // Helper method to convert string tags to string array
    public static string[] ParseTags(string tagString)
    {
        if (string.IsNullOrEmpty(tagString))
            return new string[0];

        return tagString.Split(',');
    }
}

public static class PinRegistry
{
    // Anyone can read; only your pin‑placing code should write.
    public static readonly List<PinData> Pins = new List<PinData>();

    public static void AddPin(PinData data) => Pins.Add(data);
    public static void Clear() => Pins.Clear();
}