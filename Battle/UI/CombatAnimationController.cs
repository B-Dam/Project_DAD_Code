using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CombatAnimationController : MonoBehaviour
{
    [Header("애니메이터")]
    public Animator playerAnimator;
    public Animator enemyAnimator;
    
    [Header("일반 공격 시 움직임")]
    public GameObject playerCharacter;
    public GameObject enemyCharacter;
    public float attackMoveDistance = 100f;    // 얼마나 이동할지
    public float attackMoveDuration = 0.2f;  // 얼마나 빠르게
    
    [Header("공격 모션 딜레이")]
    [SerializeField] private float attackPause = 1f;  
    
    [Header("UI 제어용")]
    [SerializeField] private CanvasGroup gameUIGroup;
    [SerializeField] private GameObject retryPanel;
    
    [Header("스킬 이펙트(prefab)")]
    [SerializeField] private GameObject biteEffect;    // 물기
    [SerializeField] private GameObject barkEffect;    // 으르렁거리기
    [SerializeField] private GameObject defEffect;     // 웅크리기
    [SerializeField] private GameObject scratchEffect; // 할퀴기

    [Header("필살기 이펙트(prefab)")]
    [SerializeField] private GameObject[] specialEffects; // [0] : 물기강화, [1] : 방패강화, [2] : 기절강화

    [Header("버프/디버프 이펙트(prefab)")]
    [SerializeField] private GameObject buffEffect;
    [SerializeField] private GameObject debuffEffect;
    
    [Header("이펙트 소환 위치 조율용 오프셋")]
    [SerializeField] private Vector3 effectOffset = new Vector3(0f, 0f, 0f);
    
    private bool isEnemyMoving;
    private bool isPlayerMoving;
    
    private Coroutine playerMoveCoroutine;
    private Coroutine enemyMoveCoroutine;
    
    const string ENRAGE_FLAG = "rampageUsed";

    private void Start()
    {
        var cm = CombatManager.Instance;
        if (cm == null) Debug.LogError("CombatAnimationController: CombatManager.Instance is null");
        else
        {
            cm.OnPlayerSkillUsed     += HandlePlayerSkill;
            cm.OnEnemySkillUsed      += HandleEnemySkill;
            cm.OnPlayerHit           += HandlePlayerHit;
            cm.OnEnemyHit            += HandleEnemyHit;
            cm.OnPlayerDeath         += HandlePlayerDeath;
            cm.OnEnemyDeath          += HandleEnemyDeath;
            cm.OnCombatStart         += HandleCombatStart;
            cm.OnStatusEffectApplied += HandleStatusEffect;
            cm.OnSpecialUsed         += HandleSpecialEffect;
            cm.OnEnemyStuned         += HandleEnemyStun;
            cm.OnEnemyStunClean      += HandleStunClean;
        }
    }

    private void OnDestroy()
    {
        var cm = CombatManager.Instance;
        if (cm != null)
        {
            cm.OnPlayerSkillUsed     -= HandlePlayerSkill;
            cm.OnEnemySkillUsed      -= HandleEnemySkill;
            cm.OnPlayerHit           -= HandlePlayerHit;
            cm.OnEnemyHit            -= HandleEnemyHit;
            cm.OnPlayerDeath         -= HandlePlayerDeath;
            cm.OnEnemyDeath          -= HandleEnemyDeath;
            cm.OnCombatStart         -= HandleCombatStart;
            cm.OnStatusEffectApplied -= HandleStatusEffect;
            cm.OnSpecialUsed         -= HandleSpecialEffect;
            cm.OnEnemyStuned         -= HandleEnemyStun;
            cm.OnEnemyStunClean      -= HandleStunClean;
        }
    }

    // 플레이어 스킬 사용 시 애니메이션 트리거
    private void HandlePlayerSkill(CardData data)
    {
        switch (data.displayName)  // 또는 data.id, data.skillName 등에 맞게
        {
            case "물기":
                playerAnimator.SetTrigger("Attack");
                StartCoroutine(HandlePlayerAttack());
                SpawnEffect(biteEffect, enemyCharacter.transform.position + Vector3.left * 1.4f);;
                AudioManager.Instance.PlaySFX("Battle/Attack");
                break;
            
            case "으르렁거리기":
                playerAnimator.SetTrigger("Bark");
                SpawnEffect(barkEffect, playerCharacter.transform.position + Vector3.right * 0.275f + Vector3.up * 1.3f );
                AudioManager.Instance.PlaySFX("Battle/DogGrowling");

                break;
            case "웅크리기":
                playerAnimator.SetTrigger("Def");
                SpawnEffect(defEffect, playerCharacter.transform.position + Vector3.right * 0.1f + Vector3.up * 0.25f);;
                AudioManager.Instance.PlaySFX("Battle/Barrier");
                break;
            default:
                playerAnimator.SetTrigger("Attack");
                break;
        }
    }
    
    // 플레이어 공격 시 이동 로직
    private IEnumerator PlayerDoAttackStep(Transform tf)
    {
        if (isPlayerMoving) yield break;
        isPlayerMoving = true;
        
        Vector3 startPos = tf.localPosition;
        Vector3 midPos   = startPos + Vector3.right * attackMoveDistance;
        float   halfDur  = attackMoveDuration * 0.5f;
        float   elapsed  = 0f;

        // 앞쪽으로 이동
        while (elapsed < halfDur)
        {
            tf.localPosition = Vector3.Lerp(startPos, midPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.localPosition = midPos;

        yield return new WaitForSeconds(attackPause);

        // 뒤로 돌아오기
        elapsed = 0f;
        while (elapsed < halfDur)
        {
            tf.localPosition = Vector3.Lerp(midPos, startPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.localPosition = startPos;
        
        isPlayerMoving = false;
    }
    
   // 적 스킬 사용시 애니메이션 트리거
    private void HandleEnemySkill(CardData data)
    {
        switch(data.ownerID)
        {
            case 1001 :
            case 1004 :
                switch (data.displayName)
                {
                    case "할퀴기":
                        enemyAnimator.SetTrigger("Attack");
                        SpawnEffect(scratchEffect, playerCharacter.transform.position);
                        AudioManager.Instance.PlaySFX("Battle/Attack");
                        if (!isEnemyMoving)
                            enemyMoveCoroutine = StartCoroutine(EnemyDoAttackStep(enemyCharacter.transform));
                        break;
                    case "움찔움찔":
                        enemyAnimator.SetTrigger("Twitch"); break;
                    case "마지막 발악":
                        enemyAnimator.SetTrigger("Enrage");
                        enemyAnimator.SetBool(ENRAGE_FLAG, true);
                        AudioManager.Instance.PlaySFX("Battle/Awaken");
                        break;
                    default:
                        break;
                }
                break;
        };
    }
    
    // 적 공격시 이동 로직
    private IEnumerator EnemyDoAttackStep(Transform tf)
    {
        if (isEnemyMoving) yield break;
        isEnemyMoving = true;
        
        Vector3 startPos = tf.localPosition;
        Vector3 midPos   = startPos + Vector3.left * attackMoveDistance;
        float   halfDur  = attackMoveDuration * 0.5f;
        float   elapsed  = 0f;

        // 앞쪽으로 이동
        while (elapsed < halfDur)
        {
            tf.localPosition = Vector3.Lerp(startPos, midPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.localPosition = midPos;

        yield return new WaitForSeconds(attackPause);

        // 뒤로 돌아오기
        elapsed = 0f;
        while (elapsed < halfDur)
        {
            tf.localPosition = Vector3.Lerp(midPos, startPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.localPosition = startPos;

        isEnemyMoving = false;
    }
    
    // 플레이어 피격 트리거
    private void HandlePlayerHit()
    {
        playerAnimator.SetTrigger("Hit");
    }

    // 적 피격 트리거
    private void HandleEnemyHit()
    {
        enemyAnimator.SetTrigger("Hit");
    }
    
    // 적 기절 트리거
    public void HandleEnemyStun()
    {
        enemyAnimator.SetBool("isStunned", true);
        enemyAnimator.SetTrigger("Stun");
    }

    // 기절 해제
    public void HandleStunClean()
    {
        enemyAnimator.SetBool("isStunned", false);
        enemyAnimator.SetTrigger("StunClean");
    }
    
    // 플레이어 사망 로직
    private void HandlePlayerDeath()
    {
        // GameUI 잠금
        LockGameUI();
        // 남아 있는 Hit 트리거 클리어
        playerAnimator.ResetTrigger("Hit");
        // Die 트리거 활성
        playerAnimator.SetTrigger("Die");
        StartCoroutine(ShowRetryAfterDelay());
    }
    
    // UI 잠금 메서드
    private void LockGameUI()
    {
        // 모든 버튼/슬롯을 비활성화
        gameUIGroup.interactable     = false;
        gameUIGroup.blocksRaycasts   = false;
    }
    
    // 재시작시 애니메이션 트리거
    private void HandleCombatStart()
    {
        enemyAnimator.SetBool(ENRAGE_FLAG, false);
        
        if (CombatDataHolder.IsRetry)
        {
            playerAnimator.SetTrigger("Retry");
            enemyAnimator.SetTrigger("Retry");
            // 한 번만 실행되도록 플래그 리셋
            CombatDataHolder.IsRetry = false;
        }
    }
    
    // 플레이어 사망 이후 재시작 패널 활성화
    private IEnumerator ShowRetryAfterDelay()
    {
        // 3초 이후 재시작 패널 활성화
        yield return new WaitForSeconds(3f);
        retryPanel.SetActive(true);
        Time.timeScale = 0;
        CombatManager.Instance.IsInCombat = false;
    }
    
    // 플레이어 연속 공격시 이동 멈춤 방지용 로직
    private IEnumerator HandlePlayerAttack()
    {
        if (isPlayerMoving)
        {
            // 이전 이동이 끝날 때까지 잠깐 대기
            while (isPlayerMoving)
                yield return null;
        }

        // 이동 시작
        yield return StartCoroutine(PlayerDoAttackStep(playerCharacter.transform));
    }

    // 적 사망 이후 전투 종료
    private void HandleEnemyDeath()
    {
        // GameUI 잠금
        LockGameUI();
        // Die 트리거 활성
        enemyAnimator.SetTrigger("Die");
        StartCoroutine(DelayedBattleEnd());
    }
    
    private IEnumerator DelayedBattleEnd()
    {
        yield return new WaitForSeconds(3f);
        // 기존 OnBattleEnd 이벤트 호출
        CombatManager.Instance.IsInCombat = false;
        CombatDataHolder.LastTrigger?.OnBattleEnd();
    }

    public void TriggerSpecialAttack()
    {
        playerAnimator.SetTrigger("Attack");
        StartCoroutine(HandlePlayerAttack());
        AudioManager.Instance.PlaySFX("Battle/Attack");
    }

    public void TriggerSpecialShield()
    {
        playerAnimator.SetTrigger("Def");
        AudioManager.Instance.PlaySFX("Battle/Barrier");
    }

    public void TriggerSpecialStun()
    {
        playerAnimator.SetTrigger("Bark");
        AudioManager.Instance.PlaySFX("Battle/DogGrowling");
    }
    
    // 상태이펙트(버프/디버프) 발생 시
    private void HandleStatusEffect(bool isPlayer, bool isBuff)
    {
        var target = isPlayer ? playerCharacter.transform : enemyCharacter.transform;
        var prefab = isBuff ? buffEffect : debuffEffect;
        SpawnEffect(prefab, target.position);
    }

    // 필살기 이펙트
    private void HandleSpecialEffect(int idx)
    {
        if (idx >= 0 && idx < specialEffects.Length)
        {
            switch (idx)
            {
                case 0 : SpawnEffect(specialEffects[idx], enemyCharacter.transform.position + Vector3.left * 1.4f); break; 
                case 1 : SpawnEffect(specialEffects[idx], playerCharacter.transform.position + Vector3.right * 0.1f + Vector3.up * 0.25f); break;
                case 2 : SpawnEffect(specialEffects[idx], playerCharacter.transform.position + Vector3.right * 0.275f + Vector3.up * 1.3f); break;
            }
        }
    }
    
    private void SpawnEffect(GameObject prefab, Vector3 worldPos)
    {
        if (prefab == null) return;
        var go = Instantiate(prefab, worldPos, Quaternion.identity);

        // Animator 에서 재생 시간 가져오기
        float lifeTime = 0.5f; // 기본값
        var animator = go.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            if (clips.Length > 0)
                lifeTime = clips.Max(c => c.length);
        }

        // 트윈을 이용해 이펙트가 자연스럽게 사라지도록 연출
        if (go.TryGetComponent(out CanvasGroup cg))
        {
            cg.alpha = 1f;
            cg.DOFade(0f, 0.3f)
              .SetDelay(lifeTime - 0.3f)
              .SetEase(Ease.Linear);
        }
        else if (go.TryGetComponent(out SpriteRenderer sr))
        {
            var col = sr.color;
            DOVirtual.Float(1f, 0f, 0.3f, v =>
            {
                sr.color = new Color(col.r, col.g, col.b, v);
            }).SetDelay(lifeTime - 0.3f);
        }
        else
        {
            // 아무것도 없으면 Scale 트윈으로 사라지게
            go.transform.localScale = Vector3.one;
            go.transform.DOScale(0f, 0.3f)
              .SetDelay(lifeTime - 0.3f)
              .SetEase(Ease.InBack);
        }

        // 정확한 타이밍에 파괴
        Destroy(go, lifeTime);
    }
}