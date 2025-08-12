using System;

[Serializable]
public class DialogueData
{
    public string dialogueId;
    public int speakerId;
    public string dialogueText;

    public DialogueData(string[] f)
    {
        dialogueId      = f[0];
        speakerId       = int.Parse(f[1]);
        dialogueText    = f[2];
    }
}