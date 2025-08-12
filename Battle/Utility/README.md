# 🧰 Battle/Utility — 유틸리티

전투 텍스트 포맷팅, 디버그 헬퍼 등 공통 유틸입니다.

---

## 📦 폴더 구조
```
 ├── TextFormatter.cs
```

---

## ✨ 설계 특징 (Highlights)
- 문자열 템플릿/하이라이트 포맷
- 테스트 도중 로그 가독성 향상

---

## 🔁 핵심 흐름
Format → Apply

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `TextFormatter.cs`
```csharp
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
```
