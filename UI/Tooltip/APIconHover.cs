using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class APIconHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] ParticleSystem hoverParticle;
    [SerializeField] CanvasGroup apTooltip; // APDescriptionPanel

    public void OnPointerEnter(PointerEventData e)
    {
        // DrawDiscardView가 켜져있으면 리턴
        if (DeckUIManager.Instance.isViewActive) return;
        
        if (hoverParticle != null) hoverParticle.Play();
        apTooltip.DOFade(1f, 0.2f).SetEase(Ease.OutSine);
        apTooltip.interactable = apTooltip.blocksRaycasts = true;
    } 
    
    public void OnPointerExit(PointerEventData e)
    {
        if (hoverParticle != null) hoverParticle.Stop();
        apTooltip.DOFade(0f, 0.2f).SetEase(Ease.OutSine);
        apTooltip.interactable = apTooltip.blocksRaycasts = false;
    }
}