using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("SO 로드")]
    [SerializeField] CharacterDataSO defaultEnemy;

    [Serializable]
    public class SaveMetadata
    {
        public DateTime timestamp; // 저장용 시간
        public string chapterName; // 저장용 챕터 이름
        public string questName;   // 저장용 퀘스트 이름
    }
    public CharacterDataSO playerData { get; private set; }
    public CharacterDataSO enemyData { get; private set; }
    public CardData[] allCards { get; private set; }   // 모든 카드 SO

    public Dictionary<string, QuestData> questTable;
    public Dictionary<string, CharacterData> characterTable;
    public Dictionary<string, DialogueData> dialogueTable;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllSkillData();
            LoadAllCsvData();
        }
        else Destroy(gameObject);
    }

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

    private void LoadAllCsvData()
    {
        questTable = CsvDatabase.LoadCsvDict("questDB", f => new QuestData(f));
        characterTable = CsvDatabase.LoadCsvDict("characterDB", f => new CharacterData(f));
        dialogueTable = CsvDatabase.LoadCsvDict("dialogueDB", f => new DialogueData(f));
        Debug.Log($"퀘스트 {questTable.Count}개, 캐릭터 {characterTable.Count}개, 대화 {dialogueTable.Count}개 로드 완료");
    }

    /// <summary>
    /// 런타임에 적을 변경하고 싶을 때 호출
    /// </summary>
    public void SetEnemy(CharacterDataSO newEnemy)
    {
        enemyData = newEnemy;
    }

    /// <summary>
    /// 플레이어 덱 구성용: ownerID가 playerData의 ID와 같고, 1성 카드만 반환
    /// </summary>

    public CardData[] GetPlayerCards()
    {
        return allCards
               .Where(c => c.ownerID == playerData.ownerID && c.rank == 1)
               .ToArray();
    }

    /// <summary>
    /// 적 스킬용: ownerID가 enemyData의 ID와 같은 모든 카드 반환
    /// </summary>
    public CardData[] GetEnemySkills()
    {
        return allCards
               .Where(c => c.ownerID == enemyData.ownerID)
               .ToArray();
    }

    /// <summary>
    /// 합성용: 특정 displayName과 rank에 해당하는 CardData 반환
    /// </summary>
    public CardData GetCard(string displayName, int rank)
    {
        var card = allCards
            .FirstOrDefault(c =>
                c.displayName == displayName &&
                c.rank == rank);
        if (card == null)
            Debug.LogError($"GetCard 실패: '{displayName}' rank={rank} 카드가 없습니다.");
        return card;
    }
    
    // JSON 직렬화를 위해 Dictionary<string,string>을 List 로 래핑
    [Serializable]
    private class JsonDictWrapper
    {
        public List<string> keys   = new List<string>();
        public List<string> values = new List<string>();

        public JsonDictWrapper(Dictionary<string, string> dict)
        {
            foreach (var kv in dict)
            {
                keys.Add(kv.Key);
                values.Add(kv.Value);
            }
        }

        public Dictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>();
            for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
                result[keys[i]] = values[i];
            return result;
        }
    }
    
    static string CurrentChapterName()
    {
        // DataManager 인스턴스가 없으면 빈 문자열 반환
        if (Instance == null) return string.Empty;

        // DialogueManager가 없으면 빈 문자열 반환
        if (DialogueManager.Instance == null) return string.Empty;

        // CSV로 로드된 모든 퀘스트 정보
        var quests = Instance.questTable.Values;

        // 대화 진행 상태(HasSeen)를 보고, conditionStart를 봤고 conditionComplete는 아직 안 본 퀘스트 찾기
        var active = quests.FirstOrDefault(q =>
            DialogueManager.Instance.HasSeen(q.conditionStart.ToString()) &&
            !DialogueManager.Instance.HasSeen(q.conditionComplete.ToString())
        );

        // 있으면 챕터 이름 반환, 없으면 빈 문자열
        return active != null ? active.chapterName : string.Empty;
    }

    static string CurrentQuestName()
    {
        // DataManager 인스턴스가 없으면 빈 문자열 반환
        if (Instance == null) return string.Empty;

        // DialogueManager가 없으면 빈 문자열 반환
        if (DialogueManager.Instance == null) return string.Empty;

        // CSV로 로드된 모든 퀘스트 정보
        var quests = Instance.questTable.Values;

        // 대화 진행 상태(HasSeen)를 보고, conditionStart를 봤고 conditionComplete는 아직 안 본 퀘스트 찾기
        var active = quests.FirstOrDefault(q =>
            DialogueManager.Instance.HasSeen(q.conditionStart.ToString()) &&
            !DialogueManager.Instance.HasSeen(q.conditionComplete.ToString())
        );

        // 있으면 퀘스트 이름 반환, 없으면 빈 문자열
        return active != null ? active.questName : string.Empty;
    }
    
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
    
    public static SaveMetadata GetSaveMetadata(int slot)
    {
        string key = (slot == 0) ? "AutoSlot" : $"ManualSlot{slot}";
        var ticksStr = PlayerPrefs.GetString(key + "_Timestamp", string.Empty);
        DateTime ts = string.IsNullOrEmpty(ticksStr) ? DateTime.MinValue : new DateTime(long.Parse(ticksStr));
        return new SaveMetadata {
            timestamp  = ts,
            chapterName = PlayerPrefs.GetString(key + "_Chapter"),
            questName   = PlayerPrefs.GetString(key + "_Quest")
        };
    }
}