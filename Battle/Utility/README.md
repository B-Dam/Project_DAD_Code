# 🧰 Battle/Utility — 유틸리티

유틸리티 모듈 설명입니다.

---

## ✨ 설계 특징 (Highlights)
- (추가 예정)

---

## 🔁 핵심 흐름
Format

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `TextFormatter.cs`
```csharp
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
