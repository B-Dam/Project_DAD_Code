using System;
using UnityEngine;

[RequireComponent(typeof(UniqueID))]
[RequireComponent(typeof(CombatTriggerEvent))]
public class CombatTriggerSave : MonoBehaviour, ISaveable
{
    UniqueID uid;
    public string UniqueID => (uid ??= GetComponent<UniqueID>()).ID;

    [Serializable]
    struct Data { public bool triggered; }

    public object CaptureState()
    {
        var trig = GetComponent<CombatTriggerEvent>();
        return new Data { triggered = trig.IsTriggered };
    }

    public void RestoreState(object state)
    {
        var json = state as string; if (string.IsNullOrEmpty(json)) return;
        var data = JsonUtility.FromJson<Data>(json);
        GetComponent<CombatTriggerEvent>()?.SetTriggered(data.triggered);
    }
}