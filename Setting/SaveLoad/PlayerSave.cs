using System;
using UnityEngine;

[Serializable]
public struct PlayerData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

[RequireComponent(typeof(UniqueID))]
public class PlayerSave : MonoBehaviour, ISaveable
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

    public object CaptureState() => new PlayerData
    {
        position = transform.position,
        rotation = transform.rotation,
        scale    = transform.localScale
    };

    public void RestoreState(object state)
    {
        var json = state as string; if (string.IsNullOrEmpty(json)) return;
        var data = JsonUtility.FromJson<PlayerData>(json);

        transform.position   = data.position;
        transform.rotation   = data.rotation;
        transform.localScale = data.scale;
    }
}