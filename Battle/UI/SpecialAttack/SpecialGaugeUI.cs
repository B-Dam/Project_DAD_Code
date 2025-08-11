using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SpecialGaugeUI : MonoBehaviour
{
    [SerializeField] private Image gaugeFillImage;
    [SerializeField] public Button specialButton;
    [SerializeField] private TextMeshProUGUI gaugeText;
    [SerializeField] private float fillTweenDuration = 0.3f;
    [SerializeField] private float buttonPopScale = 1.2f;
    [SerializeField] private float popDuration    = 0.2f;
    
    private int maxGauge;

    public static SpecialGaugeUI Instance { get; private set; }
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        var cm = CombatManager.Instance;
        maxGauge = cm.MaxSpecialGauge;

        // 초기 게이지 0%
        gaugeFillImage.fillAmount = 0f;
        specialButton.interactable = false;

        // 이벤트 구독
        cm.OnSpecialGaugeChanged += UpdateGauge;
        cm.OnSpecialReady        += OnReady;
        
        // 버튼 클릭 시 _게이지 초기화 하지 않고_ 패널 열기만
        specialButton.onClick.AddListener(() => 
            SpecialAbilityUI.Instance.ShowSpecialPanel()
        );
    }

    void OnDestroy()
    {
        CombatManager.Instance.OnSpecialGaugeChanged -= UpdateGauge;
        CombatManager.Instance.OnSpecialReady        -= OnReady;
    }

    void UpdateGauge(int current, int max)
    {
        var cm = CombatManager.Instance;
        
        float target = (float)current / maxGauge;
        // 부드럽게 채우기
        gaugeFillImage.DOKill();
        gaugeFillImage.DOFillAmount(target, fillTweenDuration)
                      .SetEase(Ease.OutQuad);
        
        if (!cm.IsSpecialReady)
            specialButton.interactable = false;
        
        gaugeText.text = $"{current}/{maxGauge}";
    }

    // 완충 시 버튼 활성화 + 팝 애니메이션
    void OnReady()
    {
        specialButton.interactable = true;

        var rt = specialButton.transform as RectTransform;
        rt.DOKill();
        rt.DOScale(buttonPopScale, popDuration)
          .SetLoops(2, LoopType.Yoyo)
          .SetEase(Ease.OutBack);
    }
    
    // 필살기 버튼 비활성화
    public void DisableSpecialButton()
    {
        specialButton.interactable = false;
    }
}