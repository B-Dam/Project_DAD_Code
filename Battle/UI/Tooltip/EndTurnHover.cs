using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EndTurnHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Outline outline;
    private Tween t;

    void Awake()
    {
        outline = outline ?? GetComponent<Outline>();
        var c = outline.effectColor; c.a = 0f; outline.effectColor = c;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        // DrawDiscardView가 켜져있으면 리턴
        if (DeckUIManager.Instance.isViewActive) return;
        
        t?.Kill();
        t = DOTween.To(
            () => outline.effectColor.a,
            a => { var col = outline.effectColor; col.a = a; outline.effectColor = col; },
            0.6f, 0.3f
        );
    }
    public void OnPointerExit(PointerEventData e)
    {
        t?.Kill();
        t = DOTween.To(
            () => outline.effectColor.a,
            a => { var col = outline.effectColor; col.a = a; outline.effectColor = col; },
            0f, 0.2f
        );
    }
}