using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    [Header("UI 참조")]
    [SerializeField] Image iconImage;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descText;
    [SerializeField] TextMeshProUGUI rankText;
    [SerializeField] TextMeshProUGUI TypeText;
    [SerializeField] private Outline outline;
    
    [HideInInspector] public RectTransform enemyDropZone;

    public RectTransform Rect { get; private set; }
    public CardData data { get; private set; }
    
    private Tween pulseTween; // 카드 아웃라인 효과용 트윈
    
    public int index;
    
    Vector2 dragOffset;
    HandManager handManager;
    Canvas canvas;
    CanvasGroup canvasGroup;
    
    // 튜토리얼 구독용 드래그 이벤트
    public static System.Action<CardView> OnCardBeginDrag;
    
    /// <summary>
    /// 튜토리얼 중 드래그만 막고 싶을 때 토글하는 플래그
    /// </summary>
    public static bool DisableCardDragging { get; set; } = false;
    
    void Awake()
    {
        // 컴포넌트 캐시
        Rect         = GetComponent<RectTransform>();
        canvas       = GetComponentInParent<Canvas>();
        canvasGroup  = GetComponent<CanvasGroup>() 
                       ?? gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;
        
        outline = outline ?? GetComponent<Outline>();

        if (outline != null)
        {
            // 1) 알파 0으로 초기화
            var c = outline.effectColor;
            c.a = 0f;
            outline.effectColor = c;

            // 2) 무한 Yoyo 트윈 생성하되, 자동 소멸하지 않도록
            pulseTween = DOTween.To(
                                    () => outline.effectColor.a,
                                    a =>
                                    {
                                        var col = outline.effectColor;
                                        col.a = a;
                                        outline.effectColor = col;
                                    },
                                    0.4f, // 피크 알파
                                    1f // 한 사이클 길이
                                )
                                .SetLoops(-1, LoopType.Yoyo)
                                .SetEase(Ease.InOutSine)
                                .SetAutoKill(false);

            // 3) 처음엔 실행
            pulseTween.Play();
        }
    }
    
    public void EnablePulse()
    {
        if (!pulseTween.IsPlaying())
            pulseTween.Play();
    }

    public void DisablePulse()
    {
        if (pulseTween.IsPlaying())
            pulseTween.Pause();
        // 즉시 감추기
        var col = outline.effectColor;
        col.a = 0f;
        outline.effectColor = col;
    }

    public void Initialize(CardData cardData, HandManager manager, RectTransform dropZone)
    {
        data           = cardData;
        handManager    = manager;
        enemyDropZone  = dropZone;

        // UI 바인딩
        iconImage.sprite = data.icon;
        nameText.text    = data.displayName;
        rankText.text    = data.rank.ToString();
        TypeText.text    = data.typePrimary.ToString();
        descText.text    = TextFormatter.Format(
            data.effectText,
            new System.Collections.Generic.Dictionary<string,string> {
                { "damage", (CombatManager.Instance.PlayerBaseAtk + data.effectAttackValue + CombatManager.Instance.playerAtkMod).ToString() },
                { "turns", data.effectTurnValue.ToString() },
                { "shield", data.effectShieldValue.ToString() },
                { "debuff", data.effectAttackDebuffValue.ToString() },
                { "buff", data.effectAttackIncreaseValue.ToString() }
            }
        );
        
        // HandManager에 자신 등록 & 첫 레이아웃 호출
        manager.AddCard(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 막기 용 플래그 (튜토리얼용)
        if (DisableCardDragging) return;
        
        canvasGroup.blocksRaycasts = false;  
        
        // Tween 취소
        Rect.DOKill();

        // 맨 앞 배치 & 반투명
        Rect.SetAsLastSibling();
        canvasGroup.alpha          = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // 드래그 플래그 ON
        handManager.isDraggingCard = true;

        // 클릭 시 카드와 포인터 간 오프셋 계산
        AudioManager.Instance.PlaySFX("Battle/SelectCard");
        Vector2 localMouse;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            handManager.handContainer, eventData.position, canvas.worldCamera, out localMouse);
        dragOffset = Rect.anchoredPosition - localMouse;
        
        // 앞쪽으로 올려주고 반투명
        Rect.SetAsLastSibling();
        canvasGroup.alpha          = 0.6f;
        canvasGroup.blocksRaycasts = false;
        
        // 튜토리얼에 드래그 시작 알림
        OnCardBeginDrag?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 막기 용 플래그 (튜토리얼용)
        if (DisableCardDragging) return;
        
        // 포인터 로컬 좌표 계산
        Vector2 localMouse;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            handManager.handContainer, eventData.position, canvas.worldCamera, out localMouse);

        // 오프셋 보정하여 바로 붙여줌
        Rect.anchoredPosition = localMouse + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 막기 용 플래그 (튜토리얼용)
        if (DisableCardDragging) return;
        
        // 투명도 복구 & Raycast 복원
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        handManager.isDraggingCard = false;

        // 드롭된 화면 좌표를 handContainer 로컬 좌표로 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            handManager.handContainer,
            eventData.position,
            canvas.worldCamera, out localPoint);
        
        // handContainer 안에 놓였는지 확인
        bool droppedInHand = handManager.handContainer.rect.Contains(localPoint);
        
        // handContainer 안이면 무조건 원위치
        if (droppedInHand)
        {
            handManager.LayoutHand();
            return;
        }
        
        // 드롭 위치가 적 드롭 존 안인지 체크
        bool droppedOnEnemy = RectTransformUtility
            .RectangleContainsScreenPoint(enemyDropZone, eventData.position, canvas.worldCamera);

        // 방어 카드거나, (공격/약화 카드면서) 적 위에 드롭됐으면 UseCard 시도
        if ((data.typePrimary == CardTypePrimary.실드 || droppedOnEnemy)
            && handManager.UseCard(this))
        {
            // 블록 레이캐스트를 꺼서 사용된 카드가 더 이상 드래그/클릭되지 않도록 함
            canvasGroup.blocksRaycasts = false;
            return;
        }
        
        handManager.LayoutHand();
    }
    
    // 카드 위로 다른 카드를 드롭했을 때 실행
    public void OnDrop(PointerEventData e)
    {
        // 드래그 중이던 카드
        var draggedGO = e.pointerDrag;
        if (draggedGO == null) return;

        var draggedCard = draggedGO.GetComponent<CardView>();
        if (draggedCard == null) return;

        // 자기 자신 위에 놓인 게 아니라면
        if (draggedCard == this) return;

        // 두 카드가 같은 Rank1이고, 아직 합성 중이 아니면
        if (draggedCard.data.rank == 1 &&
            this.data.rank == 1 &&
            HandManager.Instance.currentAP >= HandManager.Instance.combineAPCost)
        {
            // handManager 에서 TryCombine 로직 실행
            bool success = HandManager.Instance.TryCombine(draggedCard.data);
            if (success)
            {
                // (선택) 시각적 효과, 사운드 등 추가
                // 예: this.PlayCombineEffect();
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        handManager.OnCardHover(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        handManager.OnCardExit();
    }
}