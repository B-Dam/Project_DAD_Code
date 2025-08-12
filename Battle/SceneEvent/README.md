# 🎬 Battle/SceneEvent — 전투 씬 세팅

전투 씬 세팅 모듈 설명입니다.

---

## ✨ 설계 특징 (Highlights)
- (추가 예정)

---

## 🔁 핵심 흐름
Enter Trigger → SetupScene → Fade In → Play Cutscene → Run Tutorial

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `CombatSceneController.cs`
```csharp
public void PlayBattleStartSequence()
    {
        // BattleStartPanel 활성화 및 초기 상태
        battleStartGroup.gameObject.SetActive(true);
        battleStartGroup.alpha = 0f;
        battleStartGroup.transform.localScale = Vector3.zero;

        // GameUI 초기 숨김
        gameUIGroup.alpha = 0f;
        gameUIGroup.interactable = gameUIGroup.blocksRaycasts = false;

        // DOTween 시퀀스 구성
        var seq = DOTween.Sequence();
        
        // BattleStartPanel 나타내기 (스케일 + 페이드)
        seq.Append(battleStartGroup.DOFade(1f, startAnimDuration));
        seq.Join(battleStartGroup.transform
                                 .DOScale(1f, startAnimDuration)
                                 .SetEase(Ease.OutBack));
        
        // 전투 시작 직전에 환경을 먼저 설정
        seq.AppendCallback(() =>
        {
            CombatManager.Instance.SetEnvironmentEffect(currentEnvironment);

            if (currentEnvironment.effectId == "wind")
            {
                AudioManager.Instance.PlayBGM("WindyBattleBGM");
            }
            else if (currentEnvironment.effectId == "rain")
            {
                AudioManager.Instance.PlayBGM("RainningBattleBGM");
            }
            else if (currentEnvironment.effectId == "fog")
            {
                AudioManager.Instance.PlayBGM("BattleMapBGM");
            }
        });
        seq.AppendCallback(() =>
        {
            CombatManager.Instance.StartCombat();
        });
        
        // 잠시 대기
        seq.AppendInterval(battleDisplayTime);

        // BattleStartPanel 사라지기
        seq.Append(battleStartGroup.DOFade(0f, uiFadeDuration));
        seq.Join(battleStartGroup.transform
                                 .DOScale(0.8f, uiFadeDuration));

        // 완전 비활성화
        seq.AppendCallback(() =>
        {
            battleStartGroup.gameObject.SetActive(false);
        });
        
        // 환경 안내 패널 세팅 & 인
        seq.AppendCallback(() =>
        {
            envNameText.text = currentEnvironment.title;
            envDescText.text = currentEnvironment.description;
            envIcon.sprite = currentEnvironment.icon;
            envGroup.alpha = 0f;
            envGroup.gameObject.SetActive(true);
        });
        seq.Append(envGroup.DOFade(1f, envEnterDuration)
                           .SetEase(Ease.OutBack));

        // 유지
        seq.AppendInterval(envShowDuration);

        // 환경 안내 패널 아웃
        seq.Append(envGroup.DOFade(0f, envExitDuration)
                           .SetEase(Ease.InQuad));
        seq.AppendCallback(() =>
        {
            envGroup.gameObject.SetActive(false);
        });

        // GameUI 각 요소를 스태거(stagger)로 등장시키기
        seq.AppendCallback(ShowGameUIWithStagger);

        // 인터렉션 활성화
        seq.AppendCallback(() =>
        {
            gameUIGroup.interactable = gameUIGroup.blocksRaycasts = true;
        });
        
        seq.AppendCallback(() => {
            if (HandManager.Instance.IsTutorialMode)
                TutorialManager.Instance.ShowStep(0);
        });
        
        seq.Play();
    }

// ...

private void Start()
    {
        // 튜토리얼 강제 모드일 때
        if (forceTutorialMode && forceTutorialDeckOrder != null && forceTutorialDeckOrder.Length > 0)
        {
            HandManager.Instance.SetupTutorialDeck(forceTutorialDeckOrder);
            HandManager.Instance.IsTutorialMode = true;
        }
        
        // 세팅 데이터 가져오기
        CombatSetupData data = CombatDataHolder.GetData();

        if (data != null)
        {
            _setupData = data;
            SetupCombat(data);

            // 튜토리얼 모드면 여기서도 한 번 보강 주입
            if (CombatDataHolder.LastTrigger != null 
                && CombatDataHolder.LastTrigger.isTutorialTrigger 
                && data.tutorialDeckOrder != null)
            {
                HandManager.Instance.SetupTutorialDeck(data.tutorialDeckOrder);
            }
        }
        else
        {
            // 전투 세팅 데이터가 없으면 : 배틀 씬에서 테스트 하거나 etc
            Debug.LogWarning("전투 세팅 데이터가 없습니다. 모든 환경 중 하나를 랜덤으로 지정합니다.");
            if (allEnvironments != null && allEnvironments.Count > 0)
                currentEnvironment = allEnvironments[Random.Range(0, allEnvironments.Count)];
            else
                Debug.LogError("allEnvironments에 할당된 SO가 없습니다!");
            
            // 환경 아이콘 및 툴팁 텍스트 설정
            RefreshEnvironmentUI();
        }

        // 애니메이터 연결
        var animCtrl = enemyObject.GetComponentInChildren<CombatAnimationController>();
        if (animCtrl != null)
            animCtrl.enemyAnimator = this.enemyAnimator;
        
        // 전투 데이터 로드 후에 호출
        PlayBattleStartSequence();
    }

// ...

private void SetupCombat(CombatSetupData data)
    {
        // 배경 설정
        if (backgroundImage != null && data.backgroundSprite != null)
            backgroundImage.sprite = data.backgroundSprite;

        // 적 애니메이터 설정
        if (enemyAnimator != null && data.animatorController != null)
            enemyAnimator.runtimeAnimator
// (이하 생략)
```
