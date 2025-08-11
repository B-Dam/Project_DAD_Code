using UnityEngine;

[CreateAssetMenu(menuName = "Battle/CharacterData")]
public class CharacterDataSO : ScriptableObject
{
    [Header("식별 정보")]
    public string characterId;    // 예: "Mono", "CatBoss"
    public string displayName;    // 에디터에서 한글명 표시용

    [Header("소유자 ID")]
    public int ownerID;

    [Header("능력치")]
    public int maxHP;
    public int atk;
    public int def;
}