using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum ModifierType
{
    AttackBuff,
    AttackDebuff
}

public class TimedModifier
{
    public ModifierType type;
    public int           value;
    public int           remainingTurns;
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("현재 HP")]
    public int playerHp { get; private set; }
    public int enemyHp  { get; private set; }
    
    [Header("현재 보호막")]
    public int playerShield { get; private set; }
    public int enemyShield  { get; private set; }

    [Header("버프, 디버프")]
    public int playerAtkMod { get; private set; }
    public int enemyAtkMod  { get; private set; }
    
    [Header("필살기 게이지")]
    public int CurrentSpecialGauge { get; private set; }
    public int MaxSpecialGauge { get; private set; } = 10;
    public bool IsSpecialReady => CurrentSpecialGauge >= MaxSpecialGauge;
    
    // 반사 버프
    private float playerReflectPercent = 0f;
    private int   reflectTurnsRemaining = 0;
    
    // 기절 디버프
    [HideInInspector] public int enemyStunTurns = 0;
    
    // 전장 환경 효과
    private EnvironmentEffect currentEnvironment;

    // 기본 행동력 설정
    [Header("행동력 설정")]
    [SerializeField] private int baseActionPoints = 3;
    private int actionPoints;
    
    // 전투 중인지 확인용
    public bool IsInCombat { get; set; }
    
    
    List<TimedModifier> playerAttackMods = new List<TimedModifier>();
    List<TimedModifier> enemyAttackMods  = new List<TimedModifier>();
    
    public event Action<CardData> OnPlayerSkillUsed;
    public event Action<CardData> OnEnemySkillUsed;
    public event Action OnPlayerHit;
    public event Action OnEnemyHit;
    public event Action OnPlayerDeath;
    public event Action OnEnemyDeath;
    public event Action<bool /*isPlayer*/, bool /*isBuff*/> OnStatusEffectApplied; // 버프/디버프 발생 이벤트
    public event Action<int /*specialIdx*/> OnSpecialUsed; // 필살기 발생 이벤트 (0:Attack,1:Shield,2:Stun)
    public event Action OnEnemyStuned;
    public event Action OnEnemyStunClean;
    
    // 필살기 게이지 변화 이벤트
    public event Action<int, int> OnSpecialGaugeChanged;
    public event Action OnSpecialReady;
    
    // 캐릭터 공격력, 방어력 가져오기
    public int PlayerBaseAtk => DataManager.Instance.playerData.atk;
    public int EnemyBaseAtk  => DataManager.Instance.enemyData.atk;
    
    public event Action OnCombatStart;
    public event Action OnStatsChanged;
    
    public IEnumerable<TimedModifier> PlayerAttackModifiers => playerAttackMods;
    public IEnumerable<TimedModifier> EnemyAttackModifiers  => enemyAttackMods;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
        
        // 필살기 게이지 초기화
        CurrentSpecialGauge = 0;
    }

    void Start()
    {
        if (TurnManager.Instance != null)
        {
            // 턴 종료에 모디파이어 감소
            TurnManager.Instance.OnEnemyTurnEnd   += OnEnemyTurnEnd;
            // 플레이어 쪽도 동일하게 분리
            TurnManager.Instance.OnPlayerTurnStart += OnPlayerTurnStart;       // (UI 갱신만)
            TurnManager.Instance.OnPlayerTurnEnd   += OnPlayerTurnEnd;         // (모디파이어 감소)
        }
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnEnemyTurnEnd   -= OnEnemyTurnEnd;
            TurnManager.Instance.OnPlayerTurnStart-= OnPlayerTurnStart;
            TurnManager.Instance.OnPlayerTurnEnd  -= OnPlayerTurnEnd;
        }
    }
    
    /// <summary>
    /// 전장 환경 SO를 받아와 내부에 저장
    /// </summary>
    public void SetEnvironmentEffect(EnvironmentEffect env)
    {
        currentEnvironment = env;
    }
    
    /// <summary>
    /// 환경 효과 발동 여부를 결정 (안개는 10% 확률, 나머지는 항상)
    /// </summary>
    private bool ShouldApplyEnvEffect()
    {
        // 환경이 없으면 false
        if (currentEnvironment == null) 
            return false;

        // effectId가 fog면 40%확률로 발동
        if (currentEnvironment.effectId == "fog")
            return UnityEngine.Random.value <= 0.4f;  // 40% 확률

        return true;  // wind, rain 등은 항상 적용
    }

    /// <summary>전투 개시</summary>
    public void StartCombat()
    {
        IsInCombat = true;
        Time.timeScale = 1;
            
        // HP 초기화
        playerHp = DataManager.Instance.playerData.maxHP;
        enemyHp  = DataManager.Instance.enemyData.maxHP;
        
        // 쉴드 초기화
        playerShield = 0;
        enemyShield  = 0;
        
        // 상태이상 값 초기화
        playerAttackMods.Clear();
        enemyAttackMods.Clear();
        
        // 모디파이어 갱신
        RecalculateModifiers();
        
        // 환경 행동력 보너스 확률 적용
        int bonusAP = (ShouldApplyEnvEffect() ? currentEnvironment.apBonus : 0);
        actionPoints = baseActionPoints + bonusAP;
        
        OnCombatStart?.Invoke();
        
        // 첫 턴 시작
        TurnManager.Instance.StartPlayerTurn();
    }
    
    /// <summary>모디파이어 리스트에서 턴 감소, 만료된 모디파이어 제거</summary>
    void UpdateModifiers(List<TimedModifier> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (--list[i].remainingTurns <= 0)
                list.RemoveAt(i);
        }
    }
    
    // 모디파이어 합산
    void RecalculateModifiers()
    {
        playerAtkMod = 0;
        foreach (var m in playerAttackMods) playerAtkMod += m.value;
        enemyAtkMod  = 0;
        foreach (var m in enemyAttackMods)  enemyAtkMod  += m.value;
    }

    void OnPlayerTurnStart()
    {
        // 1) 환경 보너스 포함해서 actionPoints 재설정
        int bonusAP = ShouldApplyEnvEffect() ? currentEnvironment.apBonus : 0;
        actionPoints = baseActionPoints + bonusAP;

        // 2) HandManager.currentAP 에 반영
        if (HandManager.Instance != null)
        {
            HandManager.Instance.currentAP = actionPoints;
        }

        // 3) CombatUI 에서 텍스트 갱신
        var ui = CombatUI.Instance;
        if (ui != null)
        {
            ui.UpdateUI();
        }
        
        // UI 갱신
        OnStatsChanged?.Invoke();
    }
    
    void OnPlayerTurnEnd()
    {
        UpdateModifiers(playerAttackMods);
        RecalculateModifiers();
        OnStatsChanged?.Invoke();
    }
    
    // 턴 종료 시 호출될 메서드: 모디파이어만 감소시키고 UI 갱신
    void OnEnemyTurnEnd()
    {
        // 반사 버프 턴 감소
        if (reflectTurnsRemaining > 0)
        {
            if (--reflectTurnsRemaining == 0)
                playerReflectPercent = 0f;
        }

        // 기절 턴 감소 (적 턴 넘어가기 전에)
        if (enemyStunTurns > 0)
        {
            enemyStunTurns--;
        if (enemyStunTurns == 0)
            OnEnemyStunClean?.Invoke();
        }

        UpdateModifiers(enemyAttackMods);
        RecalculateModifiers();
        OnStatsChanged?.Invoke();
    }
    
    public void ResetPlayerShield()
    {
        playerShield = 0;
        OnStatsChanged?.Invoke();
    }
    
    public void ResetEnemyShield()
    {
        enemyShield = 0;
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 카드 또는 스킬 적용
    /// </summary>
    /// <param name="data">카드/스킬 데이터</param>
    /// <param name="isPlayer">플레이어가 사용했으면 true, 적이면 false</param>
    public void ApplySkill(CardData data, bool isPlayer)
    {
        // 애니메이션 트리거용 이벤트 먼저 발행
        if (isPlayer) OnPlayerSkillUsed?.Invoke(data);
        else          OnEnemySkillUsed?.Invoke(data);
        
        // 환경 효과 적용 여부 결정 (안개는 10% 확률, 나머지는 항상)
        bool applyEnv = ShouldApplyEnvEffect();
        
        // 기본 공격력 + 공격 모디파이어
        int baseAtk   = isPlayer ? PlayerBaseAtk : EnemyBaseAtk;
        int modAtk    = isPlayer ? playerAtkMod : enemyAtkMod;
        int rawAttack = baseAtk + data.effectAttackValue + modAtk;
        
        // 방어력
        int def = isPlayer ? DataManager.Instance.playerData.def : DataManager.Instance.enemyData.def;
        rawAttack = Mathf.Max(0, rawAttack - def);
        
        // 환경 배율 곱하기
        float atkMult    = applyEnv ? currentEnvironment.attackMultiplier  : 1f;
        rawAttack        = Mathf.FloorToInt(rawAttack * atkMult);
        
        // 공격 계수가 0보다 클 때만 데미지 계산
        if (rawAttack > 0)
        {
            if (isPlayer)
            {
                int shielded = Mathf.Min(enemyShield, rawAttack);
                enemyShield -= shielded;
                
                // 적 데미지
                enemyHp     = Mathf.Max(0, enemyHp - (rawAttack - shielded));
                
                // 적 피격 이벤트
                OnEnemyHit?.Invoke();
                
                if (enemyHp <= 0)
                    OnEnemyDeath?.Invoke();
            }
            else
            {
                int shielded = Mathf.Min(playerShield, rawAttack);
                playerShield -= shielded;
                
                // 반사 : 실제 입힌(rawAttack)양의 50%를 돌려주기
                if (playerReflectPercent > 0f && rawAttack > 0)
                {
                    int reflectDamage = Mathf.RoundToInt(rawAttack * playerReflectPercent);
                    SpecialAttack(reflectDamage);
                }
                
                // 플레이어 데미지
                playerHp     = Mathf.Max(0, playerHp - (rawAttack - shielded));
                // 플레이어 피격 이벤트
                OnPlayerHit?.Invoke();
                
                if (playerHp <= 0)
                    OnPlayerDeath?.Invoke();
            }
        }

        // 보호막 효과
        if (data.effectShieldValue > 0)
        {
            // 기본 획득 보호막
            int shieldGain = data.effectShieldValue;
            
            // 환경 효과가 적용되면 multiplier 곱하기
            if (applyEnv && currentEnvironment != null)
                shieldGain = Mathf.FloorToInt(shieldGain * currentEnvironment.shieldMultiplier);
            
            if (isPlayer)
                playerShield += shieldGain;
            else
                enemyShield  += shieldGain;
        }

        // 공격력 버프
        if (data.effectAttackIncreaseValue != 0 && data.effectTurnValue > 0)
        {
            AddAttackModifier(
                isPlayer,
                ModifierType.AttackBuff,
                data.effectAttackIncreaseValue,
                data.effectTurnValue
            );
            OnStatusEffectApplied?.Invoke(isPlayer, true); // 강화 이펙트
        }

        // 공격력 디버프
        if (data.effectAttackDebuffValue != 0 && data.effectTurnValue > 0)
        {
            // 원래 디버프 값
            int rawDebuff = Mathf.Abs(data.effectAttackDebuffValue);
            
            // 환경이 발동되면 multiplier 곱하기
            int debuffValue = applyEnv
                ? Mathf.FloorToInt(rawDebuff * currentEnvironment.debuffMultiplier)
                : rawDebuff;
            
            AddAttackModifier(
                !isPlayer,
                ModifierType.AttackDebuff,
                -debuffValue,
                data.effectTurnValue
            );
            
            OnStatusEffectApplied?.Invoke(!isPlayer, false); // 약화 이펙트
        }

        RecalculateModifiers();
        OnStatsChanged?.Invoke();
        CheckEnd();
    }
    
    //  모디파이어 추가
    void AddAttackModifier(bool targetIsPlayer, ModifierType type, int value, int turns)
    {
        var list = targetIsPlayer ? playerAttackMods : enemyAttackMods;

        // 같은 효과 종류(modifierType) 검색
        var existing = list.FirstOrDefault(m => m.type == type);
        if (existing != null)
        {
            // 1) 값은 합산
            existing.value += value;
            // 2) 지속 턴은 더 긴 쪽
            existing.remainingTurns = Mathf.Max(existing.remainingTurns, turns);
        }
        else
        {
            list.Add(new TimedModifier {
                type           = type,
                value          = value,
                remainingTurns = turns
            });
        }

        // 변경 즉시 재계산 및 UI 갱신
        RecalculateModifiers();
        OnStatsChanged?.Invoke();
    }

    // 적이나 아군 체력을 확인하고 종료 로직 작동
    void CheckEnd()
    {
        if (!IsInCombat) return;
        
        if (enemyHp <= 0)
        {
            Debug.Log("Victory!");
            // 승리 시 추가 로직
        }
        else if (playerHp <= 0)
        {
            Debug.Log("Defeat...");
        }
    }
    
    // 필살기 게이지 획득 (합성, 카드 사용 시 호출)
    public void GainSpecialGauge(int amount = 1)
    {
        // 최대치일때 return
        if (CurrentSpecialGauge >= MaxSpecialGauge) return;

        // 현재 게이지에 amount 추가, 최대치를 넘으면 최대치로 수치 고정
        CurrentSpecialGauge = Mathf.Min(CurrentSpecialGauge + amount, MaxSpecialGauge);
        OnSpecialGaugeChanged?.Invoke(CurrentSpecialGauge, MaxSpecialGauge);

        // 게이지가 최대치라면
        if (IsSpecialReady)
            OnSpecialReady?.Invoke();
    }

    // 필살기 발동 직후 호출 : 게이지 초기화
    public void ConsumeSpecialGauge()
    {
        // 게이지 초기화
        CurrentSpecialGauge = 0;
        OnSpecialGaugeChanged?.Invoke(CurrentSpecialGauge, MaxSpecialGauge);
    }
    
    // 필살기 : 물기
    public void SpecialAttack(int amount)
    {
        // 데미지 음수 보정
        int dmg = Mathf.Max(0, amount);
        
        // 실드 무시 - 직접 피해
        enemyHp = Mathf.Max(0, enemyHp - dmg);

        // UI, 이펙트 갱신
        OnStatsChanged?.Invoke();
        OnEnemyHit?.Invoke();
        
        OnSpecialUsed?.Invoke(0);
        
        // 죽음 체크
        if (enemyHp <= 0)
            OnEnemyDeath?.Invoke();
    }

    // 필살기 : 웅크리기
    public void SpecialShield(int amount)
    {
        playerShield += amount;
        OnStatsChanged?.Invoke();
        OnSpecialUsed?.Invoke(1);
    }

    // 필살기 : 으르렁거리기 (기절)
    public void SpecialStun(int turns)
    {
        enemyStunTurns = turns;
        OnEnemyStuned?.Invoke();
        OnStatsChanged?.Invoke();
        OnSpecialUsed?.Invoke(2);
    }
    
    /// <summary>
    /// 받은 피해의 일정 비율을 반사하는 버프를 건다.
    /// </summary>
    public void AddReflectBuff(float percent, int turns)
    {
        playerReflectPercent = percent;
        reflectTurnsRemaining = turns;
        OnStatsChanged?.Invoke();
    }
}