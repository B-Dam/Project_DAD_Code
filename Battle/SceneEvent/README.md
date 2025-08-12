# 🎬 Battle/SceneEvent — 전투 씬 세팅

씬 진입 시 **배경/적/환경효과** 세팅, 컷씬 페이드, 튜토리얼 강제 진입 등을 제어합니다.

---

## 📦 폴더 구조
```
 ├── CombatDataHolder.cs
 ├── CombatSceneController.cs
 ├── CombatSetupData.cs
 ├── CombatTriggerEvent.cs
```

---

## ✨ 설계 특징 (Highlights)
- 씬 진입 파라미터(`CombatSetupData`) 기반 동적 세팅
- 페이드 인/아웃과 대화/컷씬 순서 제어
- 튜토리얼 강제: 특정 덱/연출 강제 주입

---

## 🔁 핵심 흐름
Load Scene → Setup Background → Setup Enemy → Apply Environment → Fade In

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `CombatSceneController.cs`
```csharp
// (핵심 메서드를 찾지 못했습니다 — 파일을 확인해 주세요)
```
