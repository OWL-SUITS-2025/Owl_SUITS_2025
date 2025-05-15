using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Attach this to the same GameObject that shows the 2-D map
public class MapPinOverlay : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private RectTransform mapRect;   // drag the map quad/image
    [SerializeField] private GameObject pinPrefab; // your UX-icon prefab

    [Header("Refresh")]
    [SerializeField] private float refreshInterval = 1f;

    /* ─── real-world coordinate extents you gave ─── */
    const float RX_MIN = -9940f, RX_MAX = -5760f;   // X left → right
    const float RY_MAX = -5550f, RY_MIN = -10070f;  // Y top  → bottom

    /* one icon per registry index */
    readonly Dictionary<int, GameObject> pinInstances = new();

    void OnEnable() => StartCoroutine(UpdateLoop());
    void OnDisable() => StopAllCoroutines();

    IEnumerator UpdateLoop()
    {
        var wait = new WaitForSeconds(refreshInterval);

        while (true)
        {
            SyncPinsWithRegistry();
            yield return wait;
        }
    }

    /* ------------------------------------------------------------------ */
    void SyncPinsWithRegistry()
    {
        int count = PinRegistry.Pins.Count;                          // how many real pins? :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}

        /* ----- 1. add new ones ----- */
        for (int i = 0; i < count; i++)
        {
            if (pinInstances.ContainsKey(i) ) continue;               // already have icon

            // compute local pos once – pins never move after spawn
            Vector2 realXY = new(PinRegistry.Pins[i].mapX,
                                 PinRegistry.Pins[i].mapY);
            Vector3 localPos = RealToLocal(realXY);

            if (PinRegistry.Pins[i].type == "pin")
            {
                GameObject pinGO = Instantiate(pinPrefab, localPos, Quaternion.Euler(0, 90, 0));
                pinInstances.Add(i, pinGO);
            }
            
        }

        /* ----- 2. remove stale ones (if pins got deleted) ----- */
        if (pinInstances.Count > count)
        {
            List<int> toRemove = new();
            foreach (var kvp in pinInstances)
                if (kvp.Key >= count)
                    toRemove.Add(kvp.Key);

            foreach (int key in toRemove)
            {
                Destroy(pinInstances[key]);
                pinInstances.Remove(key);
            }
        }
    }

    /* convert metres in the real frame → localPosition under mapRect */
    Vector3 RealToLocal(Vector2 realXY)
    {
        float nx = Mathf.InverseLerp(RX_MIN, RX_MAX, realXY.x); // 0-1 across X
        float ny = Mathf.InverseLerp(RY_MAX, RY_MIN, realXY.y); // flip Y axis

        float px = nx * mapRect.rect.width;
        float py = ny * mapRect.rect.height;

        return new Vector3(px, py, -0.0003385376f);  // Z=0 keeps icon flush with map
    }
}
