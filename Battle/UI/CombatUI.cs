using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    [Header("플레이어 스탯")] [SerializeField] TMP_Text playerHPText;
    [SerializeField] TMP_Text playerShieldText;
    [SerializeField] TMP_Text playerAPText;
    [SerializeField] HealthBar playerHealthBar;
    [SerializeField] GameObject playerBarrier;

    [Header("적 스탯")] [SerializeField] TMP_Text enemyHPText;
    [SerializeField] TMP_Text enemyShieldText;
    [SerializeField] HealthBar enemyHealthBar;
    [SerializeField] GameObject enemyBarrier;
    
    [Header("텍스트 애니메이션용")]
    [SerializeField] private float popScale    = 2.5f;
    [SerializeField] private float popDuration = 0.1f;

    [Header("버프/디버프 표시용")] 
    [SerializeField] private Transform playerStatusBar;
    [SerializeField] private Transform enemyStatusBar;
    [SerializeField] private GameObject statusIconPrefab; // 단일 아이콘 Prefab
    [SerializeField] private Sprite statusIconSprite; // 아이콘
    [SerializeField] private Color positiveTextColor; // 양수일 때 텍스트 색
    [SerializeField] private Color negativeTextColor; // 음수일 때 텍스트 색

    private Vector3 normalScale;
    
    public static CombatUI Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // AP 텍스트 기존 스케일 저장
        normalScale = playerAPText.rectTransform.localScale;
    }

    void Start()
    {
        // 전투 시작과 턴 전환 이벤트 구독
        CombatManager.Instance.OnCombatStart += OnCombatStart;
        CombatManager.Instance.OnStatsChanged += UpdateUI;
        TurnManager.Instance.OnPlayerTurnStart += OnPlayerTurnStart;
        TurnManager.Instance.OnEnemySkillPreview += OnEnemySkillPreview;
        TurnManager.Instance.OnEnemyTurnStart += OnEnemyTurnStart;

        // 씬 로드 직후(전투 시작 전일 수도 있지만) 한번 갱신
        StartCoroutine(DelayedUpdateUI());
    }

    void OnDestroy()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStart -= OnCombatStart;
            CombatManager.Instance.OnStatsChanged -= UpdateUI;
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart -= OnPlayerTurnStart;
            TurnManager.Instance.OnEnemySkillPreview -= OnEnemySkillPreview;
            TurnManager.Instance.OnEnemyTurnStart -= OnEnemyTurnStart;
        }
    }

    private void Update()
    {
        if (CombatManager.Instance.playerShield <= 0)
            playerBarrier.SetActive(false);
        else playerBarrier.SetActive(true);

        if (CombatManager.Instance.enemyShield <= 0)
            enemyBarrier.SetActive(false);
        else enemyBarrier.SetActive(true);
    }

    void OnCombatStart() => StartCoroutine(DelayedUpdateUI());
    void OnPlayerTurnStart() => StartCoroutine(DelayedUpdateUI());
    void OnEnemySkillPreview() => StartCoroutine(DelayedUpdateUI());
    void OnEnemyTurnStart() => StartCoroutine(DelayedUpdateUI());

    IEnumerator DelayedUpdateUI()
    {
        // HandManager/AP 세팅, CombatManager/playerHp 세팅이 끝난 뒤에 호출될 수 있도록
        yield return null;
        UpdateUI();
    }

    public void UpdateUI()
    {
        var cm = CombatManager.Instance;
        if (cm == null) return;

        // 플레이어
        playerHPText.text = $"{cm.playerHp}/{DataManager.Instance.playerData.maxHP}";
        playerShieldText.text = $"{cm.playerShield}";
        playerAPText.text = $"{HandManager.Instance.currentAP}/3";
        playerHealthBar.SetHealth(cm.playerHp, DataManager.Instance.playerData.maxHP);

        // 적
        enemyHPText.text = $"{cm.enemyHp}/{DataManager.Instance.enemyData.maxHP}";
        enemyShieldText.text = $"{cm.enemyShield}";
        enemyHealthBar.SetHealth(cm.enemyHp, DataManager.Instance.enemyData.maxHP);

        // 버프/디버프 아이콘 채우기
        PopulateStatusBar(playerStatusBar, cm.PlayerAttackModifiers);
        PopulateStatusBar(enemyStatusBar, cm.EnemyAttackModifiers);
    }

    void ClearStatusBar(Transform bar)
    {
        foreach (Transform t in bar)
            Destroy(t.gameObject);
    }

    void PopulateStatusBar(Transform bar, IEnumerable<TimedModifier> mods)
    {
        // 0인 모디파이어는 건너뛰기
        var nonZero = mods.Where(m => m.value != 0).ToList();

        // 리스트가 비어 있으면 컨테이너 비활성화
        bar.gameObject.SetActive(nonZero.Count > 0);
        if (nonZero.Count == 0) return;

        // 기존 아이콘 모두 삭제
        foreach (Transform t in bar) Destroy(t.gameObject);

        // 아이콘 + 숫자 생성
        foreach (var mod in nonZero)
        {
            var go = Instantiate(statusIconPrefab, bar);
            var img = go.GetComponent<Image>();
            var txt = go.GetComponentInChildren<TMP_Text>();
            if (txt == null)
            {
                Debug.LogError("[CombatUI] Instantiate된 아이콘에 TMP_Text가 없습니다!", go);
                continue;
            }

            // 동일한 아이콘 사용
            img.sprite = statusIconSprite;

            // 텍스트 값 할당
            txt.text = mod.value.ToString();

            // 값에 따라 텍스트 색 변경
            txt.color = mod.value > 0
                ? positiveTextColor
                : negativeTextColor;
        }
    }
    
    /// <summary>AP 값 갱신 + 팝 애니메이션</summary>
    public void UpdateAP(int newAP)
    {
        playerAPText.text = newAP.ToString();
        AnimateAPPop();
    }

    void AnimateAPPop()
    {
        var rt = playerAPText.rectTransform;
        rt.DOKill();                    // 기존 트윈 제거
        rt.localScale = normalScale;    // 스케일 리셋
        rt.DOScale(normalScale * popScale, popDuration)
          .SetEase(Ease.OutBack)
          .SetLoops(2, LoopType.Yoyo);
    }
}