# Battle/UI — 전투 UI

전투 화면의 카드/핸드/게이지/체력바 등 **UI 레이어**와 관련된 스크립트입니다.

---

## 📂 폴더 내 스크립트
 ├── CardList.cs
 ├── CardView.cs
 ├── CombatAnimationController.cs
 ├── CombatUI.cs
 ├── DeckUIManager.cs
 ├── HandManager.cs
 ├── HealthBar.cs
 ├── HidePanel.cs
 ├── PreviewUI.cs
 ├── RetryButton.cs
 ├── SpecialAttack/SpecialAbilityPanel.cs
 ├── SpecialAttack/SpecialAbiltyUI.cs
 ├── SpecialAttack/SpecialCardHover.cs
 ├── SpecialAttack/SpecialCardIdle.cs
 ├── SpecialAttack/SpecialGaugeUI.cs
 ├── Tooltip/APIconHover.cs
 ├── Tooltip/DeckIconHover.cs
 ├── Tooltip/EndTurnHover.cs
 ├── Tooltip/EnemyPanelHover.cs
 ├── Tooltip/EnvironmentTooltip.cs
 ├── Tooltip/TooltipController.cs
 ├── Tooltip/TooltipHover.cs

---

## 🔎 대표 스크립트: `HandManager.cs` 예시

```csharp
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class HandManager : MonoBehaviour
{
    [Header("핸드 설정")] 
    public RectTransform handContainer;   // 카드들이 배치될 부모 RectTransform
    public RectTransform drawDeckPile;    // 드로우 덱 위치
    public RectTransform discardDeckPile; // 버림 덱 위치
    public GameObject cardPrefab;         // CardView 프리팹
    public int maxHandSize = 5;           // 최대 핸드 크기
    public RectTransform enemyDropZone;   // 에디터에서 EnemyPanel 할당

    [Header("합성 설정")]
    [Tooltip("합성 시 소모할 AP")]
    public int combineAPCost = 1;

    [Header("레이아웃 설정")] 
    public float cardWidth = 200f; // 카드 너비 (px, RectTransform width)
    public float spacing = 40f;    // 카드 간 간격
    public float animDuration = 0.25f;

```

> 위 코드는 핵심 로직의 일부 예시입니다. 실제 구현은 프로젝트 원본 파일을 참고하세요.
