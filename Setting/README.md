# ⚙️ Setting — 시스템/메뉴

시스템/메뉴 모듈 설명입니다.

---

## ✨ 설계 특징 (Highlights)
- (추가 예정)

---

## 🔁 핵심 흐름
Open → Execute

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `Mainmenumanager.cs`
```csharp
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
