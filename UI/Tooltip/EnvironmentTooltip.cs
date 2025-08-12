using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class EnvironmentTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("툴팁 패널")]
    [SerializeField] private GameObject tooltipPanel; // TooltipPanel
    [SerializeField] private CanvasGroup tooltipCanvas; 
    [SerializeField] private TextMeshProUGUI descText; // DescriptionText

    // 툴팁에 들어갈 설명 텍스트 세팅
    public void SetDescription(string description)
    {
        descText.text = description;
    }

    // 마우스 올리면 툴팁 보이기
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 패널 활성화
        tooltipPanel.SetActive(true);

        // 페이드 인
        tooltipCanvas
            .DOFade(1f, 0.2f)
            .SetEase(Ease.OutSine);
    }

    // 마우스 벗어나면 툴팁 숨기기
    public void OnPointerExit(PointerEventData eventData)
    {
        // 페이드 아웃, 완료 시 패널 비활성화
        tooltipCanvas
            .DOFade(0f, 0.2f)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                tooltipPanel.SetActive(false);
            });
    }

    private void Awake()
    {
        // 시작 시 무조건 숨김
        tooltipPanel.SetActive(false);
        tooltipCanvas.alpha = 0f;
    }
}