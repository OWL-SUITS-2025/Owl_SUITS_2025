using UnityEngine;
using TMPro;                                  // delete if you use legacy <Text>
using System.Collections;

public class DisplayUpdater : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IMUDataHandler imuDataHandler;        // drag your IMUDataHandler
    [SerializeField] private EVANumberHandler evaNumberHandler;
    [SerializeField] private TMP_Text xyField;    // drag the <TextMeshPro> that shows X / Y
    [SerializeField] private TMP_Text heading;

    [Header("Refresh")]
    [SerializeField] private float refreshInterval = 1f; // seconds between updates

    private Coroutine loop;

    /* ---------- life-cycle ---------- */
    private void OnEnable() { loop = StartCoroutine(UpdateLoop()); }
    private void OnDisable() { if (loop != null) StopCoroutine(loop); }

    /* ---------- once-per-second updater ---------- */
    IEnumerator UpdateLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(refreshInterval);

        while (true)
        {

            int evaNumber = evaNumberHandler != null ? evaNumberHandler.getEVANumber() : 0;
            string evaKey = "eva" + evaNumber;

            // Only proceed if we have a valid EVA number
            if (evaNumber == 1 || evaNumber == 2)
            {
                float x = imuDataHandler.GetPosx(evaKey);          // or just imu.getX()
                float y = imuDataHandler.GetPosy(evaKey);          // same idea for Y
                float head = imuDataHandler.GetHeading(evaKey);

                // show with two digits after the decimal: 12.34
                xyField.text = $"X: {x:F2} m\nY: {y:F2} m";

               
               

                // each sector is 45°, boundaries at 22.5°, 67.5°, 112.5°, …
                if (head >= 337.5f || head < 22.5f)
                {
                    heading.text = "N";
                }
                else if (head < 67.5f)
                {
                    heading.text = "NE";
                }
                else if (head < 112.5f)
                {
                    heading.text = "E";
                }
                else if (head < 157.5f)
                {
                    heading.text = "SE";
                }
                else if (head < 202.5f)
                {
                    heading.text = "S";
                }
                else if (head < 247.5f)
                {
                    heading.text = "SW";
                }
                else if (head < 292.5f)
                {
                    heading.text = "W";
                }
                else  // head < 337.5f
                {
                    heading.text = "NW";
                }



            }
            else
            {
                xyField.text = "X: NOT FOUND \nY: NOT FOUND";
                heading.text = "NOT FOUND";
            }

            yield return wait;
        }
    }
}
