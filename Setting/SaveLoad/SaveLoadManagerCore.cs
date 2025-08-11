using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class SaveEntry { public string id; public string json; }

[Serializable]
public class SaveWrapper
{
    public int version = 1;
    public SaveEntry[] entries;
    // tombstone(파괴된 ID) 등 추가하고 싶으면 여기에
}

public class SaveLoadManagerCore : MonoBehaviour
{
    public static SaveLoadManagerCore Instance { get; private set; }
    
    // 복구 중인제 확인 하는 불 값
    public static bool IsRestoring { get; private set; }

    const string FILE_PATTERN = "slot_{0}.json";

    Dictionary<string, ISaveable> saveables = new();
    readonly Dictionary<string, string> pending = new(); // 아직 씬에 없는 ID

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void RegisterSaveables()
    {
        saveables.Clear();

        var all = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
        var byId = new Dictionary<string, List<ISaveable>>();

        foreach (var sv in all)
        {
            string id = sv.UniqueID;
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[SaveCore] 빈 ID: {((MonoBehaviour)sv).name}");
                continue;
            }
            if (!byId.TryGetValue(id, out var list))
                byId[id] = list = new List<ISaveable>();
            list.Add(sv);
        }

        int dup = 0;
        foreach (var kv in byId)
        {
            var list = kv.Value;
            ISaveable chosen;
            if (list.Count == 1)
            {
                chosen = list[0];
            }
            else
            {
                dup++;
                // 활성 오브젝트 우선, 그 다음 이름/경로로 안정화
                chosen = list
                         .OrderByDescending(sv => ((MonoBehaviour)sv).gameObject.activeInHierarchy)
                         .ThenBy(sv => ((MonoBehaviour)sv).gameObject.scene.name)
                         .ThenBy(sv => ((MonoBehaviour)sv).name)
                         .First();

                var info = string.Join(" | ", list.Select(sv =>
                {
                    var mb = (MonoBehaviour)sv;
                    return $"{mb.name}(activeInHierarchy={mb.gameObject.activeInHierarchy})";
                }));
                Debug.LogWarning($"[SaveCore] 중복 ID 해결: {kv.Key} -> {((MonoBehaviour)chosen).name} 선택 | 후보: {info}");
            }

            saveables[kv.Key] = chosen;
        }

        Debug.Log($"[SaveCore] ISaveable 등록 완료: {saveables.Count}개 (중복 해결 {dup}개)");
    }

    public void SaveGame(int slotIndex)
    {
        RegisterSaveables();

        var entries = saveables.Select(kv => new SaveEntry {
            id   = kv.Key,
            json = JsonUtility.ToJson(kv.Value.CaptureState())
        }).ToArray();

        var wrapper = new SaveWrapper { version = 1, entries = entries };
        var json = JsonUtility.ToJson(wrapper, true);

        var path = Path.Combine(Application.persistentDataPath, string.Format(FILE_PATTERN, slotIndex));
        File.WriteAllText(path, json);
        Debug.Log($"[SaveCore] 슬롯 {slotIndex} 저장 → {path}");
    }

    public void LoadGame(int slotIndex)
    {
        StartCoroutine(LoadRoutine(slotIndex));
    }

    IEnumerator LoadRoutine(int slotIndex)
    {
        IsRestoring = true;
        var path = Path.Combine(Application.persistentDataPath, string.Format(FILE_PATTERN, slotIndex));
        if (!File.Exists(path)) { Debug.LogWarning($"[SaveCore] 파일 없음: {path}"); IsRestoring = false; yield break; }

        var wrapper = JsonUtility.FromJson<SaveWrapper>(File.ReadAllText(path));

        // 한 프레임 대기: 씬의 Awake/Start/OnEnable 완료 보장
        yield return null;

        RegisterSaveables();
        pending.Clear();

        // 1패스: 환경 우선(Hole/Trigger/Quest/Dialogue 등)
        int applied1 = 0, applied2 = 0;
        foreach (var e in wrapper.entries)
        {
            if (TryGet(e.id, out var sv) && IsEnvironment(sv))
            {
                sv.RestoreState(e.json);
                applied1++;
            }
        }

        // 2패스: 동적(박스/NPC/플레이어/카메라 등)
        foreach (var e in wrapper.entries)
        {
            if (TryGet(e.id, out var sv) && !IsEnvironment(sv))
            {
                sv.RestoreState(e.json);
                applied2++;
            }
            else if (!saveables.ContainsKey(e.id))
            {
                pending[e.id] = e.json; // 나중에 등장하면 적용
            }
        }
        Debug.Log($"[SaveCore] Restore 적용: env={applied1}, dynamic={applied2}, pending={pending.Count}");

        // 다음 프레임에 PostLoad 훅 호출(겹침 정리 등)
        yield return null;
        foreach (var mb in FindObjectsOfType<MonoBehaviour>(true))
            if (mb is IPostLoad post) post.OnPostLoad();
        
        // 정착 재시도 루프: 늦게 등장한 트리거까지 적용
        int frames = 0;
        int stable = 0;
        const int MAX_FRAMES = 20;     // 필요에 맞게 조절 (보통 5~10로도 충분)
        while (pending.Count > 0 && frames++ < MAX_FRAMES && stable < 2)
        {
            int before = pending.Count;

            // 1프레임 쉬면서 생성/활성/OnEnable 완료 대기
            yield return null;

            // 씬에 새로 생긴 저장대상 다시 등록(+비활성 포함)
            RegisterSaveables();
            TryApplyPending();

            // 진행 상황 로깅
            Debug.Log($"[SaveCore] settle {frames}: pending {before} -> {pending.Count}");

            // 더 이상 줄지 않으면 안정화 카운트 증가
            stable = (pending.Count == before) ? (stable + 1) : 0;
        }

        if (pending.Count > 0)
        {
            Debug.LogWarning($"[SaveCore] 일부 항목 미적용(pending={pending.Count}). 다음 프레임에 재시도 예정.");
        }
        
        yield return null;
        
        var updater = FindObjectOfType<CameraConfinerUpdater>(true);
        if (updater != null)
        {
            // MapManager에 현재 맵 ID가 있다면
            updater.RefreshById(MapManager.Instance.currentMapID);
        }
        
        IsRestoring = false;
    }

    bool TryGet(string id, out ISaveable sv) => saveables.TryGetValue(id, out sv);

    void TryApplyPending()
    {
        var keys = pending.Keys.ToList();
        int applied = 0;
        foreach (var k in keys)
        {
            if (saveables.TryGetValue(k, out var sv))
            {
                sv.RestoreState(pending[k]);
                pending.Remove(k);
                applied++;
            }
        }
        if (applied > 0) Debug.Log($"[SaveCore] pending 적용: {applied}, 남음={pending.Count}");
    }
    
    public bool HasSaveFile(int slotIndex)
    {
        var path = System.IO.Path.Combine(
            Application.persistentDataPath,
            string.Format(FILE_PATTERN, slotIndex));
        return System.IO.File.Exists(path);
    }

    // 타입으로 간단 분류: 먼저 복원돼야 안정적인 애들
    bool IsEnvironment(ISaveable sv) =>
        sv is HoleSave || sv is MapTriggerSave || sv is QuestItemSave || sv is DialogueSave || sv is MapManagerSave ||
        sv is CameraSave || sv is NPCMoveTriggerSave || sv is CombatTriggerSave ;
}