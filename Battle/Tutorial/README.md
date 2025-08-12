# 📚 Battle/Tutorial — 튜토리얼

튜토리얼 모듈 설명입니다.

---

## ✨ 설계 특징 (Highlights)
- (추가 예정)

---

## 🔁 핵심 흐름
StartTutorial → ShowStep → WaitCondition → NextStep

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `TutorialManager.cs`
```csharp
public void ShowStep(int i) 
    {
        if (i == currentStep) 
            return;
        
        if (i >= steps.Length)
        {
            EndTutorial();
            return;
        }
        
        // 이전 단계의 남은 트윈 모두 정리
        topPanel.DOKill();
        bottomPanel.DOKill();
        leftPanel.DOKill();
        rightPanel.DOKill();

        // 색 초기화
        ResetPanelColor(topPanel, topOrig);
        ResetPanelColor(bottomPanel, bottomOrig);
        ResetPanelColor(leftPanel, leftOrig);
        ResetPanelColor(rightPanel, rightOrig);
        
        currentStep = i;
        typingComplete = false;
        var step       = steps[i];
        bool hasHole   = step.highlightTarget != null;

        // 1) 메시지 박스 페이드인
        messageBox.anchoredPosition   = step.messageBoxAnchoredPos;
        messageBox.gameObject.SetActive(true);
        messageBoxGroup.interactable = true;
        messageBoxGroup.alpha        = 0f;
        messageBoxGroup.DOFade(1f, 0.3f);

        // 텍스트 타이핑 시작
        StopAllCoroutines();
        StartCoroutine(TypeText(step.instruction));

        // 홀 패널 초기화 모드 분기
        // 먼저 모든 패널 Anchors 초기화
        PrepPanel(topPanel.rectTransform);
        PrepPanel(bottomPanel.rectTransform);
        PrepPanel(leftPanel.rectTransform);
        PrepPanel(rightPanel.rectTransform);
        
        // 모두 비활성화
        topPanel  .gameObject.SetActive(false);
        bottomPanel.gameObject.SetActive(false);
        leftPanel .gameObject.SetActive(false);
        rightPanel.gameObject.SetActive(false);

        if (hasHole)
        {
            // 부분 가림 모드
            Highlight(step.highlightTarget);
        }
        else
        {
            // 전체 가림 모드 → topPanel 하나만 풀스크린
            float cw = canvasRect.rect.width;
            float ch = canvasRect.rect.height;

            topPanel.rectTransform
                    .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, ch);
            topPanel.rectTransform
                    .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, cw);

            topPanel.raycastTarget = true;
            topPanel.gameObject.SetActive(true);
            topPanel.transform.SetAsLastSibling();
        }

        // 4) 스텝 시작 이벤트
        step.onStepStart.Invoke();

        // 5) 완료 시 연결
        step.onStepComplete.RemoveAllListeners();
        step.onStepComplete.AddListener(() => ShowStep(i + 1));
    }

// ...

public void StepCombineUse()
    {
        // 카드 드래그 활성화
        EnableCardDrag();
        
        // 턴 종료 버튼 비활성화
        endTurnButton.interactable = false;

        // 카드 합성 활성화
        EnableCardCombine();
        
        // 카드 합성 확인 후 콜백 호출
        HandManager.OnCardCombinedNew += OnTutorialCardCombined;
    }

// ...

private void OnTutorialCombinedCardUsed(CardData data)
    {
        CombatManager.Instance.OnPlayerSkillUsed -= OnTutorialCombinedCardUsed;
        if (currentOverlay != null) Destroy(currentOverlay);

        // 스텝 완료 이후 다음 스텝으로
        steps[currentStep].onStepComplete.Invoke();
    }
```
