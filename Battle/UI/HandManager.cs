using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class HandManager : MonoBehaviour
{
    [Header("핸드 설정")] 
    public RectTransform handContainer;   // 카드들이 배치될 부모 RectTransform
    public RectTransform drawDeckPile;    // 드로우 덱 위치
    public RectTransform discardDeckPile; // 버림 덱 위치
    public GameObject cardPrefab;         // CardView 프리팹
    public int maxHandSize = 5;           // 최대 핸드 크기
    public RectTransform enemyDropZone;   // 에디터에서 EnemyPanel 할당

    [Header("합성 설정")]
    [Tooltip("합성 시 소모할 AP")]
    public int combineAPCost = 1;

    [Header("레이아웃 설정")] 
    public float cardWidth = 200f; // 카드 너비 (px, RectTransform width)
    public float spacing = 40f;    // 카드 간 간격
    public float animDuration = 0.25f;

    [Header("포커스 설정")] 
    public float focusScale = 1.2f;
    public float focusShift = 50f;  // 양옆으로 밀어내는 거리
    public float focusLift  = 200f;  // 위로 띄울 거리
    
    [Header("드로우 애니메이션용")]
    [SerializeField] private float drawMoveDuration  = 0.5f;  // 카드 이동 시간
    [SerializeField] private float drawScaleDuration = 0.1f;  // 카드 팝업 스케일 시간
    [SerializeField] private float drawStaggerDelay  = 0.1f;  // 카드 간 지연
    [SerializeField] private float spawnYOffset      = 250f;   // 카드 스폰 y축 프리셋 조정용
    
    [Header("버림 애니메이션용")]
    [SerializeField] private float discardMoveDuration   = 0.4f;  // 이동 시간
    [SerializeField] private float discardStaggerDelay   = 0.05f; // 카드 간 지연
    
    [Header("리필 애니메이션용")]
    [SerializeField] private GameObject cardBackPrefab;       // 카드 뒷면 프리팹
    [SerializeField] private float refillMoveDuration  = 0.4f; // 이동 시간
    [SerializeField] private float refillScaleDuration = 0.3f; // 축소 시간
    [SerializeField] private float refillStaggerDelay  = 0.05f;// 카드 간 지연

    [HideInInspector] public bool isDraggingCard;
    public bool AllowCombine = true;

    // 현재 AP
    private int _currentAP;

    public int currentAP
    {
        get => _currentAP;
        set
        {
            _currentAP = value;
            RefreshCardPulse();
        }
    }

    // 내부 덱 / 디스카드 / 핸드 뷰
    public List<CardData> deck = new List<CardData>();
    public List<CardData> discard = new List<CardData>();
    List<CardView> handViews = new List<CardView>();

    CardView currentlyDiscarding;
    
    // 튜토리얼 모드 플래그
    public bool IsTutorialMode { get; set; } = false;
    // 튜토리얼용 덱 순서 큐
    private Queue<CardData> tutorialDeckQueue;
    // 카드 합성 확인용 이벤트
    public static event System.Action<CardView> OnCardCombinedNew;

    public static HandManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 처음 한 번 덱 초기화, 섞기
        InitializeDeck();
        
        // 턴 시작 구독
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnPlayerTurnStart += StartPlayerTurn;
        else
            Debug.LogError("HandManager: TurnManager.Instance is NULL");
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnPlayerTurnStart -= StartPlayerTurn;
    }
    
    /// <summary>
    /// 튜토리얼 모드로 덱을 지정된 순서로 세팅
    /// </summary>
    public void SetupTutorialDeck(IEnumerable<CardData> orderedCards)
    {
        IsTutorialMode = true;
        tutorialDeckQueue = new Queue<CardData>(orderedCards);
    }
    
    void InitializeDeck()
    {
        if (IsTutorialMode)
            return; // 이미 tutorialDeckQueue가 세팅되어 있으므로, deck 사용 안 함
        
        // DataManager에서 플레이어 기본 카드 3종을 가져와서 5장씩 복제
        var baseCards = DataManager.Instance.GetPlayerCards();
        deck = baseCards
               .SelectMany(cd => Enumerable.Repeat(cd, 5))
               .ToList();

        Shuffle(deck);
    }

    // AP 대비 펄스 상태를 갱신하는 메서드
    private void RefreshCardPulse()
    {
        foreach (var cv in handViews)
        {
            if (cv.data.costAP <= currentAP)
                cv.EnablePulse();
            else
                cv.DisablePulse();
        }
    }

    // 플레이어 턴이 시작될 때 호출
    void StartPlayerTurn()
    {
        ClearHand();
        currentAP = 3;

        // 카드 드로우
        for (int i = 0; i < maxHandSize; i++)
            DrawCard();
        
        // 즉시가 아니라 다음 프레임에 레이아웃 걸기
        DOVirtual.DelayedCall(0f, () => {
            DOTween.Kill("Layout");
            LayoutHand();
        });
    }

    // 덱 섞기
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public void DrawCard()
    {
        CardData data;

        // 카드 데이터 분기
        if (IsTutorialMode && tutorialDeckQueue != null && tutorialDeckQueue.Count > 0)
        {
            // 튜토리얼 모드: 지정한 순서대로 큐에서 꺼내 쓰기
            data = tutorialDeckQueue.Dequeue();
        }
        else
        {
            // 일반 모드: 덱 비었으면 리필·셔플
            if (deck.Count == 0)
                RefillAndShuffleDeck();

            // 손패 꽉 찼거나 덱 비었으면 리턴
            if (deck.Count == 0 || handViews.Count >= maxHandSize)
                return;

            data = deck[0];
            deck.RemoveAt(0);
        }

        // CardView 생성 및 초기화
        var go = Instantiate(cardPrefab, handContainer);
        var cv = go.GetComponent<CardView>();
        cv.Initialize(data, this, enemyDropZone);
        AudioManager.Instance.PlaySFX("Battle/DrawCard");

        // RectTransform 가져오기
        var rt = cv.Rect;
        
        // drawDeckPile의 월드 위치를 handContainer 로컬 좌표로 변환
        Vector3 worldPos = drawDeckPile.position;
        Vector3 localPos = handContainer.InverseTransformPoint(worldPos);
        localPos.y -= spawnYOffset; // y 오프셋 추가: spawnYOffset만큼 아래로
        
        // 카드를 덱 위치에 스폰
        rt.anchoredPosition = new Vector2(localPos.x, localPos.y);
        rt.localScale = Vector3.zero;
        
        // CanvasGroup 세팅
        var cg = cv.GetComponent<CanvasGroup>();
        if (cg == null) cg = cv.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // 스태거 딜레이 계산 (예: 기존 핸드 수 * interval)
        float delay = (handViews.Count - 1) * drawStaggerDelay;

        // 페이드 인 + 스케일 팝업
        cg.DOFade(1f, drawScaleDuration)
          .SetDelay(delay)
          .SetEase(Ease.Linear);
        cv.Rect
          .DOScale(1f, animDuration)
          .From(0.8f)
          .SetDelay(delay)
          .SetEase(Ease.OutBack);

        // 마지막에 전체 레이아웃
        DOVirtual.DelayedCall(delay + animDuration, LayoutHand);
    }

    // 새로 생성된 CardView를 핸드에 등록하고, 레이아웃을 갱신
    public void AddCard(CardView cv)
    {
        // 내부 리스트에 등록
        handViews.Add(cv);

        // 카드가 추가될 때도, 현재 AP 대비 펄스 상태를 초기화
        if (cv.data.costAP <= currentAP)
            cv.EnablePulse();
        else
            cv.DisablePulse();

        // 모든 CardView.index 재설정
        for (int i = 0; i < handViews.Count; i++)
            handViews[i].index = i;
    }

    // discard를 모두 덱으로 되돌리고 셔플
    void RefillAndShuffleDeck()
    {
        deck = discard.ToList();
        discard.Clear();
        Shuffle(deck);
    }
    
    // 손에 있는 카드를 전부 없에기 : 튜토리얼용
    public void DiscardHandInstant()
    {
        foreach (var cv in handViews)
            Destroy(cv.gameObject);
        handViews.Clear();
    }

    // 카드 사용 시 호출
    public bool UseCard(CardView cv)
    {
        // AP 체크
        if (currentAP < cv.data.costAP)
        {
            return false;
        }

        // AP 차감, 효과 적용
        currentAP -= cv.data.costAP;
        CombatUI.Instance.UpdateAP(currentAP);
        
        // 필살기 게이지 증가
        CombatManager.Instance.GainSpecialGauge(1);
        
        CombatManager.Instance.ApplySkill(cv.data, isPlayer: true);

        // 기본 카드(rank==1)만 discard에 추가하고, 합성된 카드(rank>1)는 버리지 않음
        if (cv.data.rank == 1)
            discard.Add(cv.data);

        // 현재 카드만 LayoutHand 스킵하기 위한 플래그
        currentlyDiscarding = cv;

        // Tween 시작 전에 남아있는 트윈 모두 제거
        var rt = cv.Rect;
        rt.DOKill();
        
        // 버림 덱 위치 계산
        Vector3 worldTarget = discardDeckPile.position;
        Vector3 localTarget = ((RectTransform)rt.parent).InverseTransformPoint(worldTarget);
        
        // 카드 이동 + 축소 애니메이션
        rt.DOAnchorPos(localTarget, discardMoveDuration)
          .SetEase(Ease.InQuad);
        rt.DOScale(0.2f, discardMoveDuration)
          .SetEase(Ease.InQuad)
          .OnComplete(() =>
          {
              // Hand에서 제거
              handViews.Remove(cv);
              
              // 인덱스 재설정
              for (int i = 0; i < handViews.Count; i++)
                  handViews[i].index = i;
              
              Destroy(cv.gameObject);
              LayoutHand();
          });

        isDraggingCard = false;
        return true;
    }

    /// <summary>
    /// 같은 Rank1 카드 두 장을 합성해 Rank2 카드로 만드는 함수
    /// </summary>
    public bool TryCombine(CardData baseCardData)
    {
        // 카드 합성이 막힌 경우 바로 반환
        if (!AllowCombine) return false;
        
        // AP 체크
        if (currentAP < combineAPCost) return false;

        // 같은 이름·같은 rank 카드 두 장 찾기
        var candidates = handViews
                         .Where(cv =>
                             cv.data.displayName == baseCardData.displayName &&
                             cv.data.rank == baseCardData.rank)
                         .ToList();
        if (candidates.Count < 2) return false;

        // AP 차감
        currentAP -= combineAPCost;
        CombatUI.Instance.UpdateAP(currentAP);
        CombatUI.Instance.UpdateUI();
        
        // 필살기 게이지 획득
        CombatManager.Instance.GainSpecialGauge(2);
        
        // 두 장 제거 (트윈 끊지 않도록 바로 DOKill + Destroy)
        foreach (var cv in candidates.Take(2))
        {
            // 소모 카드 데이터를 discard에 추가
            discard.Add(cv.data);
            
            // 핸드에서 제거
            handViews.Remove(cv);
            Destroy(cv.gameObject);
        }
        
        int nextRank = baseCardData.rank + 1;
        var newData = DataManager.Instance.GetCard(baseCardData.displayName, nextRank);
        if (newData == null)
        {
            // 레이아웃 복귀
            DOVirtual.DelayedCall(0f, LayoutHand);
            return false;
        }
        
        // 신규 카드 생성 및 초기화 (여기서 AddCard()만, 레이아웃은 따로)
        var go   = Instantiate(cardPrefab, handContainer);
        var cvNew = go.GetComponent<CardView>();
        cvNew.Initialize(newData, this, enemyDropZone);
        
        // 카드 합성 성공해서 생성됐는지 확인 이벤트 호출
        OnCardCombinedNew?.Invoke(cvNew);
        
        // 인덱스 재설정
        for (int i = 0; i < handViews.Count; i++)
            handViews[i].index = i;

        // LayoutHand 한 번 호출해서 새 카드 포함 모든 카드에 애니메이션 걸기
        DOTween.Kill("Layout");  // 기존 레이아웃 트윈만 초기화
        
        isDraggingCard = false;
        LayoutHand();

        // 새 카드만 별도 스케일 애니메이션
        cvNew.Rect.localScale = Vector3.zero;
        cvNew.Rect
             .DOScale(1f, animDuration)
             .SetId("Combine")
             .SetEase(Ease.OutBack);

        return true;
    }

    // 핸드 전체 제거 (초기화용)
    void ClearHand()
    {
        foreach (var cv in handViews)
            Destroy(cv.gameObject);
        handViews.Clear();
    }

    // 카드 배치
    public void LayoutHand()
    {
        if (isDraggingCard) return;

        int count = handViews.Count;
        if (count == 0) return;

        DOTween.Kill("Layout");

        float totalW = count * cardWidth + (count - 1) * spacing;
        float startX = -totalW / 2f + cardWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            var cv = handViews[i];

            // 버려지는 애니메이션 중인 카드는 완전히 건너뛰기
            if (cv == currentlyDiscarding) continue;

            var rt = cv.Rect;

            float x = startX + i * (cardWidth + spacing);
            rt.DOAnchorPos(new Vector2(x, 0), animDuration).SetId("Layout").SetEase(Ease.OutQuad);
            rt.DOScale(1f, animDuration).SetId("Layout").SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// 카드 위에 포인터 enter 될 때 호출
    /// </summary>
    public void OnCardHover(int idx)
    {
        if (isDraggingCard) return;

        int count = handViews.Count;
        if (count == 0) return;
        
        DOTween.Kill("Layout");

        float totalW = count * cardWidth + (count - 1) * spacing;
        float startX = -totalW / 2f + cardWidth / 2f;
        
        for (int i = 0; i < count; i++)
        {
            var cv = handViews[i];

            // 버려지는 중인 카드는 보호
            if (cv == currentlyDiscarding) continue;

            var rt = cv.Rect;
            float baseX = startX + i * (cardWidth + spacing);
            Vector2 pos = new Vector2(baseX, 0);
            float scale = 1f;
            Ease easePos = Ease.OutQuad, easeScale = Ease.OutQuad;

            if (i == idx)
            {
                // 스케일 키우기
                scale = focusScale;
                // 위로 살짝 띄우기
                pos.y += focusLift;
                rt.SetAsLastSibling();
                easeScale = Ease.OutBack;
            }
            else if (i == idx - 1) pos += Vector2.left * focusShift;
            else if (i == idx + 1) pos += Vector2.right * focusShift;

            rt.DOAnchorPos(pos, animDuration).SetId("Layout").SetEase(easePos);
            rt.DOScale(scale, animDuration).SetId("Layout").SetEase(easeScale);
        }
    }

    /// <summary>
    /// 카드를 떼면 기본 배치로 복귀
    /// </summary>
    public void OnCardExit()
    {
        if (isDraggingCard) return;
        
        LayoutHand();
    }
    
    /// <summary>
    /// 턴 종료 버튼과 연결
    /// </summary>
    public void OnEndTurnButton()
    {
        // 기본 카드(rank==1)만 Discard에 추가, 합성 카드(rank>1)는 제외
        foreach (var cv in handViews)
        {
            if (cv.data.rank == 1)
                discard.Add(cv.data);
        }
        
        // 2) 애니메이션 코루틴 실행
        StartCoroutine(DiscardHandAndEndTurn());
        TurnManager.Instance.EndPlayerTurn();
    }

    private IEnumerator DiscardHandAndEndTurn()
    {
        // 복사본으로 돌리기
        var cards = handViews.ToList();
        handViews.Clear();

        for (int i = 0; i < cards.Count; i++)
        {
            var cv = cards[i];
            var rt = cv.Rect;
            rt.DOKill();

            // 부모를 캔버스 루트로 이동
            rt.SetParent(handContainer.parent, true);

            // 목표 위치 계산
            Vector3 worldTarget = discardDeckPile.position;
            Vector3 localTarget = ((RectTransform)rt.parent)
                .InverseTransformPoint(worldTarget);
            // 이동 + 축소
            rt.DOAnchorPos(localTarget, discardMoveDuration)
              .SetEase(Ease.InQuad);
            rt.DOScale(0.2f, discardMoveDuration)
              .SetEase(Ease.InQuad)
              .OnComplete(() => Destroy(cv.gameObject));

            yield return new WaitForSeconds(discardStaggerDelay);
        }

        // 마지막 카드 움직임 끝나길 대기
        yield return new WaitForSeconds(discardMoveDuration);
    }
}