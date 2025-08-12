using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardListItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;

    public void SetData(CardData data)
    {
        iconImage.sprite = data.icon;
        nameText.text = data.displayName;
        descText.text    = TextFormatter.Format(
            data.effectText,
            new System.Collections.Generic.Dictionary<string,string> {
                { "damage", (CombatManager.Instance.PlayerBaseAtk + data.effectAttackValue + CombatManager.Instance.playerAtkMod).ToString() },
                { "turns", data.effectTurnValue.ToString() },
                { "shield", data.effectShieldValue.ToString() },
                { "debuff", data.effectAttackDebuffValue.ToString() },
                { "buff", data.effectAttackIncreaseValue.ToString() }
            }
        );
    }
}