# ⚔️ Battle/Combat — 전투 코어

턴 기반 전투의 **라운드 진행**, **AI 행동**, **데미지/실드/버프 계산**을 담당합니다. 전투 시작/종료, 이벤트 브로드캐스트까지 코어 루프가 모두 여기에 있습니다.

---

## 📦 폴더 구조
```
 ├── CombatManager.cs
 ├── EnemyAI.cs
 ├── TurnManager.cs
```

---

## ✨ 설계 특징 (Highlights)
- 이벤트 기반 구조: `OnCombatStart/End`, `OnPlayerSkillUsed/OnEnemySkillUsed`
- 분리된 책임: 턴은 `TurnManager`, 의사결정은 `EnemyAI`, 계산은 `CombatManager`
- 가시성: UI/애니메이션 레이어로 이벤트만 발행하여 결합도 최소화
- 확장 포인트: 카드/스킬 추가 시 `ApplySkill` 분기만 확장

---

## 🔁 핵심 흐름
StartCombat → Player Turn → UseCard/ApplySkill → Enemy Turn → CheckWinLose

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
               
// (이하 생략)
```
