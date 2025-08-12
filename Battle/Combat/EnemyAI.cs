using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    CardData upcoming;
    int turnCount = 1;
    bool rampageUsed = false;   // 마지막 발악 사용 여부

    void Start()
    {
        TurnManager.Instance.OnEnemyTurnStart += Act;
        CombatManager.Instance.OnCombatStart += OnCombatStart;
        CombatManager.Instance.OnEnemyHit += OnEnemyDamaged;  // 추가
    }
    
    void OnDestroy()
    {
        TurnManager.Instance.OnEnemyTurnStart -= Act;
        CombatManager.Instance.OnCombatStart -= OnCombatStart;
        CombatManager.Instance.OnEnemyHit -= OnEnemyDamaged;    
    }
    
    private void OnCombatStart()
    {
        turnCount = 1;        // 턴 카운터 리셋
        rampageUsed = false;  // 마지막 발악 사용 여부 리셋
        SetupNextSkill();     // 이제 여기서만 초기 세팅
    }
    
    private void OnEnemyDamaged()
    {
        int hp = CombatManager.Instance.enemyHp;
        // 아직 마지막 발악을 안 썼고, HP가 40 이하로 떨어졌다면
        if (!rampageUsed && hp <= 40)
        {
            rampageUsed = true;
            // “마지막 발악” 스킬 찾아서 upcoming 덮어쓰기
            var skills = DataManager.Instance.GetEnemySkills();
            var rampage = skills.FirstOrDefault(s => s.displayName == "마지막 발악");
            if (rampage != null)
            {
                upcoming = rampage;
                PreviewUI.Instance.Show(upcoming);
            }
        }
    }

    void SetupNextSkill()
    {
        var skills = DataManager.Instance.GetEnemySkills();
        int ownerID = DataManager.Instance.enemyData.ownerID;
        int enemyHP = CombatManager.Instance.enemyHp;
        int enemyMaxHP   = DataManager.Instance.enemyData.maxHP;

        switch (ownerID)
        {
            // 튜토리얼 송곳니
            case 1001 : upcoming = skills[0]; break;
            // 송곳니
            case 1004 :
                if (!rampageUsed && enemyHP <= enemyMaxHP * 0.5f)
                {
                    upcoming = skills.FirstOrDefault(s => s.displayName == "마지막 발악");
                    rampageUsed = true;
                }
                else if (rampageUsed)
                {
                    upcoming = skills.FirstOrDefault(s => s.displayName == "할퀴기");
                }
                else
                {
                    // 2턴 주기: 홀수턴엔 할퀴기, 짝수턴엔 움찔움찔
                    if (turnCount % 2 == 1)
                        upcoming = skills.FirstOrDefault(s => s.displayName == "할퀴기");
                    else
                        upcoming = skills.FirstOrDefault(s => s.displayName == "움찔움찔");

                    turnCount++;
                }
                break;
            default : upcoming = skills[Random.Range(0, skills.Length)]; break;
        }
        
        // 널 체크 후 fallback
        if (upcoming == null && skills.Length > 0)
            upcoming = skills[Random.Range(0, skills.Length)];
        
        // PreviewUI 출력
        PreviewUI.Instance.Show(upcoming);
    }   

    public void Act()  // 이걸 TurnManager.OnEnemyTurnStart 에 연결
    {
        // 적이 스턴 상태라면 스턴 애니메이션만 띄우고 턴 스킵
        if (CombatManager.Instance.enemyStunTurns > 0)
        {
            // 차후 스턴 애니메이션, 이펙트 트리거
            // CombatManager.Instance.OnEnemyStunned?.Invoke();
            return;
        }
        
        // 적 행동 발동
        CombatManager.Instance.ApplySkill(upcoming, isPlayer:false);
        
        // 바로 다음 스킬 미리보기 준비
        SetupNextSkill();
    }
}