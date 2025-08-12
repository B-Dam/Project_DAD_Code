using System;
using UnityEngine;

[RequireComponent(typeof(UniqueID))]
public class MapManagerSave : MonoBehaviour, ISaveable
{
    [Serializable]
    struct Data
    {
        public string currentID;
        public string prevID;
    }

    UniqueID uid;
    public string UniqueID => (uid ??= GetComponent<UniqueID>()).ID;

    public object CaptureState()
    {
        var mm = MapManager.Instance;
        return new Data {
            currentID = mm != null ? mm.currentMapID : null,
            prevID    = mm != null ? mm.prevMapID    : null
        };
    }

    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<Data>(json);

        // 즉시 필드 복원
        if (MapManager.Instance != null)
        {
            MapManager.Instance.prevMapID    = data.prevID; 
            MapManager.Instance.currentMapID = data.currentID;
        }

        // 씬 콜라이더/시네머신 준비를 한 프레임 기다렸다가 컨파이너 재적용
        StartCoroutine(ApplyNextFrame(data.currentID));
    }

    System.Collections.IEnumerator ApplyNextFrame(string id)
    {
        yield return null;
        yield return null; // 씬 내 콜라이더 / 카메라 준비 대기
        var mm = MapManager.Instance;
        if (mm == null) yield break;

        // 현재 ID 기준으로 컨파이너/음악 등을 재적용
        mm.ReapplyForCurrentMap();
    }
}