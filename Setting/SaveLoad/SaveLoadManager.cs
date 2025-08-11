using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections; 

public class SaveLoadManager : MonoBehaviour
{
    public enum Mode { Save, Load }
    
    [Header("컨텐츠 패널")]
    [SerializeField] GameObject saveContent;  
    [SerializeField] GameObject loadContent;

    [Header("슬롯 버튼 & 라벨")]
    [SerializeField] Button[] saveSlots;     // [0]=Auto, [1..3]=Manual
    [SerializeField] TMP_Text[] saveSlotLabels;
    [SerializeField] Button[] loadSlots;
    [SerializeField] TMP_Text[] loadSlotLabels;

    [Header("저장 확인 패널")]
    [SerializeField] GameObject saveConfirmPanel;
    [SerializeField] TMP_Text saveConfirmText;
    [SerializeField] Button saveYesButton;
    [SerializeField] Button saveNoButton;

    [Header("불러오기 확인 패널")]
    [SerializeField] GameObject loadConfirmPanel;
    [SerializeField] TMP_Text loadConfirmText;
    [SerializeField] Button loadYesButton;
    [SerializeField] Button loadNoButton;

    private Mode currentMode;
    private int selectedSlotIndex;

    void Start()
    {
        // 씬 전환 직후, 혹은 Start 시점에 현재 씬의 모든 ISaveable을 등록
        SaveLoadManagerCore.Instance.RegisterSaveables();
        
        for (int i = 0; i < saveSlots.Length; i++)
        {
            int idx = i;
            saveSlots[idx].onClick.AddListener(() => OnClickSaveSlot(idx));
        }

        for (int i = 0; i < loadSlots.Length; i++)
        {
            int idx = i;
            loadSlots[i].onClick.AddListener(() => OnClickLoadSlot(idx));
        }
        
        saveYesButton.onClick.AddListener(OnConfirmSave);
        saveNoButton.onClick.AddListener(() => saveConfirmPanel.SetActive(false));
        loadYesButton.onClick.AddListener(OnConfirmLoad);
        loadNoButton.onClick.AddListener(() => loadConfirmPanel.SetActive(false));

        SwitchMode(Mode.Save);
    }

    public void SwitchMode(Mode mode)
    {
        currentMode = mode;
        saveContent.SetActive(mode == Mode.Save);
        loadContent.SetActive(mode == Mode.Load);
        saveConfirmPanel.SetActive(false);
        loadConfirmPanel.SetActive(false);
        RefreshAllSlots();
    }

    void RefreshAllSlots()
    {
        RefreshSaveSlots();
        RefreshLoadSlots();
    }
    
    void RefreshSaveSlots()
    {
        if (saveSlotLabels == null) return;

        for (int i = 0; i < saveSlotLabels.Length; i++)
        {
            bool hasData = SaveLoadManagerCore.Instance.HasSaveFile(i);
            if (!hasData)
            {
                saveSlotLabels[i].text = "빈 슬롯";
                continue;
            }
            var meta = DataManager.GetSaveMetadata(i);
            saveSlotLabels[i].text = BuildSlotLabelText(meta);
        }
    }

    void RefreshLoadSlots()
    {
        if (loadSlotLabels == null) return;

        for (int i = 0; i < loadSlotLabels.Length; i++)
        {
            bool hasData = SaveLoadManagerCore.Instance.HasSaveFile(i);
            if (!hasData)
            {
                loadSlotLabels[i].text = "빈 슬롯";
                continue;
            }
            var meta = DataManager.GetSaveMetadata(i);   // ← 저장된 메타 그대로
            loadSlotLabels[i].text = BuildSlotLabelText(meta);
        }
    }

    void OnClickSaveSlot(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        // 0번(Auto)은 수동 저장 불가
        if (currentMode == Mode.Save && slotIndex == 0) return;

        saveConfirmText.text = $"슬롯 {slotIndex}에 저장하시겠습니까?\n슬롯에 저장된 이전의 데이터는 지워집니다!";
        saveConfirmPanel.SetActive(true);
    }

    void OnConfirmSave()
    {
        // 다시 한 번 저장 대상( ISaveable )을 최신화
        SaveLoadManagerCore.Instance.RegisterSaveables();
        
        // 실제 Save
        SaveLoadManagerCore.Instance.SaveGame(selectedSlotIndex);
        
        // 메타 기록
        DataManager.UpdateSaveMetadata(selectedSlotIndex);
        
        // 슬롯 라벨 즉시 갱신
        RefreshAllSlots();
        
        // 패널 닫기
        saveConfirmPanel.SetActive(false);
    }

    void OnClickLoadSlot(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        loadConfirmText.text = $"슬롯 {slotIndex}의 데이터를 불러오시겠습니까?\n저장하지 않은 데이터는 지워집니다!";
        loadConfirmPanel.SetActive(true);
    }

    void OnConfirmLoad()
    {
        // 로드 전에도 저장 대상 레지스트리 갱신
        SaveLoadManagerCore.Instance.RegisterSaveables();
        // 실제 Load
        SaveLoadManagerCore.Instance.LoadGame(selectedSlotIndex);
        
        loadConfirmPanel.SetActive(false);
        
        StartCoroutine(RefreshAfterLoad());
    }
    
    IEnumerator RefreshAfterLoad()
    {
        yield return null; // 1프레임
        yield return null; // 2프레임 (안정성)
        
        // 로드 시 퀘스트 텍스트 갱신
        if (QuestGuideUI.Instance != null)
            QuestGuideUI.Instance.RefreshQuest();
        
        // 슬롯 라벨 동기화
        RefreshAllSlots();
    }
    
    string BuildSlotLabelText(DataManager.SaveMetadata meta)
    {
        if (meta == null) return "No Data";
        string time    = (meta.timestamp == DateTime.MinValue) ? "-" : meta.timestamp.ToString("yyyy.MM.dd HH:mm");
        string chapter = string.IsNullOrEmpty(meta.chapterName) ? "-" : meta.chapterName;
        string quest   = string.IsNullOrEmpty(meta.questName)   ? "-" : meta.questName;
        return $"{time}\n{chapter} - {quest}";
    }
}