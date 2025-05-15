using UnityEngine;
using TMPro;                                  // delete if you use legacy <Text>
using System.Collections;

public class DisplayUpdater : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IMUDataHandler imuDataHandler;        // drag your IMUDataHandler
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
            float x = imuDataHandler.GetPosx("eva1");          // or just imu.getX()
            float y = imuDataHandler.GetPosy("eva1");          // same idea for Y
            float head = imuDataHandler.GetHeading("eva1");

            // show with two digits after the decimal: 12.34
            xyField.text = $"X: {x:F2} m\nY: {y:F2} m";
            heading.text = $"{head:F2}";

            yield return wait;
        }
    }
}
