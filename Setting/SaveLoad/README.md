# 💾 Setting/SaveLoad — 저장/로드 파이프라인

세이브 슬롯 → 직렬화 래퍼 → 개별 오브젝트 상태 복원까지 **두 패스 복원 루틴**을 포함합니다.

---

## 📦 폴더 구조
```
 ├── BoxSave.cs
 ├── CameraSave.cs
 ├── CombatTriggerSave.cs
 ├── DialogueSave.cs
 ├── HoleSave.cs
 ├── ISaveable.cs
 ├── MapManagerSave.cs
 ├── MapTriggerSave.cs
 ├── NPCMoveTriggerSave.cs
 ├── NPCSave.cs
 ├── PlayerSave.cs
 ├── QuestItemSave.cs
 ├── SaveLoadManager.cs
 ├── SaveLoadManagerCore.cs
 ├── UniqueID.cs
```

---

## ✨ 설계 특징 (Highlights)
- ISaveable 수집 → SaveWrapper/SaveEntry 직렬화
- 두 단계 복원(레퍼런스 안정화 후 후처리 `OnPostLoad`)
- UniqueID 보장(에디터 OnValidate + 런타임 보정)
- 안정화 루프(settle)로 순서/의존성 문제 해결

---

## 🔁 핵심 흐름
Collect → Serialize → Load 1st pass → Stabilize → PostLoad

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `SaveLoadManagerCore.cs`
```csharp
// (핵심 메서드를 찾지 못했습니다 — 파일을 확인해 주세요)
```
