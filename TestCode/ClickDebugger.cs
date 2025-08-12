using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickDebugger : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[ClickDebugger] {name} 클릭됨 at {eventData.position}");
    }
    
    void Start()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(() => Debug.Log("[ButtonClickTester] onClick 리스너 호출!"));
    }
}