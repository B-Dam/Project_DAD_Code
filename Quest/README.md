# 🗺️ Quest — 퀘스트 UI/안내

퀘스트 UI/안내 모듈 설명입니다.

---

## ✨ 설계 특징 (Highlights)
- (추가 예정)

---

## 🔁 핵심 흐름
Detect → Update

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `QuestGuideUI.cs`
```csharp
public void RefreshQuest()
    {
        if (DialogueManager.Instance == null || DataManager.Instance == null)
            return;

        // CSV로 로드된 모든 퀘스트 정보
        var quests = DataManager.Instance.questTable.Values;

        // 대화 진행 상태를 확인해서 활성화된 퀘스트 찾기
        var activeQuest = quests.FirstOrDefault(q =>
            DialogueManager.Instance.HasSeen(q.conditionStart.ToString()) &&
            !DialogueManager.Instance.HasSeen(q.conditionComplete.ToString())
        );

        if (activeQuest != null)
        {
            string newText = activeQuest.questName;

            if (newText != currentDisplayedQuestName)
            {
                AudioManager.Instance.PlaySFX("QuestSignal");
            }

            guideText.text = newText;
            currentDisplayedQuestName = newText;
            lastValidQuestName = newText;
        }
        else
        {
            guideText.text = lastValidQuestName;
            currentDisplayedQuestName = lastValidQuestName;
        }
    }

// ...

private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
```
