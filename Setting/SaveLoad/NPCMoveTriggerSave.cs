using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(UniqueID))]
public class NPCMoveTriggerSave : MonoBehaviour, ISaveable
{
    UniqueID uid;
    public string UniqueID
    {
        get
        {
            if (!uid) uid = GetComponent<UniqueID>();
            return uid ? uid.ID + ":MoveTrig" : name + ":MoveTrig"; // 같은 오브젝트의 다른 ISaveable과 충돌 방지
        }
    }

    [Serializable]
    struct Data
    {
        public bool hasMoveDisappear;
        public bool trigMoveDisappear;

        public bool hasMoveOnly;
        public bool trigMoveOnly;

        public bool hasMoveList;
        public bool trigMoveList;
    }

    // ===== 저장 =====
    public object CaptureState()
    {
        var moveDisappear = FindComp("NPCDialogueMoveAndDisappear");
        var moveOnly      = FindComp("NPCDialogueMoveOnly");
        var moveList      = FindComp("NPCDialogueMoveOnlyList");

        var data = new Data
        {
            hasMoveDisappear = moveDisappear != null,
            trigMoveDisappear = ReadTriggered(moveDisappear),

            hasMoveOnly = moveOnly != null,
            trigMoveOnly = ReadTriggered(moveOnly),

            hasMoveList = moveList != null,
            trigMoveList = ReadTriggered(moveList),
        };

        // 디버그
        Debug.Log($"[NPCMoveTriggerSave/Capture] {name} " +
                  $"Disappear=({data.hasMoveDisappear},{data.trigMoveDisappear}) " +
                  $"Only=({data.hasMoveOnly},{data.trigMoveOnly}) " +
                  $"List=({data.hasMoveList},{data.trigMoveList})");

        return data;
    }

    // ===== 불러오기 =====
    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<Data>(json);

        var moveDisappear = FindComp("NPCDialogueMoveAndDisappear");
        var moveOnly      = FindComp("NPCDialogueMoveOnly");
        var moveList      = FindComp("NPCDialogueMoveOnlyList");

        if (data.hasMoveDisappear && moveDisappear != null)
            WriteTriggered(moveDisappear, data.trigMoveDisappear);

        if (data.hasMoveOnly && moveOnly != null)
            WriteTriggered(moveOnly, data.trigMoveOnly);

        if (data.hasMoveList && moveList != null)
            WriteTriggered(moveList, data.trigMoveList);

        Debug.Log($"[NPCMoveTriggerSave/Restore] {name} 적용 완료");
    }

    // ───────── 헬퍼 ─────────
    Component FindComp(string typeName)
    {
        var t = Type.GetType(typeName) ?? AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(typeName)).FirstOrDefault(x => x != null);
        if (t == null) return null;
        return GetComponent(t); // 트리거 스크립트는 같은 오브젝트에 붙어있음(스크린샷 기준)
    }

    bool ReadTriggered(Component c)
    {
        if (!c) return false;
        var t = c.GetType();

        // public bool IsTriggered {get;}
        var prop = t.GetProperty("IsTriggered", BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.PropertyType == typeof(bool))
            return (bool)prop.GetValue(c);

        // public bool HasTriggered()/GetTriggered()
        var get = t.GetMethod("HasTriggered", BindingFlags.Public | BindingFlags.Instance)
               ?? t.GetMethod("GetTriggered", BindingFlags.Public | BindingFlags.Instance);
        if (get != null && get.ReturnType == typeof(bool))
            return (bool)get.Invoke(c, null);

        // private bool hasTriggered
        var fld = t.GetField("hasTriggered", BindingFlags.NonPublic | BindingFlags.Instance);
        if (fld != null && fld.FieldType == typeof(bool))
            return (bool)fld.GetValue(c);

        return false;
    }

    void WriteTriggered(Component c, bool v)
    {
        if (!c) return;
        var t = c.GetType();

        // public void SetTriggered(bool)
        var set = t.GetMethod("SetTriggered", BindingFlags.Public | BindingFlags.Instance);
        if (set != null && set.GetParameters().Length == 1 && set.GetParameters()[0].ParameterType == typeof(bool))
        {
            set.Invoke(c, new object[] { v });
            return;
        }

        // private bool hasTriggered
        var fld = t.GetField("hasTriggered", BindingFlags.NonPublic | BindingFlags.Instance);
        if (fld != null && fld.FieldType == typeof(bool))
            fld.SetValue(c, v);
    }
}