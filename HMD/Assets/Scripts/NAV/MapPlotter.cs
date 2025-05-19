using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPinOverlay : MonoBehaviour
{
    [Header("Map settings")]
    [SerializeField] RectTransform mapRect;
    [SerializeField] float xMin=-9940f, xMax=-5760f, yMin=-10070f, yMax=-5550f;
    [SerializeField] private IMUDataHandler imuDataHandler;        // drag your IMUDataHandler
    [SerializeField] private EVANumberHandler evaNumberHandler;
    [Header("Pin")]
    [SerializeField] GameObject generalPrefab;
    [SerializeField] GameObject samplePrefab;
    [SerializeField] GameObject hazardPrefab;
    [SerializeField] GameObject selfPrefab;
    [SerializeField] float refreshInterval = 1f;

    Dictionary<int, GameObject> pinInstances = new Dictionary<int, GameObject>();

    void OnEnable() => StartCoroutine(UpdateLoop());
    void OnDisable() => StopAllCoroutines();

    public void OnMapButtonPressed()
    {
        
    }

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
                if (PinRegistry.Pins[i].type.ToLower() == "general")
                {
                    pin = Instantiate(generalPrefab, mapRect, false);
                }
                else if (PinRegistry.Pins[i].type.ToLower() == "sample")
                {
                    pin = Instantiate(samplePrefab, mapRect, false);
                }
                else if (PinRegistry.Pins[i].type.ToLower() == "hazard")
                {
                    pin = Instantiate(hazardPrefab, mapRect, false);
                }
               
                pinInstances.Add(i, pin);
            }

      
            // get real coords
            var data = PinRegistry.Pins[i];
            Vector2 real = new Vector2(data.x, data.y);

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

        person();
        





    }

    private void person()
    {
        int evaNumber = evaNumberHandler != null ? evaNumberHandler.getEVANumber() : 0;
        string evaKey = "eva" + evaNumber;

        // Only proceed if we have a valid EVA number
        if (evaNumber == 1 || evaNumber == 2)
        {
            float x = imuDataHandler.GetPosx(evaKey);          // or just imu.getX()
            float y = imuDataHandler.GetPosy(evaKey);          // same idea for Y
            float head = imuDataHandler.GetHeading(evaKey);

            Vector2 real = new Vector2(x, y);

            // compute normalized
            float u = Mathf.InverseLerp(xMin, xMax, real.x);
            float v = Mathf.InverseLerp(yMin, yMax, real.y);

            // map to UI pixels (pivot = bottom-left)
            float px = u * mapRect.rect.width;
            float py = v * mapRect.rect.height;

            GameObject pin = Instantiate(selfPrefab, mapRect, false);
            pin.GetComponent<RectTransform>().anchoredPosition = new Vector2(px, py);


        }
       
       

    }
}
