# 🃏 Battle/UI — 전투 UI

핸드/카드 드로우, 드래그-드롭, 프리뷰, 체력/게이지 갱신 등 **플레이어 인터랙션 UI**를 담당합니다.

---

## 📦 폴더 구조
```
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
```

---

## ✨ 설계 특징 (Highlights)
- DOTween으로 카드 레이아웃/포커스/드래그 반환 연출
- 핸드 정렬, 곡선 배치, 강조/확대 등 시각 피드백 강화
- `HandManager.UseCard` → `CombatManager.ApplySkill` 연동

---

## 🔁 핵심 흐름
Draw → Layout → Drag → Drop → UseCard

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `HandManager.cs`
```csharp
public bool UseCard(CardView cv)
    {
        // AP 체크
        if (currentAP < cv.data.costAP)
        {
            return false;
        }

        // AP 차감, 효과 적용
        currentAP -= cv.data.costAP;
        CombatUI.Instance.UpdateAP(currentAP);
        
        // 필살기 게이지 증가
        CombatManager.Instance.GainSpecialGauge(1);
        
        CombatManager.Instance.ApplySkill(cv.data, isPlayer: true);

        // 기본 카드(rank==1)만 discard에 추가하고, 합성된 카드(rank>1)는 버리지 않음
        if (cv.data.rank == 1)
            discard.Add(cv.data);

        // 현재 카드만 LayoutHand 스킵하기 위한 플래그
        currentlyDiscarding = cv;

        // Tween 시작 전에 남아있는 트윈 모두 제거
        var rt = cv.Rect;
        rt.DOKill();
        
        // 버림 덱 위치 계산
        Vector3 worldTarget = discardDeckPile.position;
        Vector3 localTarget = ((RectTransform)rt.parent).InverseTransformPoint(worldTarget);
        
        // 카드 이동 + 축소 애니메이션
        rt.DOAnchorPos(localTarget, discardMoveDuration)
          .SetEase(Ease.InQuad);
        rt.DOScale(0.2f, discardMoveDuration)
          .SetEase(Ease.InQuad)
          .OnComplete(() =>
          {
              // Hand에서 제거
              handViews.Remove(cv);
              
              // 인덱스 재설정
              for (int i = 0; i < handViews.Count; i++)
                  handViews[i].index = i;
              
              Destroy(cv.gameObject);
              LayoutHand();
          });

        isDraggingCard = false;
        return true;
    }

// ...

public bool TryCombine(CardData baseCardData)
    {
        // 카드 합성이 막힌 경우 바로 반환
        if (!AllowCombine) return false;
        
        // AP 체크
        if (currentAP < combineAPCost) return false;

        // 같은 이름·같은 rank 카드 두 장 찾기
        var candidates = handViews
                         .Where(cv
// (이하 생략)
```
