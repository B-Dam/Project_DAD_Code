# Battle/UI/SpecialAttack — 필살기 UI

필살기 선택, 게이지 연동, 컷신 호출 등 **특수 공격 UI**.

---

## 📂 폴더 내 스크립트
 ├── SpecialAbilityPanel.cs
 ├── SpecialAbiltyUI.cs
 ├── SpecialCardHover.cs
 ├── SpecialCardIdle.cs
 ├── SpecialGaugeUI.cs

---

## 🔎 대표 스크립트: `SpecialAbilityPanel.cs` 예시

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpecialAbilityPanel : MonoBehaviour
{
    [Header("버튼 3개 (Inspector에서 크기 3으로)")]
    [SerializeField] private Button[] abilityButtons;
    
    [Header("필살기 사용 후 컷씬")]
    [SerializeField] private Animator cutsceneAnimator; // 컷씬용 Animator
    [SerializeField] private CombatAnimationController animCtrl; // 스킬 애니메이션 컨트롤러
    [SerializeField] private float cutsceneDuration = 1.5f; // 컷씬 시간
    
    [Header("컷씬 패널")]
    [SerializeField] private RectTransform cutscenePanel; // UI Image를 감싸는 RectTransform
    [SerializeField] private float enterDurationFast = 0.3f; // 첫 진입 속도
    [SerializeField] private float slowDuration = 0.6f; // 중앙에서 천천히
    [SerializeField] private float exitDurationFast = 0.2f;  // 종료 빠르게
    [SerializeField] private float slowOffset = 100f; // 느림 구간 Offset
    
    [Header("버튼 트윈 세팅")]
    [SerializeField] private float entryYOffset   = 300f;  // 씬에 배치된 Y에서 이만큼 아래에서 시작
    [SerializeField] private float entryDuration = 0.5f;
```

> 위 코드는 핵심 로직의 일부 예시입니다. 실제 구현은 프로젝트 원본 파일을 참고하세요.
