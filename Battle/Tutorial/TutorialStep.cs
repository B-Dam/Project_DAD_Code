using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TutorialStep
{
    public string instruction;              // 말풍선에 띄울 텍스트
    public RectTransform highlightTarget;   // 강조할 UI (카드 슬롯, AP 아이콘 등)
    public UnityEvent onStepStart;          // 이 단계 시작 시 실행할 로직
    public UnityEvent onStepComplete;       // 완료 조건이 만족되면 Invoke
    
    [Header("스페이스/클릭으로 넘어가기 허용")]
    public bool allowSkipWithInput = true;
    
    [Header("메시지 박스 위치")] 
    public Vector2 messageBoxAnchoredPos;  // Canvas 상의 좌표 (anchoredPosition)
}