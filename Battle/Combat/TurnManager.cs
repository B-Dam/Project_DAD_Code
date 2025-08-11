using UnityEngine;
using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public enum Phase { Player, EnemyPreview, Enemy }

    public Phase currentPhase { get; private set; }
    
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Animator enemyAnimator;

    // 각 페이즈 진입 시점에 구독할 이벤트
    public event Action OnPlayerTurnStart;
    public event Action OnPlayerTurnEnd;
    public event Action OnEnemySkillPreview;
    public event Action OnEnemyTurnStart;
    public event Action OnEnemyTurnEnd;
    
    [Tooltip("공격 시작 전 딜레이 시간")]
    public float delayDuration = 1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 플레이어 턴 시작
    /// </summary>
    public void StartPlayerTurn()
    {
        currentPhase = Phase.Player;
        
        // 턴 종료 버튼 텍스트 변경
        ChangeTurnEndButtonText();
        
        // 실드 초기화
        CombatManager.Instance.ResetPlayerShield();
        
        OnPlayerTurnStart?.Invoke();
        
        // 턴 종료 버튼 활성화
        endTurnButton.enabled = true;
        
        // 필살기 버튼 활성화
        if(CombatManager.Instance.IsSpecialReady)
            SpecialGaugeUI.Instance.specialButton.interactable = true;
    }
    
    /// <summary>
    /// 플레이어가 “턴 종료” 버튼을 눌렀을 때
    /// </summary>
    public void EndPlayerTurn()
    {
        currentPhase = Phase.EnemyPreview;
        OnPlayerTurnEnd?.Invoke();

        if (CombatManager.Instance.enemyStunTurns <= 0)
        {
            OnEnemySkillPreview?.Invoke();
        }

        // 턴 종료 버튼 비활성화
        endTurnButton.enabled = false;
        
        // 필살기 버튼 비활성화
        SpecialGaugeUI.Instance.specialButton.interactable = false;
        
        // 턴 종료 버튼 텍스트 변경
        ChangeTurnEndButtonText();
        
        StartCoroutine(DoEnemyTurnAfterDelay());
    }
    
    IEnumerator DoEnemyTurnAfterDelay()
    {
        yield return new WaitForSeconds(delayDuration);
        StartEnemyTurn();
    }

    /// <summary>
    /// 적 턴 실제 시작 (프리뷰 후 수초 뒤에 호출)
    /// </summary>
    public void StartEnemyTurn()
    {
        // 기절 상태면 바로 턴 종료
        if (CombatManager.Instance.enemyStunTurns > 0)
        {
            // (차후에 여기서 “기절!” 텍스트, 아이콘 연출)
            OnEnemyTurnStart?.Invoke();
            // 한 프레임 대기 없이 바로 끝내기
            OnEnemyTurnEnd?.Invoke();
            // 플레이어 턴으로
            StartPlayerTurn();
            return;
        }
        
        currentPhase = Phase.Enemy;
        
        // 실드 초기화
        CombatManager.Instance.ResetEnemyShield();
        
        OnEnemyTurnStart?.Invoke();
        StartCoroutine(EnemyAnimationDelay());
    }

    IEnumerator EnemyAnimationDelay()
    {
        // 한 프레임 대기해서 트리거가 제대로 들어간 애니메이터 상태로 업데이트되도록 함
        yield return null;

        // 현재 플레이 중인 애니메이션 클립 길이 계산
        var state = enemyAnimator.GetCurrentAnimatorStateInfo(0);
        float duration = state.length / state.speed;

        // 실제 애니메이션 길이만큼 대기
        yield return new WaitForSeconds(duration);
        
        // 적 턴이 완전히 끝나갈 때 이벤트 호출
        OnEnemyTurnEnd?.Invoke();

        // 플레이어 턴 시작
        StartPlayerTurn();
    }

    // 턴 종료 버튼 텍스트
    public void ChangeTurnEndButtonText()
    {
        var tmp = endTurnButton.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = (currentPhase == Phase.Player) ? "턴 종료" : "상대 턴";
    }
}