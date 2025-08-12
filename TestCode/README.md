# Battle/TestCode — 테스트 도우미

테스트/디버깅을 위한 임시 코드 모음.

---

## 📂 폴더 내 스크립트
 ├── ClickDebugger.cs
 ├── CombatInitializer.cs

---

## 🔎 대표 스크립트: `ClickDebugger.cs` 예시

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickDebugger : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[ClickDebugger] {name} 클릭됨 at {eventData.position}");
    }
    
    void Start()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(() => Debug.Log("[ButtonClickTester] onClick 리스너 호출!"));
    }
}
```

> 위 코드는 핵심 로직의 일부 예시입니다. 실제 구현은 프로젝트 원본 파일을 참고하세요.
