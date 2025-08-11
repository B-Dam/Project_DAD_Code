using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckIconHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] RectTransform iconRect;
    [SerializeField] CanvasGroup descriptionPanel;

    public void OnPointerEnter(PointerEventData e)
    {
        // DrawDiscardView가 켜져있으면 리턴
        if (DeckUIManager.Instance.isViewActive) return;

        iconRect.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
        descriptionPanel.DOFade(1f, 0.2f).SetEase(Ease.OutSine);
        descriptionPanel.interactable = descriptionPanel.blocksRaycasts = true;
    }
    public void OnPointerExit(PointerEventData e)
    {
        iconRect.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        descriptionPanel.DOFade(0f, 0.2f).SetEase(Ease.OutSine);
        descriptionPanel.interactable = descriptionPanel.blocksRaycasts = false;
    }
}