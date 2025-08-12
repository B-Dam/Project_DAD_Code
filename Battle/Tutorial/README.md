# 📚 Battle/Tutorial — 튜토리얼

스텝 단위로 UI 하이라이트, 입력 가이드, 조건 검증을 수행합니다.

---

## 📦 폴더 구조
```
 ├── TutorialManager.cs
 ├── TutorialStep.cs
```

---

## ✨ 설계 특징 (Highlights)
- Canvas 마스킹으로 포커스 영역 강조
- 타이핑 효과/메시지 진행, 스텝별 콜백
- 전투 시스템과 느슨한 결합(이벤트 구독)

---

## 🔁 핵심 흐름
Start → Show Step → Wait Condition → Next Step

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
            float ch = canvasRect.
// (이하 생략)
```
