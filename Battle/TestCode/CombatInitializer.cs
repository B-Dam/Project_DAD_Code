using System.Collections;
using UnityEngine;

public class CombatInitializer : MonoBehaviour
{
    public void Start()
    {
        CombatManager.Instance.StartCombat();
    }
}