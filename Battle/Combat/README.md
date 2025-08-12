# ⚔️ Battle/Combat — 전투 코어

턴 기반 전투의 **라운드 진행**, **AI 행동**, **데미지/실드/버프 계산**, **승패 판정**을 담당합니다. 이벤트 드리븐으로 UI/애니메이션과 느슨하게 결합되어 있습니다.

---

## ✨ 설계 특징 (Highlights)
- 🔔 이벤트: `OnCombatStart/End`, `OnPlayerSkillUsed/OnEnemySkillUsed` 발행
- 🧠 책임 분리: 턴=`TurnManager`, AI=`EnemyAI`, 계산=`CombatManager`
- 🔌 확장: 카드/스킬 추가 시 `ApplySkill` 분기만 확장
- 🧱 내결함성: 승패/예외 상황에서도 루프 종료 보장

---

## 🔁 핵심 흐름
StartCombat → StartPlayerTurn → UseCard/ApplySkill → StartEnemyTurn → CheckWinLose → EndCombat

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `CombatManager.cs`
```csharp
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

// ...

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

// ...

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
```

---

## 📐 설계 메모
- 단방향 흐름 유지
- CombatManager만 수치 변경
- ApplySkill 계산/애니 분리
