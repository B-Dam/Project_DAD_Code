using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] CanvasGroup targetPanel;

    public void OnPointerEnter(PointerEventData e)
    {
        // DrawDiscardView가 켜져있으면 리턴
        if (DeckUIManager.Instance.isViewActive) return;

        targetPanel.DOFade(1f, 0.2f);
        targetPanel.interactable = targetPanel.blocksRaycasts = true;
    }

    public void OnPointerExit(PointerEventData e)
    {
        targetPanel.DOFade(0f, 0.2f);
        targetPanel.interactable = targetPanel.blocksRaycasts = false;
    }
}