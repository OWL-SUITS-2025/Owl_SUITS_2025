using System.Collections.Generic;
using UnityEngine;

/// Central list that remembers every pin you drop.
/// Feel free to extend PinData with timestamp, label, etc.
public struct PinData
{
    public float mapX, mapY;      // 2‑D coords in your map’s system
    public string tags;
    public string name;
    public AudioClip clip;
    public string type;


    public PinData(string ty, float x, float y, string n, string t, AudioClip c)
    {
        mapX = x; mapY = y; tags = t; name = n; clip = c; type = ty;
    }
    public PinData(string ty, float x, float y, string n)
    {
        mapX = x; mapY = y; tags = ""; name = n; clip = null; type = ty;
    }
   
}

public static class PinRegistry
{
    // Anyone can read; only your pin‑placing code should write.
    public static readonly List<PinData> Pins = new List<PinData>();

    public static void AddPin(PinData data) => Pins.Add(data);
    public static void Clear() => Pins.Clear();
}
