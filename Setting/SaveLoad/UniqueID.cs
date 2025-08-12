using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

[DisallowMultipleComponent]
public class UniqueID : MonoBehaviour
{
    [SerializeField, HideInInspector] private string id;
    public string ID => id;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        if (string.IsNullOrEmpty(id) || IsDuplicate(id))
        {
            id = System.Guid.NewGuid().ToString("N");
            EditorUtility.SetDirty(this);
        }
    }

    private bool IsDuplicate(string value)
    {
        var all = Object.FindObjectsOfType<UniqueID>(true);
        return all.Any(u => u != this && u.id == value);
    }
#endif

    private void Awake()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString("N");
    }
}