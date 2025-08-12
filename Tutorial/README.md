# Battle/Tutorial — 튜토리얼

게임 진행을 안내하는 튜토리얼 스텝 정의 및 연출.

---

## 📂 폴더 내 스크립트
 ├── TutorialManager.cs
 ├── TutorialStep.cs

---

## 🔎 대표 스크립트: `TutorialManager.cs` 예시

```csharp
using System.Collections;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Canvas RectTransform")]
    public RectTransform canvasRect; 
    
    [Header("턴 종료 버튼")]
    public Button endTurnButton;
    
    [Header("필살기 버튼")]
    public Button specialButton;

    [Header("필살기 스킬 버튼")] 
    public Button specialAttackButton;
    public Button specialDefenseButton;
    public Button specialDebuffButton;
    
    [Header("튜토리얼 스텝 설정")]
    public TutorialStep[] steps;
```

> 위 코드는 핵심 로직의 일부 예시입니다. 실제 구현은 프로젝트 원본 파일을 참고하세요.
