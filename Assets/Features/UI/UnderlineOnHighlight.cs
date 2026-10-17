
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UnderlineOnHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text buttonText;

    private void OnDisable()
    {
        buttonText.fontStyle &= ~FontStyles.Underline;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.fontStyle |= FontStyles.Underline;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.fontStyle &= ~FontStyles.Underline;
    }
}
