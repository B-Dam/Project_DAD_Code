using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/EnvironmentEffect")]
public class EnvironmentEffect : ScriptableObject
{
    public string effectId; // 효과 ID (wind, rain, fog..)
    public string title; // UI에 띄울 이름
    [TextArea] public string description; // UI에 띄울 설명

    [Header("전투 매개변수")]
    public int apBonus = 0; // 턴당 행동력 보너스
    public float attackMultiplier = 1f; // 공격 배율
    public float shieldMultiplier = 1f; // 보호막 배율
    public float debuffMultiplier = 1f; // 약화 카드 효과 배율

    [Header("UI용 아이콘")] 
    public Sprite icon; // 각 환경 아이콘
    
    [Header("이 환경을 사용할 수 있는 보스 ID")]
    public List<string> applicableBossIds; 
}