using System;

[Serializable]
public class QuestData
{
    public int questId;
    public string questName;
    public int conditionStart;
    public int conditionComplete;
    public string chapterName;
    
    public QuestData(string[] f)
    {
        questId            = int.Parse(f[0]);
        questName          = f[1];
        conditionStart     = int.Parse(f[2]);
        conditionComplete  = int.Parse(f[3]);
        chapterName        = f[4];
    }
}