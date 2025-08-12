# Battle/SceneEvent — 전투 씬 세팅

전투 씬 진입 시 배경/적/환경효과 세팅 및 연출.

---

## 📂 폴더 내 스크립트
 ├── CombatDataHolder.cs
 ├── CombatSceneController.cs
 ├── CombatSetupData.cs
 ├── CombatTriggerEvent.cs

---

## 🔎 대표 스크립트: `CombatSceneController.cs` 예시

```csharp
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatSceneController : MonoBehaviour
{
    [Header("배경")]
    [SerializeField] private Image backgroundImage;

    [Header("적")]
    [SerializeField] private GameObject enemyObject; // 적 오브젝트 (애니메이터 포함)
    [SerializeField] private Animator enemyAnimator;

    [Header("GameUI 그룹")]
    [SerializeField] private CanvasGroup gameUIGroup; // 첫 시작시 제어할 게임 UI
    [SerializeField] private CanvasGroup battleStartGroup;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float startAnimDuration = 0.6f;
    [SerializeField] private float uiFadeDuration   = 0.3f;
    [SerializeField] private float battleDisplayTime = 1.2f;
    [SerializeField] private float staggerInterval  = 0.1f;
```

> 위 코드는 핵심 로직의 일부 예시입니다. 실제 구현은 프로젝트 원본 파일을 참고하세요.
