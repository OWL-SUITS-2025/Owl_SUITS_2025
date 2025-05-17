using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPinOverlay : MonoBehaviour
{
    [Header("Map settings")]
    [SerializeField] RectTransform mapRect;
    [SerializeField] float xMin=-9940f, xMax=-5760f, yMin=-10070f, yMax=-5550f;

    [Header("Pin")]
    [SerializeField] GameObject pinPrefab;
    [SerializeField] float refreshInterval = 1f;

    Dictionary<int, GameObject> pinInstances = new Dictionary<int, GameObject>();

    void OnEnable() => StartCoroutine(UpdateLoop());
    void OnDisable() => StopAllCoroutines();

    IEnumerator UpdateLoop()
    {
        while (true)
        {
            UpdatePins();
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    void UpdatePins()
    {
        for (int i = 0; i < PinRegistry.Pins.Count; i++)
        {
            // spawn if needed
            if (!pinInstances.TryGetValue(i, out GameObject pin))
            {
                pin = Instantiate(pinPrefab, mapRect);
                pinInstances.Add(i, pin);
            }

            // get real coords
            var data = PinRegistry.Pins[i];
            Vector2 real = new Vector2(data.mapX, data.mapY);

            // compute normalized
            float u = Mathf.InverseLerp(xMin, xMax, real.x);
            float v = Mathf.InverseLerp(yMin, yMax, real.y);

            // map to UI pixels (pivot = bottom-left)
            float px = u * mapRect.rect.width;
            float py = v * mapRect.rect.height;

            // if pivot is center, uncomment next two lines:
            // px -= mapRect.rect.width  * 0.5f;
            // py -= mapRect.rect.height * 0.5f;

            pin.GetComponent<RectTransform>().anchoredPosition = new Vector2(px, py);
        }
    }
}
