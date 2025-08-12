using System;
using UnityEditor.Tilemaps;
using UnityEngine;

[Serializable]
public struct NPCData
{
    public bool isActive;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public bool[] colliderEnabledStates; // 콜라이더 활성화 상태
}

[RequireComponent(typeof(UniqueID))]
public class NPCSave : MonoBehaviour, ISaveable
{
    private UniqueID idComp;
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

    public object CaptureState()
    {
        var cols = GetComponentsInChildren<Collider2D>(true);
        Debug.Log($"[NPCSave/Capture] id={GetComponent<UniqueID>()?.ID} active={gameObject.activeSelf} cols={cols.Length}");
        var states = new bool[cols.Length];
        for (int i = 0; i < cols.Length; i++) states[i] = cols[i].enabled;

        return new NPCData
        {
            isActive = gameObject.activeInHierarchy,
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale,
            colliderEnabledStates = states
        };
    }

    public void RestoreState(object state)
    {
        var json = state as string; if (string.IsNullOrEmpty(json)) return;
        var data = JsonUtility.FromJson<NPCData>(json);
        Debug.Log($"[NPCSave/Restore] id={GetComponent<UniqueID>()?.ID} -> isActive={data.isActive}");

        if (gameObject.activeSelf != data.isActive)
            gameObject.SetActive(data.isActive);

        transform.position   = data.position;
        transform.rotation   = data.rotation;
        transform.localScale = data.scale;
        
        var cols = GetComponentsInChildren<Collider2D>(true);
        if (data.colliderEnabledStates != null && data.colliderEnabledStates.Length == cols.Length)
        {
            for (int i = 0; i < cols.Length; i++) cols[i].enabled = data.colliderEnabledStates[i];
        }
        else
        {
            // 기본적으로 콜라이더 활성화 (상호작용 가능 보장)
            foreach (var c in cols) c.enabled = true;
        }
    }
}