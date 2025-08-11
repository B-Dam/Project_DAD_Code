using System;
using UnityEngine;

public enum characterType
{
    player,
    enemy,
    npc
}

[Serializable]
public class CharacterData
{
    public int id;
    public string displayName;
    public characterType type;
    public int health;
    public int attack;
    public int defense;
    public int relatedMap;
    public int appearanceCondition;

    public CharacterData(string[] f)
    {
        id                  = f[0] == "" ? 0 : int.Parse(f[0]);
        displayName         = f[1];
        type                = (characterType)Enum.Parse(typeof(characterType), f[2]);
        health              = f[3] == "" ? 0 : int.Parse(f[3]);
        attack              = f[4] == "" ? 0 : int.Parse(f[4]);
        defense             = f[5] == "" ? 0 : int.Parse(f[5]);
        relatedMap          = f[6] == "" ? 0 : int.Parse(f[6]);
        appearanceCondition = f[7] == "" ? 0 : int.Parse(f[7]);
    }
}