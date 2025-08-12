# 📁 Data — 공통 데이터(SO)

대화/퀘스트/캐릭터 등 **프로젝트 전역 ScriptableObject** 모음입니다.

---

## 📦 폴더 구조
```
 ├── CharacterData.cs
 ├── CsvDataBase.cs
 ├── DialogueData.cs
 ├── QuestData.cs
```

---

## ✨ 설계 특징 (Highlights)
- CSV 파이프라인과 연결되는 베이스 클래스
- Dialogue/Quest 데이터 구조화

---

## 🔁 핵심 흐름
Load → Query → Consume

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `DialogueData.cs`
```csharp
public class DialogueData
{
    public string dialogueId;
    public int speakerId;
    public string dialogueText;

    public DialogueData(string[] f)
    {
        dialogueId      = f[0];
        speakerId       = int.Parse(f[1]);
        dialogueText    = f[2];
    }
```
