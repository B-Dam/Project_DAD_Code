# 📦 Battle/Data — 전투 데이터

카드/캐릭터/환경효과 등 전투에 필요한 데이터와 로더입니다.

---

## 📦 폴더 구조
```
 ├── CardData.cs
 ├── CharacterData.cs
 ├── DataManager.cs
 ├── Editor/SkillDataImporter.cs
 ├── Enums/OwnerType.cs
 ├── EnvironmentEffect.cs
```

---

## ✨ 설계 특징 (Highlights)
- ScriptableObject/CSV 기반 데이터 로딩
- 전투 시점에 필요한 최소 데이터만 참조
- 에디터 임포터로 팀 협업 효율화

---

## 🔁 핵심 흐름
Load CSV/SO → Build Lookup → Provide Read API

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `DataManager.cs`
```csharp
public CardData[] GetEnemySkills()
    {
        return allCards
               .Where(c => c.ownerID == enemyData.ownerID)
               .ToArray();
    }
```
