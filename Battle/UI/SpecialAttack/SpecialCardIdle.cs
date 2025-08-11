using UnityEngine;
using DG.Tweening;

public class SpecialCardIdle : MonoBehaviour
{
    private RectTransform rt;

    [Header("기본 설정")]
    [SerializeField] private float baseFloatRange = 30f;    // 기본 진폭
    [SerializeField] private float baseFloatDuration = 1f;   // 기본 주기

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        // 개별 카드마다 랜덤 진폭/속도/위상 부여
        float range = baseFloatRange * Random.Range(0.8f, 1.2f);
        float duration = baseFloatDuration * Random.Range(0.8f, 1.2f);

        // 시작 위상도 랜덤으로 약간씩 딜레이
        float startDelay = Random.Range(0f, duration);

        // 3재 Y 위치를 기준점으로 잡고
        float originY = rt.anchoredPosition.y;

        // Tween 실행
        rt
            .DOAnchorPosY(originY + range, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(startDelay);
    }

    void OnDisable()
    {
        // 이 RectTransform에 걸린 모든 트윈 Kill
        rt.DOKill();
    }
}