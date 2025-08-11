using System;
using UnityEngine;

[Serializable]
public struct QuestItemData
{
    public bool isActive;
    public Vector3 position;
}

[RequireComponent(typeof(UniqueID))]
public class QuestItemSave : MonoBehaviour, ISaveable
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

    public object CaptureState() => new QuestItemData
    {
        isActive = gameObject.activeSelf,
        position = transform.position
    };

    public void RestoreState(object state)
    {
        var json = state as string; if (string.IsNullOrEmpty(json)) return;
        var data = JsonUtility.FromJson<QuestItemData>(json);

        if (gameObject.activeSelf != data.isActive)
            gameObject.SetActive(data.isActive);

        transform.position = data.position;
    }
}