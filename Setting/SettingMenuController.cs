using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingMenuController : MonoBehaviour
{
    public enum MenuType { Sound, Save, Load }

    [Header("메뉴 버튼")]
    [SerializeField] private Image soundButtonImage;
    [SerializeField] private Image saveButtonImage;
    [SerializeField] private Image loadButtonImage;
    
    [Header("버튼 색상")]
    [SerializeField] private Color normalColor   = Color.white;
    [SerializeField] private Color selectedColor = Color.green;

    [Header("내용 패널")]
    [SerializeField] private GameObject soundContent;
    [SerializeField] private GameObject saveContent;
    [SerializeField] private GameObject loadContent;
    
    [Header("세이브 로드 매니저")]
    [SerializeField] private SaveLoadManager saveLoadManager;
    
    [Header("설정창 전체 Panel")]
    [SerializeField] private GameObject settingsPanel;
    
    [Header("퀘스트 UI")] 
    public GameObject questUI;
    
    private void Start()
    {
        settingsPanel.SetActive(false);
        // 시작할 때 기본 메뉴 선택
        ShowMenu(MenuType.Sound);
    }
    
    // 설정창 진입 방지용 bool 헬퍼
    bool IsSettingsBlocked()
    {
        // 대화 진행 중
        var dm = DialogueManager.Instance;
        if (dm != null && dm.IsDialogueActive) return true;

        // 컷씬 재생/준비 중
        var cc = CutsceneController.Instance;
        if (cc != null && (cc.IsVideoPlaying || cc.IsPreparing)) return true;

        // NPC 이동 중(플레이어 조작 막힘)
        var pc = PlayerController.Instance;
        if (pc != null && !pc.enabled) return true;

        return false;
    }
    
    private void Update()
    {
        // 해당 상태면 Esc 무시
        if (IsSettingsBlocked()) return;
        
        // Esc 키를 누르면 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isOn = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isOn);
            if (!isOn)
            {
                settingsPanel.transform.SetAsLastSibling();
                ShowMenu(MenuType.Sound); // 열 때 기본 탭 지정
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        // 전투 중에 QuestUI 비활성화
        if (CombatManager.Instance != null && CombatManager.Instance.IsInCombat)
            questUI.SetActive(false);
        else
            questUI.SetActive(true);
    }

    // 버튼 OnClick 으로 연결
    public void OnClickSound() => ShowMenu(MenuType.Sound);
    public void OnClickSave()
    {
        ShowMenu(MenuType.Save);
        saveLoadManager.SwitchMode(SaveLoadManager.Mode.Save);
    }

    public void OnClickLoad()
    {
        ShowMenu(MenuType.Load);
        saveLoadManager.SwitchMode(SaveLoadManager.Mode.Load);
    }

    private void ShowMenu(MenuType type)
    {
        // 오른쪽 콘텐츠 활성/비활성
        soundContent.SetActive(type == MenuType.Sound);
        saveContent.SetActive(type == MenuType.Save);
        loadContent.SetActive(type == MenuType.Load);
        
        // 버튼 색상 강조
        soundButtonImage.color = (type == MenuType.Sound) ? selectedColor : normalColor;
        saveButtonImage.color = (type == MenuType.Save) ? selectedColor : normalColor;
        loadButtonImage.color = (type == MenuType.Load) ? selectedColor : normalColor;
    }
}