using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatSceneController : MonoBehaviour
{
    [Header("배경")]
    [SerializeField] private Image backgroundImage;

    [Header("적")]
    [SerializeField] private GameObject enemyObject; // 적 오브젝트 (애니메이터 포함)
    [SerializeField] private Animator enemyAnimator;

    [Header("GameUI 그룹")]
    [SerializeField] private CanvasGroup gameUIGroup; // 첫 시작시 제어할 게임 UI
    [SerializeField] private CanvasGroup battleStartGroup;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float startAnimDuration = 0.6f;
    [SerializeField] private float uiFadeDuration   = 0.3f;
    [SerializeField] private float battleDisplayTime = 1.2f;
    [SerializeField] private float staggerInterval  = 0.1f;
    
    [Header("환경 데이터 풀")]
    [SerializeField] private List<EnvironmentEffect> allEnvironments; // 인스펙터에 모든 SO 할당
    
    [Header("전장 환경 패널")] 
    [SerializeField] private CanvasGroup envGroup; // 환경 안내 패널 CanvasGroup
    [SerializeField] private TextMeshProUGUI envNameText; // 환경 이름
    [SerializeField] private TextMeshProUGUI envDescText; // 설명
    [SerializeField] private Image envIcon; // 아이콘 이미지
    [SerializeField] private float envEnterDuration = 0.3f;
    [SerializeField] private float envShowDuration = 1.0f;
    [SerializeField] private float envExitDuration = 0.2f;
    
    [Header("화면 왼쪽 환경 아이콘")]
    [SerializeField] private Image envIconTopLeft;
    [SerializeField] private EnvironmentTooltip envTooltip;
    
    [Header("디버그: 튜토리얼 강제 활성화")]
    [SerializeField] private bool forceTutorialMode = false;
    [SerializeField] private CardData[] forceTutorialDeckOrder;
    
    private EnvironmentEffect currentEnvironment;
    
    private CombatSetupData _setupData;
    
    private void Start()
    {
        // 튜토리얼 강제 모드일 때
        if (forceTutorialMode && forceTutorialDeckOrder != null && forceTutorialDeckOrder.Length > 0)
        {
            HandManager.Instance.SetupTutorialDeck(forceTutorialDeckOrder);
            HandManager.Instance.IsTutorialMode = true;
        }
        
        // 세팅 데이터 가져오기
        CombatSetupData data = CombatDataHolder.GetData();

        if (data != null)
        {
            _setupData = data;
            SetupCombat(data);

            // 튜토리얼 모드면 여기서도 한 번 보강 주입
            if (CombatDataHolder.LastTrigger != null 
                && CombatDataHolder.LastTrigger.isTutorialTrigger 
                && data.tutorialDeckOrder != null)
            {
                HandManager.Instance.SetupTutorialDeck(data.tutorialDeckOrder);
            }
        }
        else
        {
            // 전투 세팅 데이터가 없으면 : 배틀 씬에서 테스트 하거나 etc
            Debug.LogWarning("전투 세팅 데이터가 없습니다. 모든 환경 중 하나를 랜덤으로 지정합니다.");
            if (allEnvironments != null && allEnvironments.Count > 0)
                currentEnvironment = allEnvironments[Random.Range(0, allEnvironments.Count)];
            else
                Debug.LogError("allEnvironments에 할당된 SO가 없습니다!");
            
            // 환경 아이콘 및 툴팁 텍스트 설정
            RefreshEnvironmentUI();
        }

        // 애니메이터 연결
        var animCtrl = enemyObject.GetComponentInChildren<CombatAnimationController>();
        if (animCtrl != null)
            animCtrl.enemyAnimator = this.enemyAnimator;
        
        // 전투 데이터 로드 후에 호출
        PlayBattleStartSequence();
    }

    // 받아오는 데이터를 기반으로 보스 및 배경 설정
    private void SetupCombat(CombatSetupData data)
    {
        // 배경 설정
        if (backgroundImage != null && data.backgroundSprite != null)
            backgroundImage.sprite = data.backgroundSprite;

        // 적 애니메이터 설정
        if (enemyAnimator != null && data.animatorController != null)
            enemyAnimator.runtimeAnimatorController = data.animatorController;

        // 적 데이터 설정
        if (data.enemyCharacterSO != null)
            DataManager.Instance.SetEnemy(data.enemyCharacterSO);
        
        // 환경 랜덤 선택
        var candidates = allEnvironments
                         .Where(env => env.applicableBossIds.Contains(data.enemyName))
                         .ToList();

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"[{data.enemyName}]에 매핑된 환경 SO가 없습니다. 전체 풀에서 선택합니다.");
            candidates = allEnvironments;
        }

        currentEnvironment = candidates[Random.Range(0, candidates.Count)];

        // 화면 왼쪽 아이콘 & 툴팁 세팅
        RefreshEnvironmentUI();
    }
    
    private void RefreshEnvironmentUI()
    {
        if (currentEnvironment == null) return;
        // 왼쪽 위 아이콘
        envIconTopLeft.sprite = currentEnvironment.icon;
        // 툴팁 설명
        envTooltip.SetDescription(currentEnvironment.description);
    }
    
    // 전투 시작시 자연스러운 UI 세팅
    public void PlayBattleStartSequence()
    {
        // BattleStartPanel 활성화 및 초기 상태
        battleStartGroup.gameObject.SetActive(true);
        battleStartGroup.alpha = 0f;
        battleStartGroup.transform.localScale = Vector3.zero;

        // GameUI 초기 숨김
        gameUIGroup.alpha = 0f;
        gameUIGroup.interactable = gameUIGroup.blocksRaycasts = false;

        // DOTween 시퀀스 구성
        var seq = DOTween.Sequence();
        
        // BattleStartPanel 나타내기 (스케일 + 페이드)
        seq.Append(battleStartGroup.DOFade(1f, startAnimDuration));
        seq.Join(battleStartGroup.transform
                                 .DOScale(1f, startAnimDuration)
                                 .SetEase(Ease.OutBack));
        
        // 전투 시작 직전에 환경을 먼저 설정
        seq.AppendCallback(() =>
        {
            CombatManager.Instance.SetEnvironmentEffect(currentEnvironment);

            if (currentEnvironment.effectId == "wind")
            {
                AudioManager.Instance.PlayBGM("WindyBattleBGM");
            }
            else if (currentEnvironment.effectId == "rain")
            {
                AudioManager.Instance.PlayBGM("RainningBattleBGM");
            }
            else if (currentEnvironment.effectId == "fog")
            {
                AudioManager.Instance.PlayBGM("BattleMapBGM");
            }
        });
        seq.AppendCallback(() =>
        {
            CombatManager.Instance.StartCombat();
        });
        
        // 잠시 대기
        seq.AppendInterval(battleDisplayTime);

        // BattleStartPanel 사라지기
        seq.Append(battleStartGroup.DOFade(0f, uiFadeDuration));
        seq.Join(battleStartGroup.transform
                                 .DOScale(0.8f, uiFadeDuration));

        // 완전 비활성화
        seq.AppendCallback(() =>
        {
            battleStartGroup.gameObject.SetActive(false);
        });
        
        // 환경 안내 패널 세팅 & 인
        seq.AppendCallback(() =>
        {
            envNameText.text = currentEnvironment.title;
            envDescText.text = currentEnvironment.description;
            envIcon.sprite = currentEnvironment.icon;
            envGroup.alpha = 0f;
            envGroup.gameObject.SetActive(true);
        });
        seq.Append(envGroup.DOFade(1f, envEnterDuration)
                           .SetEase(Ease.OutBack));

        // 유지
        seq.AppendInterval(envShowDuration);

        // 환경 안내 패널 아웃
        seq.Append(envGroup.DOFade(0f, envExitDuration)
                           .SetEase(Ease.InQuad));
        seq.AppendCallback(() =>
        {
            envGroup.gameObject.SetActive(false);
        });

        // GameUI 각 요소를 스태거(stagger)로 등장시키기
        seq.AppendCallback(ShowGameUIWithStagger);

        // 인터렉션 활성화
        seq.AppendCallback(() =>
        {
            gameUIGroup.interactable = gameUIGroup.blocksRaycasts = true;
        });
        
        seq.AppendCallback(() => {
            if (HandManager.Instance.IsTutorialMode)
                TutorialManager.Instance.ShowStep(0);
        });
        
        seq.Play();
    }
    
    private void ShowGameUIWithStagger()
    {
        // GameUI 하위에 CanvasGroup이 붙은 요소들 가져오기
        var uiElements = gameUIGroup
                         .GetComponentsInChildren<CanvasGroup>(true)
                         .Where(cg => !cg.gameObject.CompareTag("Tooltip"));; // Tooltip 태그만 없는 친구들만 가져오기
        float delay = 0f;

        foreach (var cg in uiElements)
        {
            // 각 요소를 순차적으로 페이드 인
            cg.alpha = 0f;
            cg.DOFade(1f, uiFadeDuration)
              .SetDelay(delay)
              .SetEase(Ease.Linear);
            delay += staggerInterval;
        }
    }
}