using UnityEngine;

public class BattleStartButton : MonoBehaviour
{
    [Header("버튼 클릭시 숨길 패널")]
    public GameObject startPanel;
    
    public void HideStartPanel()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }
    }
}