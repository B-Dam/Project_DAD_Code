using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float animDuration = 0.5f;

    // 초기화 - 필요시 사용
    public void Initialize(int maxHealth)
    {
        SetHealth(maxHealth, maxHealth);
    }

    // 체력 업데이트
    public void SetHealth(int currentHealth, int maxHealth)
    {
        float targetRatio = (float)currentHealth / maxHealth;
        fillImage.DOKill();
        fillImage.DOFillAmount(targetRatio, animDuration).SetEase(Ease.OutQuad);
    }
}