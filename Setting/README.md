# ⚙️ Setting — 시스템/메뉴

메인 메뉴, 게임 종료, 공통 설정 등을 제공합니다.

---

## 📦 폴더 구조
```
 ├── Audio/AudioManager.cs
 ├── GameExit.cs
 ├── Mainmenumanager.cs
 ├── SaveLoad/BoxSave.cs
 ├── SaveLoad/CameraSave.cs
 ├── SaveLoad/CombatTriggerSave.cs
 ├── SaveLoad/DialogueSave.cs
 ├── SaveLoad/HoleSave.cs
 ├── SaveLoad/ISaveable.cs
 ├── SaveLoad/MapManagerSave.cs
 ├── SaveLoad/MapTriggerSave.cs
 ├── SaveLoad/NPCMoveTriggerSave.cs
 ├── SaveLoad/NPCSave.cs
 ├── SaveLoad/PlayerSave.cs
 ├── SaveLoad/QuestItemSave.cs
 ├── SaveLoad/SaveLoadManager.cs
 ├── SaveLoad/SaveLoadManagerCore.cs
 ├── SaveLoad/UniqueID.cs
 ├── SettingMenuController.cs
 ├── SoundSettingManager.cs
 ├── UI/UIManager.cs
```

---

## ✨ 설계 특징 (Highlights)
- 메뉴에서 저장/불러오기/옵션으로 이동
- 씬 독립 유틸

---

## 🔁 핵심 흐름
Open Menu → Handle Buttons

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `Mainmenumanager.cs`
```csharp
public Button startButton;
    public Button settingsButton;
    public Button quitButton;

    private void Start()
    {
        // 버튼 연결
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

// ...

private void StartGame()
    {
        SceneManager.LoadScene("Marge5"); // 실제 게임 씬 이름
    }

// ...

private void QuitGame()
    {
        Debug.Log("게임 종료 시도");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 테스트용
#endif
    }
```
