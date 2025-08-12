# 🃏 Battle/UI — 전투 UI

핸드/카드 드로우, 드래그-드롭, 프리뷰, 체력/게이지 갱신 등 플레이어 인터랙션 전반을 담당합니다.

---

## ✨ 설계 특징 (Highlights)
- 🎯 입력 흐름: `CardView`→`HandManager`→`CombatManager.ApplySkill`
- 🎬 DOTween 기반 레이아웃/포커스/리턴 연출
- 🧮 곡선 배치 + Hover 강조

---

## 🔁 핵심 흐름
Draw → LayoutHand → BeginDrag → Drag → EndDrag → UseCard

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
                         .Where(cv =>
                             cv.data.displayName == baseCardData.displayName &&
                             cv.data.rank == baseCardData.rank)
                         .ToList();
        if (candidates.Count < 2) return false;

        // AP 차감
        currentAP -= combineAPCost;
        CombatUI.Instance.UpdateAP(currentAP);
        CombatUI.Instance.UpdateUI();
        
        // 필살기 게이지 획득
        CombatManager.Instance.GainSpecialGauge(2);
        
        // 두 장 제거 (트윈 끊지 않도록 바로 DOKill + Destroy)
        foreach (var cv in candidates.Take(2))
        {
            // 소모 카드 데이터를 discard에 추가
            discard.Add(cv.data);
            
            // 핸드에서 제거
            handViews.Remove(cv);
            Destroy(cv.gameObject);
        }
        
        int nextRank = baseCardData.rank + 1;
        var newData = DataManager.Instance.GetCard(baseCardData.displayName, nextRank);
        if (newData == null)
        {
            // 레이아웃 복귀
            DOVirtual.DelayedCall(0f, LayoutHand);
            return false;
        }
        
        // 신규 카드 생성 및 초기화 (여기서 AddCard()만, 레이아웃은 따로)
        var go   = Instantiate(cardPrefab, handContainer);
        var cvNew = go.GetComponent<CardView>();
        cvNew.Initialize(newData, this, enemyDropZone);
        
        // 카드 합성 성공해서 생성됐는지 확인 이벤트 호출
        OnCardCombinedNew?.Invoke(cvNew);
        
        // 인덱스 재설정
        for (int i = 0; i < handViews.Count; i++)
            handViews[i].index = i;

        // LayoutHand 한 번 호출해서 새 카드 포함 모든 카드에 애니메이션 걸기
        DOTween.Kill("Layout");  // 기존 레이아웃 트윈만 초기화
        
        isDraggingCard = false;
        LayoutHand();

        // 새 카드만 별도 스케일 애니메이션
        cvNew.Rect.localScale = Vector3.zero;
        cvNew.Rect
             .DOScale(1f, animDuration)
             .SetId("Combine")
             .SetEase(Ease.OutBack);

        return true;
    }

// ...

public void LayoutHand()
    {
        if (isDraggingCard) return;

        int count = handViews.Count;
        if (count == 0) return;

        DOTween.Kill("Layout");

        float totalW = count * cardWidth + (count - 1) * spacing;
        float startX = -totalW / 2f + cardWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            var cv = handViews[i];

            // 버려지는 애니메이션 중인 카드는 완전히 건너뛰기
            if (cv == currentlyDiscarding) continue;

            var rt = cv.Rect;

            float x = startX + i * (cardWidth + spacing);
            rt.DOAnchorPos(new Vector2(x, 0), animDuration).SetId("Layout").SetEase(Ease.OutQuad);
            rt.DOScale(1f, animDuration).SetId("Layout").SetEase(Ease.OutQuad);
        }
    }
```

---

## 🧪 UX 체크리스트
- 드래그 취소 원복 시간 일관성
- Hover 확대 배율 충돌 체크
- 모바일 터치 임계값 조정
