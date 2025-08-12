using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PreviewUI : MonoBehaviour
{
    public static PreviewUI Instance { get; private set; }
    
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;

    private CardData currentSkill;
    public CardData CurrentSkill => currentSkill;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(CardData skill)
    {
        currentSkill = skill;
        iconImage.sprite = skill.icon;
        nameText.text    = skill.displayName;
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}