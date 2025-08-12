# 📁 Data — 공통 데이터(SO)

공통 데이터(SO) 모듈 설명입니다.

---

## ✨ 설계 특징 (Highlights)
- (추가 예정)

---

## 🔁 핵심 흐름
Load → Query

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `DialogueData.cs`
```csharp
public DialogueData(string[] f)
    {
        dialogueId      = f[0];
        speakerId       = int.Parse(f[1]);
        dialogueText    = f[2];
    }
```
