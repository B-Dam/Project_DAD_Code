using System;
using UnityEngine;

[Serializable]
public struct TriggerData
{
    public bool isActive;
    public bool triggered;
}

[RequireComponent(typeof(UniqueID))]
public class MapTriggerSave : MonoBehaviour, ISaveable
{
    private UniqueID idComp;
    
    [SerializeField] EventTriggerZone zone;     // 같은 오브젝트에 붙은 존 참조
    
    public string UniqueID
    {
        get
        {
            if (idComp == null)
            {
                idComp = GetComponent<UniqueID>();
                if (idComp == null) Debug.LogError($"[Save] UniqueID 누락: {name}", this);
            }
            return idComp.ID;
        }
    }

    public object CaptureState() => new TriggerData
    {
        isActive = gameObject.activeSelf,
        triggered = zone != null && zone.HasTriggered(), // 추가 저장
    };

    public void RestoreState(object state)
    {
        var json = state as string; if (string.IsNullOrEmpty(json)) return;
        var data = JsonUtility.FromJson<TriggerData>(json);

        if (gameObject.activeSelf != data.isActive)
            gameObject.SetActive(data.isActive);
        
        // 저장 당시의 '이미 발동됨' 상태 복원
        if (zone != null)
            zone.SetTriggered(data.triggered);
    }
}