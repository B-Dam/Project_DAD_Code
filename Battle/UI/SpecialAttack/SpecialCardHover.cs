using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SpecialCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("호버 설정")]
    [SerializeField] private float hoverScale   = 1.2f;
    [SerializeField] private float hoverDuration= 0.2f;
    private Vector3            originalScale;
    private Outline            outline;
    private Button             button;

    void Awake()
    {
        button        = GetComponent<Button>();
        originalScale = transform.localScale;
        outline       = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        // 확대
        transform
            .DOScale(originalScale * hoverScale, hoverDuration)
            .SetEase(Ease.OutSine);
        // 아웃라인
        if (outline != null)
        {
            outline.effectColor = Color.white;
            outline.enabled     = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;

        // 원래 크기 복귀
        transform
            .DOScale(originalScale, hoverDuration)
            .SetEase(Ease.OutSine);
        if (outline != null) outline.enabled = false;
    }
}