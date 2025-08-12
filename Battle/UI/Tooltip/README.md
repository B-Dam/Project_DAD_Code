# 💡 Battle/UI/Tooltip — 툴팁 UI

툴팁 UI 모듈 설명입니다.

---

## ✨ 설계 특징 (Highlights)
- (추가 예정)

---

## 🔁 핵심 흐름
PointerEnter → Show → PointerExit

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `TooltipController.cs`
```csharp
public void Hide()
    {
        cg.DOKill();
        cg.DOFade(0f, fadeDuration)
          .SetEase(Ease.OutSine)
          .OnComplete(() =>
          {
              cg.interactable   = false;
              cg.blocksRaycasts = false;
          });
    }

// ...

public void ShowCurrentSkill()
    {
        var skill = PreviewUI.Instance.CurrentSkill;
        if (skill == null) return;

        nameText.text = skill.displayName;

        // 포맷팅
        string formatted = TextFormatter.Format(
            skill.effectText,
            new System.Collections.Generic.Dictionary<string,string> {
                { "damage", (CombatManager.Instance.EnemyBaseAtk
                             + skill.effectAttackValue
                             + CombatManager.Instance.enemyAtkMod).ToString() },
                { "turns",  skill.effectTurnValue.ToString() },
                { "shield", skill.effectShieldValue.ToString() },
                { "debuff", skill.effectAttackDebuffValue.ToString() },
                { "buff",   skill.effectAttackIncreaseValue.ToString() }
            }
        );
        descText.text = formatted;

        // 기존 트윈 중단
        cg.DOKill();
        // 인터랙트/레이캐스트 허용
        cg.interactable   = true;
        cg.blocksRaycasts = true;
        // 알파 페이드인
        cg.DOFade(1f, fadeDuration).SetEase(Ease.OutSine);
    }
```
