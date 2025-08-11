using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 저장 포맷
[Serializable]
public struct DialogueSaveData
{
    public bool   isActive;       // 대화창이 켜진 상태였는지
    public int    currentIndex;   // 현재 인덱스
    public string[] ids;          // 진행 중이던 ID 시퀀스 전체
    public string[] seenIds;      // 세션 내 '이미 본' ID들
}

[RequireComponent(typeof(UniqueID))]
public class DialogueSave : MonoBehaviour, ISaveable
{
    private UniqueID idComp;

    public string UniqueID
    {
        get
        {
            if (!idComp)
            {
                idComp = GetComponent<UniqueID>();
                if (!idComp) Debug.LogError($"[Save] UniqueID 누락: {name}", this);
            }
            return idComp.ID;
        }
    }

    void Awake()
    {
        idComp = GetComponent<UniqueID>();
    }

    // === 저장 ===
    public object CaptureState()
    {
        var dm = DialogueManager.Instance;
        var data = new DialogueSaveData
        {
            isActive     = dm != null && dm.IsDialogueActive,
            currentIndex = 0,
            ids          = Array.Empty<string>(),
            seenIds      = Array.Empty<string>()
        };
        
        // 세션 없어도 누적 seen(캐시 포함) 저장
        if (dm != null)
            data.seenIds = dm.GetAllSeenIDs();

        if (dm != null && dm.Session != null)
        {
            // 세션 전체 ID 목록 수집
            var ids = new List<string>();
            int i = 0;
            while (dm.Session.HasIndex(i)) // 세션에 인덱스가 존재하는 동안
            {
                ids.Add(dm.Session.GetID(i));
                i++;
            }

            data.ids          = ids.ToArray();
            data.currentIndex = dm.Session.CurrentIndex;
        }
        return data;
    }

    // === 불러오기 ===
    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<DialogueSaveData>(json);
        
        StartCoroutine(ApplyNextFrame(data)); // UI/싱글톤 준비 이후에 적용
    }

    private IEnumerator ApplyNextFrame(DialogueSaveData data)
    {
        // 한 프레임 미루기: DialogueUIDisplayer/DB/컷신 컨트롤러 등 초기화 완료 보장
        yield return null;

        var dm = DialogueManager.Instance;
        var db = DialogueDatabase.Instance;

        if (dm == null || db == null)
        {
            Debug.LogWarning("[DialogueSave] DialogueManager/Database 준비 안 됨. 적용 보류.");
            yield break;
        }

        // 대화창 꺼진 상태 저장본이면 seen만 복원하고 UI 갱신 후 종료
        if (!data.isActive || data.ids == null || data.ids.Length == 0)
        {
            dm.LoadSeenIDs(data.seenIds, true); // 덮어쓰기
            if (QuestGuideUI.Instance != null) //  로드시 퀘스트 텍스트 갱신
                QuestGuideUI.Instance.RefreshQuest();
            yield break;
        }

        // 저장해둔 ID 시퀀스로 라인 재구성
        var lines = new DialogueDatabase.DialogueLine[data.ids.Length];
        for (int i = 0; i < data.ids.Length; i++)
            lines[i] = db.GetLineById(data.ids[i]);

        // 새로운 세션 생성 후 시작 (index=0에서 표시는 되지만, 곧 점프함)
        var newSession = new DialogueSession(lines, data.ids, dm.SharedSeen);
        dm.StartDialogue(newSession);
        
        // 세션이 생긴 상태에서 한 번만 덮어쓰기 로드
        dm.LoadSeenIDs(data.seenIds, true);

        // 저장해 둔 인덱스로 점프 (이벤트/컷신 발동 없이 세션 인덱스만 이동)
        int cur = dm.Session.CurrentIndex;
        while (cur < data.currentIndex && dm.Session.HasIndex(cur + 1))
        {
            // Display/ShowNextLine을 호출하지 않고, 세션 인덱스만 올림
            dm.Session.MoveNext();
            cur++;
        }

        // 점프한 위치의 라인을 다시 그리기, DisplayCurrentLine에서 QuestUI도 갱신
        dm.DisplayCurrentLine();
        dm.UnlockInput();
    }
    
    // ▼ 디버그 전용 필드
    [Header("DEBUG · Dialogue SeenIDs (runtime)")]
    [SerializeField] bool debugAutoRefresh = true;   // 실시간 갱신 on/off
    [SerializeField] float debugRefreshInterval = 0.5f;

    [SerializeField] int debugSeenCount;             // 본 ID 개수
    [SerializeField] string[] debugSeenIDs;          // 본 ID 리스트
    [TextArea(2, 6)] [SerializeField] string debugSeenPreview; // 미리보기(콤마로 합침)

    [SerializeField] string debugCurrentID;          // 현재 라인 ID
    [SerializeField] int debugCurrentIndex = -1;     // 현재 인덱스

    float _debugNextAt; // 내부 타이머
    
    // ▼ 인스펙터 갱신용: 수동 갱신 버튼
    [ContextMenu("DEBUG · Refresh SeenIDs Now")]
    void DebugRefreshNow()
    {
        var dm = DialogueManager.Instance;
        if (dm == null)
        {
            debugSeenIDs = System.Array.Empty<string>();
            debugSeenCount = 0;
            debugSeenPreview = "(DialogueManager 없음)";
            debugCurrentIndex = -1;
            debugCurrentID = string.Empty;
            return;
        }

        // 세션의 '이미 본' ID 전부 가져오기
        var arr = dm.GetAllSeenIDs();     // ← 실제로 존재 (DialogueManager.cs)
        debugSeenIDs = arr;
        debugSeenCount = arr?.Length ?? 0;
        debugSeenPreview = (arr == null || arr.Length == 0) ? "(none)" : string.Join(", ", arr);

        // 현재 라인/인덱스도 확인
        if (dm.Session != null && dm.Session.HasIndex(dm.Session.CurrentIndex))
        {
            debugCurrentIndex = dm.Session.CurrentIndex;
            debugCurrentID = dm.Session.GetID(debugCurrentIndex);
        }
        else
        {
            debugCurrentIndex = -1;
            debugCurrentID = string.Empty;
        }
    }
    
    // ▼ 런타임 자동 갱신 (OnEnable/OnDisable 또는 Update)
    void Update()
    {
        if (!Application.isPlaying) return;
        if (!debugAutoRefresh) return;

        if (Time.unscaledTime >= _debugNextAt)
        {
            DebugRefreshNow();
            _debugNextAt = Time.unscaledTime + Mathf.Max(0.1f, debugRefreshInterval);
        }
    }
}
