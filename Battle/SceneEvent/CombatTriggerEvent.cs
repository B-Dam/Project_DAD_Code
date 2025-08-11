using System.Collections;
using Unity.Multiplayer.Center.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatTriggerEvent : MonoBehaviour
{
    public static CombatTriggerEvent Instance;

    [Tooltip("이 전투가 튜토리얼인지")]
    public bool isTutorialTrigger;
    
    [Header("전투 정보")]
    public CombatSetupData setupData;

    [Header("메인 UI 루트 (씬 전환 시 숨길 것)")]
    public GameObject mainUI;

    private Scene _previousScene;

    bool hasTriggered;
    public bool IsTriggered => hasTriggered;
    public void SetTriggered(bool v) => hasTriggered = v;

    private void Awake()
    {
        // 트리거 오브젝트가 Battle 씬 로딩/언로드 때 파괴되지 않도록
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerCombat()
    {
        if (SaveLoadManagerCore.IsRestoring) return;
        if (hasTriggered) return; // 이미 실행됐으면 무시
        hasTriggered = true;
    // 대화 UI 숨기되 데이터는 유지하되 스프라이트는 꺼버림
    if (DialogueManager.Instance != null)
    {
        DialogueUIDisplayer.Instance.HideDialogueSprites(); // 새 메서드 추가
        DialogueManager.Instance.EndDialogue(clearState: false);
        DialogueUIDisplayer.Instance.ClearUI();
        QuestGuideUI.Instance.questUI.SetActive(false);

        }

        // UI 숨기기
        if (mainUI != null) mainUI.SetActive(false);

        if (InteractHintController.Instance != null)
            InteractHintController.Instance.DisableHint();

        // 이전 씬 저장
        _previousScene = SceneManager.GetActiveScene();

        // 전투 데이터 전달
        CombatDataHolder.SetData(setupData);
        CombatDataHolder.LastTrigger = this;

        // 전투 씬 로딩
        SceneManager.LoadSceneAsync("Battle", LoadSceneMode.Additive)
                    .completed += _ =>
        {
            var battle = SceneManager.GetSceneByName("Battle");
            if (battle.IsValid())
                SceneManager.SetActiveScene(battle);
            CombatManager.Instance.IsInCombat = true;
        };
        return;
    }

    // 전투가 끝났을 때 호출
    public void OnBattleEnd()
    {
        StartCoroutine(UnloadBattleSceneRoutine());
        AudioManager.Instance.PlayBGM("MapBGM");
    }

    private IEnumerator UnloadBattleSceneRoutine()
    {
        // Battle 씬을 비동기로 언로드
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync("Battle");

        // 언로드가 완료될 때까지 대기
        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        // 이전 씬을 다시 활성 씬으로 설정
        if (_previousScene.IsValid())
        {
            SceneManager.SetActiveScene(_previousScene);
        }

        // 메인 UI 재활성화
        if (mainUI != null) mainUI.SetActive(true);

        // 대화 재개 (안전하게 호출)
        DialogueManager.Instance?.ResumeDialogue();

        // 모든 작업이 끝난 후 데이터 정리
        CombatDataHolder.Clear();

        // 진짜 다 끝나고 트리거 초기화
        CombatDataHolder.ClearTrigger();
    }
}