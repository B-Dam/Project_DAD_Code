using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(UniqueID))]
public class HoleSave : MonoBehaviour, ISaveable, IPostLoad
{
    [Serializable]
    private struct HoleData { public bool hasCover; }

    [Header("자식 트리거(비워두면 자동탐색)")]
    [SerializeField] private HoleTrigger targetTrigger;

    [Header("커버 프리팹 (필수)")]
    [SerializeField] private GameObject holeCoverPrefab;

    [Header("세이브 상태")]
    [SerializeField] private bool hasCover;   // 채워진 상태로 저장되었는지
    public bool HasCover => hasCover;

    // ISaveable 고유 ID
    private UniqueID idComp;
    public string UniqueID
    {
        get
        {
            if (!idComp && !TryGetComponent(out idComp))
            {
                Debug.LogError($"[HoleSave] UniqueID 누락: {name}", this);
                return null;
            }
            return idComp.ID;
        }
    }

    private void Awake()
    {
        if (!idComp) TryGetComponent(out idComp);
        if (!targetTrigger)
            targetTrigger = GetComponentInChildren<HoleTrigger>(true);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!idComp && !TryGetComponent(out idComp))
            idComp = gameObject.AddComponent<UniqueID>();

        if (!targetTrigger)
            targetTrigger = GetComponentInChildren<HoleTrigger>(true);

        if (!holeCoverPrefab)
            Debug.LogWarning($"[HoleSave] holeCoverPrefab 미할당: {name}", this);
    }
#endif

    // ---------- Save / Load ----------

    public object CaptureState()
    {
        return JsonUtility.ToJson(new HoleData { hasCover = hasCover });
    }

    public void RestoreState(object state)
    {
        var json = state as string;
        if (!string.IsNullOrEmpty(json))
        {
            var data = JsonUtility.FromJson<HoleData>(json);
            hasCover = data.hasCover;
        }
        // 실제 적용은 OnPostLoad에서
    }

    public void OnPostLoad()
    {
        if (!targetTrigger)
            targetTrigger = GetComponentInChildren<HoleTrigger>(true);

        if (targetTrigger)
        {
            // 트리거/블로커 상태 동기화
            targetTrigger.ResetRuntime(hasCover);
            // 커버 생성/삭제 보정
            EnsureCover(hasCover);
        }
        else
        {
            Debug.LogError($"[HoleSave] 자식에서 HoleTrigger를 찾지 못했습니다: {name}", this);
        }
    }

    // 런타임에서 상태 변경 시 호출(트리거에서 호출)
    public void SetHasCover(bool value)
    {
        hasCover = value;
        EnsureCover(hasCover);
    }

    // ---------- 커버 관리 ----------

    // 커버는 항상 이 부모(Hole)의 직속 자식으로 관리
    private Vector3 ResolveCoverPosition()
    {
        return targetTrigger ? targetTrigger.transform.position : transform.position;
    }

    private List<GameObject> FindExistingCovers()
    {
        var list = new List<GameObject>();
        string key = holeCoverPrefab ? holeCoverPrefab.name : "HoleCover";

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var n = child.name;
            if (n.StartsWith(key, StringComparison.OrdinalIgnoreCase) ||
                n.IndexOf("HoleCover", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                list.Add(child.gameObject);
            }
        }
        return list;
    }

    private void EnsureCover(bool needCover)
    {
        var covers = FindExistingCovers();

        if (!needCover)
        {
            // 없어야 하면 전부 제거
            for (int i = 0; i < covers.Count; i++)
                if (covers[i]) Destroy(covers[i]);
            return;
        }

        // 있어야 하면 1개 보장
        if (covers.Count == 0)
        {
            if (!holeCoverPrefab)
            {
                Debug.LogWarning($"[HoleSave] holeCoverPrefab이 비어 있어 커버를 생성할 수 없습니다: {name}", this);
                return;
            }
            var go = Instantiate(holeCoverPrefab, ResolveCoverPosition(), Quaternion.identity, transform);
            // go.name = holeCoverPrefab.name; // 원하면 이름 고정
        }
        else if (covers.Count > 1)
        {
            for (int i = 1; i < covers.Count; i++)
                if (covers[i]) Destroy(covers[i]);
        }
    }
}
