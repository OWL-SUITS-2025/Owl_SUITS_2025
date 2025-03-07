using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPrefab; // Reference to the popup prefab
    private GameObject popupInstance; // Instance of the popup

    public void ShowPopup()
    {
        Debug.Log("Popup triggered!");
        if (popupInstance == null)
        {
            popupInstance = Instantiate(popupPrefab, transform.position, Quaternion.identity);
        }
    }

    public void HidePopup()
    {
        if (popupInstance != null)
        {
            Destroy(popupInstance);
            popupInstance = null;
        }
    }

}
