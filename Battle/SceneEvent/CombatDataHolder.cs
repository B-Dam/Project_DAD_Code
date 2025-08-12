using UnityEngine;

// 전투 세팅 데이터 저장
public static class CombatDataHolder
{
    private static CombatSetupData currentData;
    
    public static CombatTriggerEvent LastTrigger { get; set; }
    public static bool IsRetry = false;
    
    public static void SetData(CombatSetupData data)
    {
        currentData = data;
    }

    public static CombatSetupData GetData()
    {
        return currentData;
    }

    public static void Clear()
    {
        currentData = null;
    }
    
    public static void ClearTrigger() => LastTrigger = null;
}