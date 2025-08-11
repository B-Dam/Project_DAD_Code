using DG.Tweening;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance { get; private set; }
    
    [Header("툴팁 패널")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    
    [Header("페이드 시간")]
    [SerializeField] private float fadeDuration = 0.2f;
    
    // panelRoot에 붙어 있을 CanvasGroup
    private CanvasGroup cg;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // panelRoot에서 CanvasGroup을 가져오거나 없으면 추가
        cg = panelRoot.GetComponent<CanvasGroup>();
        if (cg == null) cg = panelRoot.AddComponent<CanvasGroup>();

        // panelRoot는 항상 Active 상태로 두고, 
        // 투명하게, 인터랙트/레이캐스트 차단 상태로 초기화
        panelRoot.SetActive(true);
        cg.alpha           = 0f;
        cg.interactable    = false;
        cg.blocksRaycasts  = false;
    }

    /// <summary>
    /// PreviewUI.currentSkill로부터 정보를 가져와 화면에 고정 표시
    /// </summary>
    public void ShowCurrentSkill()
    {
        var skill = PreviewUI.Instance.CurrentSkill;
        if (skill == null) return;

        nameText.text = skill.displayName;

        // 포맷팅
        string formatted = TextFormatter.Format(
            skill.effectText,
            new System.Collections.Generic.Dictionary<string,string> {
                { "damage", (CombatManager.Instance.EnemyBaseAtk
                             + skill.effectAttackValue
                             + CombatManager.Instance.enemyAtkMod).ToString() },
                { "turns",  skill.effectTurnValue.ToString() },
                { "shield", skill.effectShieldValue.ToString() },
                { "debuff", skill.effectAttackDebuffValue.ToString() },
                { "buff",   skill.effectAttackIncreaseValue.ToString() }
            }
        );
        descText.text = formatted;

        // 기존 트윈 중단
        cg.DOKill();
        // 인터랙트/레이캐스트 허용
        cg.interactable   = true;
        cg.blocksRaycasts = true;
        // 알파 페이드인
        cg.DOFade(1f, fadeDuration).SetEase(Ease.OutSine);
    }

    public void Hide()
    {
        cg.DOKill();
        cg.DOFade(0f, fadeDuration)
          .SetEase(Ease.OutSine)
          .OnComplete(() =>
          {
              cg.interactable   = false;
              cg.blocksRaycasts = false;
          });
    }
}