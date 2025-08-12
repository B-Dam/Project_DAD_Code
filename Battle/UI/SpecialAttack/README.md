# 💥 Battle/UI/SpecialAttack — 필살기 UI

필살기 UI 모듈 설명입니다.

---

## ✨ 설계 특징 (Highlights)
- (추가 예정)

---

## 🔁 핵심 흐름
Select → PlayCutscene → ApplyEffect

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `SpecialAbilityPanel.cs`
```csharp
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

// ...

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

// ...

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
```
