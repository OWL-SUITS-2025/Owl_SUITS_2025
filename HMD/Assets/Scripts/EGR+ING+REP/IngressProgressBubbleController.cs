using UnityEngine;
using UnityEngine.UI;

public class ProgressBubbleController : MonoBehaviour
{
    [Tooltip("All 17 bubble GameObjects components in order: ProgressBubble1.0, 1.1, … 4.3")]
    public GameObject[] bubbles = new GameObject[17];


    void Start()
    {
        // Deactivate all bubbles at the start
        DeactivateAllBubbles();

        // Set the first bubble to active
        if (bubbles.Length > 0 && bubbles[0] != null)
            bubbles[0].SetActive(true);
    }


    /// <summary>
    /// Enable or disable a single bubble by its index (0–16).
    /// </summary>
    public void SetBubbleState(int index, bool active)
    {
        if (index < 0 || index >= bubbles.Length)
        {
            Debug.LogWarning($"[ProgressBubble] Index out of range: {index}");
            return;
        }
        bubbles[index].gameObject.SetActive(active);
    }

    /// <summary>
    /// Turns all bubbles off.
    /// </summary>
    public void DeactivateAllBubbles()
    {
        for (int i = 0; i < bubbles.Length; i++)
        {
            if (bubbles[i] != null)
                bubbles[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles a bubble on/off.
    /// </summary>
    public void ToggleBubble(int index)
    {
        if (index < 0 || index >= bubbles.Length)
        {
            Debug.LogWarning($"[ProgressBubble] Index out of range: {index}");
            return;
        }
        var img = bubbles[index];
        img.gameObject.SetActive(!img.gameObject.activeSelf);
    }
}
