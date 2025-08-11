using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;

/// <summary>
/// DialogueManager의 진행 상태를 바탕으로
/// conditionStart ~ conditionComplete 범위에 해당하는 퀘스트를 자동으로 찾아
/// UI에 퀘스트 이름을 표시합니다.
/// </summary>
public class QuestGuideUI : MonoBehaviour
{
    public GameObject questUI;

    [Header("출력할 텍스트")]
    public TextMeshProUGUI guideText;

    private string currentDisplayedQuestName = "";
    private string lastValidQuestName = "";

    public static QuestGuideUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

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
}