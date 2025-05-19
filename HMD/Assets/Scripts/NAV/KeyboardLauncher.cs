using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class KeyboardTrigger : MonoBehaviour, IPointerClickHandler
{
    public TMP_InputField inputField;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inputField != null)
        {
            inputField.ActivateInputField();

            // Open the on-screen keyboard
            TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false, "");
        }
    }
}
