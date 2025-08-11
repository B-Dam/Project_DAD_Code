using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpecialAbilityPanel : MonoBehaviour
{
    [Header("버튼 3개 (Inspector에서 크기 3으로)")]
    [SerializeField] private Button[] abilityButtons;
    
    [Header("필살기 사용 후 컷씬")]
    [SerializeField] private Animator cutsceneAnimator; // 컷씬용 Animator
    [SerializeField] private CombatAnimationController animCtrl; // 스킬 애니메이션 컨트롤러
    [SerializeField] private float cutsceneDuration = 1.5f; // 컷씬 시간
    
    [Header("컷씬 패널")]
    [SerializeField] private RectTransform cutscenePanel; // UI Image를 감싸는 RectTransform
    [SerializeField] private float enterDurationFast = 0.3f; // 첫 진입 속도
    [SerializeField] private float slowDuration = 0.6f; // 중앙에서 천천히
    [SerializeField] private float exitDurationFast = 0.2f;  // 종료 빠르게
    [SerializeField] private float slowOffset = 100f; // 느림 구간 Offset
    
    [Header("버튼 트윈 세팅")]
    [SerializeField] private float entryYOffset   = 300f;  // 씬에 배치된 Y에서 이만큼 아래에서 시작
    [SerializeField] private float entryDuration = 0.5f;
    [SerializeField] private float entryStagger  = 0.1f;
    [SerializeField] private float flipDuration  = 0.4f; // 뒤집기 애니메이션
    
    // 씬에 배치된 “최종 위치”를 저장
    private Vector2[] finalPositions;
    private bool      positionsCached = false;
    private Vector2[] frontOriginalSizes;
    
    private void OnEnable()
    {
        // 레이아웃 강제 업데이트 (최종 위치 올바르게 읽기 위해)
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

        // 최초 한 번만 씬에 배치된 위치 읽어 두기
        if (!positionsCached)
        {
            positionsCached = true;
            finalPositions = new Vector2[abilityButtons.Length];
            for (int i = 0; i < abilityButtons.Length; i++)
            {
                var rt = abilityButtons[i].GetComponent<RectTransform>();
                finalPositions[i] = rt.anchoredPosition;
            }
            
            // 그리고 앞면의 원래 크기도 같이 저장
            frontOriginalSizes = new Vector2[abilityButtons.Length];
            for (int i = 0; i < abilityButtons.Length; i++)
            {
                var frontImg = abilityButtons[i]
                               .transform.Find("Front")
                               .GetComponent<Image>()
                               .rectTransform;
                frontOriginalSizes[i] = frontImg.sizeDelta;
            }
        }

        // 3) 각 버튼 리셋 (위치, 스케일, flip 패널, interactable)
        for (int i = 0; i < abilityButtons.Length; i++)
        {
            var btn = abilityButtons[i];
            var rt  = btn.GetComponent<RectTransform>();

            // 씬 배치된 위치에서 아래로 startYOffset 만큼 떨어뜨려서 시작
            rt.anchoredPosition = finalPositions[i] - Vector2.up * entryYOffset;
            // 스케일도 반드시 1로
            rt.localScale = Vector3.one;

            // Back/Front CanvasGroup 초기화
            var back  = btn.transform.Find("Back")?.GetComponent<CanvasGroup>();
            var front = btn.transform.Find("Front")?.GetComponent<CanvasGroup>();
            if (back  != null) back .alpha = 1f;
            if (front != null) front.alpha = 0f;

            // 클릭 잠금
            btn.interactable = false;

            // 이전에 붙었던 Idle/Hover 스크립트가 있으면 제거
            Destroy(btn.GetComponent<SpecialCardIdle>());
            Destroy(btn.GetComponent<SpecialCardHover>());
        }

        // 4) 진입 + 뒤집기 애니메이션 시작
        StartCoroutine(AnimateEntryAndFlip());
    }

    private IEnumerator AnimateEntryAndFlip()
    {
        // 순차적으로 아래→원위치 이동
        for (int i = 0; i < abilityButtons.Length; i++)
        {
            var rt = abilityButtons[i].GetComponent<RectTransform>();
            rt.DOAnchorPos(finalPositions[i], entryDuration)
              .SetEase(Ease.OutBack);
            yield return new WaitForSeconds(entryStagger);
        }

        // 약간 여유 두고 → 뒤집기
        yield return new WaitForSeconds(0.1f);

        // 순차적으로 뒤집기
        for (int i = 0; i < abilityButtons.Length; i++)
        {
            // idx를 같이 넘김
            FlipCard(abilityButtons[i], i);
            yield return new WaitForSeconds(0.1f);
        }

        // 뒤집기 애니 지속 시간 후 → 버튼 활성화 & Idle/Hover
        yield return new WaitForSeconds(flipDuration);
        for (int i = 0; i < abilityButtons.Length; i++)
        {
            var btn = abilityButtons[i];
            int idx = i;  // 클로저 방지

            btn.interactable = true;
            // 리스너 초기화 후에 코루틴 호출 리스너 등록
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => StartCoroutine(DoSpecial(idx)));

            // 이제 아이들·호버 컴포넌트 붙이기
            if (btn.GetComponent<SpecialCardIdle>() == null)
                btn.gameObject.AddComponent<SpecialCardIdle>();
            if (btn.GetComponent<SpecialCardHover>() == null)
                btn.gameObject.AddComponent<SpecialCardHover>();
        }
    }

    private void FlipCard(Button btn, int idx)
    {
        var rt       = btn.GetComponent<RectTransform>();
        var backImg  = btn.transform.Find("Back") .GetComponent<Image>();
        var frontImg = btn.transform.Find("Front").GetComponent<Image>();
        var backCG   = btn.transform.Find("Back") .GetComponent<CanvasGroup>();
        var frontCG  = btn.transform.Find("Front").GetComponent<CanvasGroup>();
        if (backImg == null || frontImg == null || backCG == null || frontCG == null) return;

        // pivot & scale 초기화
        rt.pivot      = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        backCG.alpha  = 1f;
        frontCG.alpha = 0f;

        var seq = DOTween.Sequence();
        // 접듯이 가로 축소
        seq.Append(rt.DOScaleX(0f, flipDuration * 0.5f).SetEase(Ease.InQuad));

        // 중간(스케일X==0)에 스프라이트 교체 & 크기 리셋
        seq.AppendCallback(() =>
        {
            // 원래 front 크기로 복구
            frontImg.rectTransform.sizeDelta = frontOriginalSizes[idx];
            backCG.alpha  = 0f;
            frontCG.alpha = 1f;
        });

        // 다시 펴기
        seq.Append(rt.DOScaleX(1f, flipDuration * 0.5f).SetEase(Ease.OutQuad));
        seq.Play();
    }
    
    private void OnDisable()
    {
        // 혹시 남아 있는 트윈들 모두 정리
        foreach (var btn in abilityButtons)
        {
            var rt = btn.GetComponent<RectTransform>();
            DOTween.Kill(rt);
        }
    }
    
    void Start()
    {
        // (모노 눈 컷씬) 처음에는 화면 왼쪽 바깥에 위치
        var w = Screen.width;
        cutscenePanel.anchoredPosition = new Vector2(-w, 0);
    }
    
    IEnumerator PlayCutsceneFlyThrough()
    {
        // 화면 너비 + 패널 폭
        float screenW = Screen.width;
        float panelW  = cutscenePanel.rect.width;

        AudioManager.Instance.PlaySFX("Battle/UseSpecialSkill");

        // 화면 왼쪽 바깥으로 위치
        cutscenePanel.anchoredPosition = new Vector2(-screenW - panelW, 0);

        var seq = DOTween.Sequence();

        // 빠르게 -slowOffset 지점까지
        seq.Append(cutscenePanel
                   .DOAnchorPosX(-slowOffset, enterDurationFast)
                   .SetEase(Ease.OutCubic));

        // 느리게 +slowOffset/3 지점까지
        seq.Append(cutscenePanel
                   .DOAnchorPosX(+slowOffset/3, slowDuration)
                   .SetEase(Ease.OutQuad));

        // 다시 빠르게 오른쪽 바깥으로
        seq.Append(cutscenePanel
                   .DOAnchorPosX(screenW + panelW, exitDurationFast)
                   .SetEase(Ease.InCubic));

        // 끝나면 비활성화
        seq.OnComplete(() => cutscenePanel.gameObject.SetActive(false));

        seq.Play();
        yield return seq.WaitForCompletion();
    }


    private IEnumerator DoSpecial(int idx)
    {
        // UI 잠금 및 패널 페이드 아웃
        SpecialGaugeUI.Instance.DisableSpecialButton();
        SpecialAbilityUI.Instance.JustHideSpecialPanel();
        
        // 컷씬 오브젝트 활성화
        var go = cutsceneAnimator.gameObject;
        go.SetActive(true);

        // 컷씬 연출
        yield return StartCoroutine(PlayCutsceneFlyThrough());

        // 캐릭터 스킬 애니메이션 트리거
        switch (idx)
        {
            case 0: animCtrl.TriggerSpecialAttack(); break;
            case 1: animCtrl.TriggerSpecialShield(); break;
            case 2: animCtrl.TriggerSpecialStun();   break;
        }

        // 스킬 애니메이션이 끝날 때까지 대기(CombatAnimationController에 플래그 추가)
        /*yield return new WaitUntil(() => animCtrl.IsSkillAnimationDone);*/
        
        // 실제 효과 적용
        ExecuteSkill(idx);

        // 스킬 실행 뒤에만 게이지 초기화
        CombatManager.Instance.ConsumeSpecialGauge();

        // 버튼 비활성화
        SpecialGaugeUI.Instance.DisableSpecialButton();

        // 패널 닫기
        SpecialAbilityUI.Instance.HideSpecialPanel();
    }
    
    void ExecuteSkill(int idx)
    {
        var cm = CombatManager.Instance;
        switch(idx)
        {
            case 0:
                cm.SpecialAttack(60);
                break;
            case 1:
                cm.SpecialShield(50);
                cm.AddReflectBuff(0.5f, 1);
                break;
            case 2:
                cm.SpecialStun(2);
                break;
        }
    }
}