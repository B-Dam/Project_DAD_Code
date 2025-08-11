using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("📍 버튼들")]
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;

    private void Start()
    {
        // 버튼 연결
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void StartGame()
    {
        SceneManager.LoadScene("Marge5"); // 실제 게임 씬 이름
    }

   

    private void QuitGame()
    {
        Debug.Log("게임 종료 시도");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 테스트용
#endif
    }
}