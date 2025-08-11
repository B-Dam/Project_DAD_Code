using UnityEditor;
using UnityEngine;

public class GameExit : MonoBehaviour
{
    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // 에디터 실행 종료
#else
        Application.Quit(); // 빌드된 게임 종료
#endif
    }
}
