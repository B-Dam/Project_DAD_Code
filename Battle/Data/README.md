# 📦 Battle/Data — 전투 데이터

전투 데이터 모듈 설명입니다.

---

## ✨ 설계 특징 (Highlights)
- (추가 예정)

---

## 🔁 핵심 흐름
Load Tables → Build Index → Query

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `DataManager.cs`
```csharp
public CardData[] GetEnemySkills()
    {
        return allCards
               .Where(c => c.ownerID == enemyData.ownerID)
               .ToArray();
    }

// ...

public static void UpdateSaveMetadata(int slot)
    {
        string key = (slot == 0) ? "AutoSlot" : $"ManualSlot{slot}";
        PlayerPrefs.SetString(key + "_Timestamp", DateTime.Now.Ticks.ToString());

        // 1) QuestGuideUI에 보이는 텍스트 우선 사용
        string uiQuest = null;
        if (QuestGuideUI.Instance != null && QuestGuideUI.Instance.guideText != null)
        {
            // 최신 상태 보장
            QuestGuideUI.Instance.RefreshQuest();

            uiQuest = QuestGuideUI.Instance.guideText.text;
        }

        string chapter = string.Empty;
        string quest   = string.Empty;

        if (!string.IsNullOrEmpty(uiQuest))
        {
            // 동일한 퀘스트명을 questTable에서 찾아 챕터까지 매핑
            var row = Instance?.questTable?.Values?
                .FirstOrDefault(q => q.questName == uiQuest);

            if (row != null)
            {
                chapter = row.chapterName;
                quest   = row.questName;   // UI와 동일
            }
            else
            {
                // 테이블 매칭이 안되면 최소한 UI 텍스트만 저장
                quest = uiQuest;
            }
        }
        else
        {
            // 2) UI가 없거나 비어 있으면 기존 계산으로 폴백
            chapter = CurrentChapterName() ?? string.Empty;
            quest   = CurrentQuestName()   ?? string.Empty;
        }

        PlayerPrefs.SetString(key + "_Chapter", chapter);
        PlayerPrefs.SetString(key + "_Quest",   quest);
        PlayerPrefs.Save();
    }

// ...

private void LoadAllSkillData()
    {
        // 모든 카드 SO 로드
        allCards = Resources.LoadAll<CardData>("ScriptableObjects/Cards");

        // 모든 캐릭터 SO 로드
        var chars = Resources.LoadAll<CharacterDataSO>("ScriptableObjects/Characters");
        // 플레이어는 항상 Mono로 고정
        playerData = chars.First(c => c.characterId == "Mono");

        // Enemy: Inspector에서 지정된 defaultEnemy가 있으면 사용, 없으면 첫번째 또는 예외 처리
        if (defaultEnemy != null)
            enemyData = defaultEnemy;
        else
            enemyData = chars.First(c => c.characterId != playerData.characterId);
    }
```
