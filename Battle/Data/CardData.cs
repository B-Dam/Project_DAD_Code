using UnityEngine;

public enum CardTypePrimary { 공격, 약화, 실드, 버프, 디버프 }
public enum CardTypeSecondary { None, 공격, 약화, 실드, 버프, 디버프 }

[CreateAssetMenu(menuName = "Battle/CardData")]
public class CardData : ScriptableObject
{
    [Header("매핑된 키값")]
    public int cardId;              // 2000, 2001 등
    public string displayName;      // "물기", "으르렁거리기" 등

    [Header("소유자 ID")]
    public int ownerID;             // 1000, 1001, 1004 등

    [Header("카드 분류")]
    public CardTypePrimary typePrimary;         // "공격", "약화", "방어"
    public CardTypeSecondary typeSecondary;     // 보조 분류

    [Header("효과 수치")]
    public int effectAttackValue;           // 공격 계수
    public int effectShieldValue;           // 보호막 계수
    public int effectAttackIncreaseValue;   // 버프 계수
    public int effectAttackDebuffValue;     // 디버프 계수
    public int effectTurnValue;             // 지속 턴 수

    [TextArea(2,4), Header("효과 텍스트")]
    public string effectText;          // 효과 설명

    [Header("등급")]
    public int rank;                   // 카드의 등급 (1성=1, 2성=2)
    
    [Header("소모 AP")]
    public int costAP;                 // 기본적으로는 1만을 소모하지만 확장성을 위해 추가
    
    [Header("UI 리소스")]
    public Sprite icon;                // 스킬 아이콘
}