using UnityEngine;

public class RetryButton : MonoBehaviour
{
    [SerializeField] private CanvasGroup gameUIGroup;
    [SerializeField] private GameObject retryPanel;

    public void OnRetryButtonClicked()
    {
        // 리트라이 플래그 세팅
        CombatDataHolder.IsRetry = true;

        // UI 잠금 해제
        gameUIGroup.interactable   = true;
        gameUIGroup.blocksRaycasts = true;

        // 리트라이 패널 숨기기
        retryPanel.SetActive(false);

        // 전투 초기화(리셋)
        CombatManager.Instance.StartCombat();
    }
}