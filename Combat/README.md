# Battle/Combat — 전투 코어

턴 관리, 적 AI, 데미지/버프 계산, 이벤트 발행 등 **전투 코어 로직**을 담당합니다.

---

## 📂 폴더 내 스크립트
 ├── CombatManager.cs
 ├── EnemyAI.cs
 ├── TurnManager.cs

---

## 🔎 대표 스크립트: `CombatManager.cs` 예시

```csharp
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
```

> 위 코드는 핵심 로직의 일부 예시입니다. 실제 구현은 프로젝트 원본 파일을 참고하세요.
