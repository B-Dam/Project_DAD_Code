# Battle/Utility — 유틸리티

전투 UI/로그 출력에 필요한 포맷터 등 공통 유틸.

---

## 📂 폴더 내 스크립트
 ├── TextFormatter.cs

---

## 🔎 대표 스크립트: `TextFormatter.cs` 예시

```csharp
using System.Collections.Generic;

public static class TextFormatter
{
    /// <summary>
    /// template 문자열 안에 있는 "{key}" 플레이스홀더를 values[key] 문자열로 바꿔 줍니다.
    /// </summary>
    public static string Format(string template, Dictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(template) || values == null)
            return template;

        foreach (var pair in values)
        {
            // "{damage}" → "10" 같이 치환
            template = template.Replace("{" + pair.Key + "}", pair.Value);
        }
        return template;
    }
}
```

> 위 코드는 핵심 로직의 일부 예시입니다. 실제 구현은 프로젝트 원본 파일을 참고하세요.
