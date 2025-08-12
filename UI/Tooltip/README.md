# Battle/UI/Tooltip — 툴팁 UI

전투 UI 요소(AP, 덱, 엔드턴, 적 정보, 환경효과 등)의 툴팁 표시.

---

## 📂 폴더 내 스크립트
 ├── APIconHover.cs
 ├── DeckIconHover.cs
 ├── EndTurnHover.cs
 ├── EnemyPanelHover.cs
 ├── EnvironmentTooltip.cs
 ├── TooltipController.cs
 ├── TooltipHover.cs

---

## 🔎 대표 스크립트: `TooltipController.cs` 예시

```csharp
using DG.Tweening;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance { get; private set; }
    
    [Header("툴팁 패널")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    
    [Header("페이드 시간")]
    [SerializeField] private float fadeDuration = 0.2f;
    
    // panelRoot에 붙어 있을 CanvasGroup
    private CanvasGroup cg;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

```

> 위 코드는 핵심 로직의 일부 예시입니다. 실제 구현은 프로젝트 원본 파일을 참고하세요.
