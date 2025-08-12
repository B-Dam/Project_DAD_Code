# 💡 Battle/UI/Tooltip — 툴팁 UI

AP, 덱, 엔드턴, 적 정보, 환경효과 등 마우스 오버 툴팁을 제공합니다.

---

## 📦 폴더 구조
```
 ├── APIconHover.cs
 ├── DeckIconHover.cs
 ├── EndTurnHover.cs
 ├── EnemyPanelHover.cs
 ├── EnvironmentTooltip.cs
 ├── TooltipController.cs
 ├── TooltipHover.cs
```

---

## ✨ 설계 특징 (Highlights)
- IPointerEnter/Exit 기반 표시
- CanvasGroup 페이드, 설명 텍스트 포맷팅
- 툴팁 컨트롤러에서 일괄 제어

---

## 🔁 핵심 흐름
Pointer Enter → Show Tooltip → Pointer Exit

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `TooltipController.cs`
```csharp
public void Hide()
    {
        cg.DOKill();
        cg.DOFade(0f, fadeDuration)
          .SetEase(Ease.OutSine)
          .OnComplete(() =>
          {
              cg.interactable   = false;
              cg.blocksRaycasts = false;
          });
    }
```
