using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SpecialAbilityUI : MonoBehaviour
{
    [SerializeField] private GameObject specialPanel; // 카드 3장 선택 창
    private CanvasGroup panelCg;                             // CanvasGroup
    [SerializeField] private float fadeDuration = 0.3f;      // 페이드 시간
    
    public static SpecialAbilityUI Instance { get; private set; }
    
    void Awake()
    {
        Instance = this;
        
        // CanvasGroup 가져오기
        panelCg = specialPanel.GetComponent<CanvasGroup>();
        if (panelCg == null)
            panelCg = specialPanel.AddComponent<CanvasGroup>();

        // 초기 상태: 투명 + 비활성화된 클릭
        panelCg.alpha = 0f;
        panelCg.blocksRaycasts = false;
        specialPanel.SetActive(false);
    }
    
    void Update()
    {
        // ESC 키로 언제든 패널 닫기
        if (specialPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            HideSpecialPanel();
    }

    public void ShowSpecialPanel()
    {
        // 활성화
        specialPanel.SetActive(true);
        
        // CanvasGroup 초기화
        panelCg.DOKill();
        panelCg.alpha = 0f;
        panelCg.blocksRaycasts = true;
        
        // 페이드 인
        panelCg.DOFade(1f, fadeDuration)
               .SetEase(Ease.OutSine);
    }

    // 비활성화 하지 않고 페이드 아웃만
    public void JustHideSpecialPanel()
    {
        panelCg.DOKill();
        panelCg.blocksRaycasts = false;
        panelCg.DOFade(0f, fadeDuration)
               .SetEase(Ease.OutSine);
    }

    public void HideSpecialPanel()
    {
        // 페이드아웃, 끝나면 비활성화
        panelCg.DOKill();
        panelCg.blocksRaycasts = false;
        panelCg.DOFade(0f, fadeDuration)
               .SetEase(Ease.OutSine)
               .OnComplete(() => specialPanel.SetActive(false));
    }
}