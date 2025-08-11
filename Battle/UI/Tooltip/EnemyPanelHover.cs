using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyPanelHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipController.Instance.ShowCurrentSkill();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.Hide();
    }
}