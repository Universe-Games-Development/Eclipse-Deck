using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Planned to add card rotation to face the player in the future
public class CardHand3DView : CardHandView {
    [Header("Pool")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private CardPool cardPool;

    private readonly Dictionary<CardView, LayoutPoint> cardLayoutData = new();

    [SerializeField] LayoutSettings settings;
    ILayout3DHandler layout;

    [SerializeField] protected float cardsOrganizeDuration = 0.2f;

    [SerializeField] protected int baseRenderOrder = 2800;
    [Header("Hover Settings")]
    [SerializeField] protected int hoverRenderOrderBoost = 50;
    [SerializeField] protected float cardHoverDuration = 0.2f;
    [SerializeField] protected Vector3 hoverOffset = new Vector3(0f, 1f, 1f);
    [Header ("Card Spawning")]
    [SerializeField] Vector3 spawnOffset;


    private void Awake() {
        if (cardsContainer == null)
            throw new UnassignedReferenceException(nameof(cardsContainer));
        if (cardPool == null)
            throw new UnassignedReferenceException(nameof(cardPool));

        ClearContainer();
        layout = new Linear3DLayout(settings);
    }

    private void ClearContainer() {
        foreach (Transform child in cardsContainer) {
            if (child != null) {
                Destroy(child.gameObject);
            }
        }
    }

    public Vector3? GetOriginalCardPosition(CardView cardView) {
        return cardLayoutData.TryGetValue(cardView, out var data)
            ? data.position
            : null;
    }

    private void CleanupLayoutData(List<CardView> activeCardViews) {
        var activeCardViewsSet = new HashSet<CardView>(activeCardViews);
        var keysToRemove = cardLayoutData.Keys.Where(k => !activeCardViewsSet.Contains(k)).ToList();

        foreach (var key in keysToRemove) {
            cardLayoutData.Remove(key);
        }
    }

    public override void RegisterView(CardView cardView) {
        if (cardView != null) {
            //card3DView.transform.SetParent(cardsContainer); pool already placed on this container but maybe be global?
            cardView.transform.position = cardsContainer.position + spawnOffset;
            cardView.transform.rotation = cardsContainer.rotation;
        }
    }

    public override CardView CreateCardView() {
        return cardPool.Get();
    }

    protected override void HandleCardViewRemoval(CardView cardView) {
        cardLayoutData.Remove(cardView);

        if (cardView is Card3DView card3DView && cardPool != null) {
            cardPool.Release(card3DView);
        } else {
            Debug.LogWarning($"Trying to remove a CardView that is not a Card3DView or pool is null. Destroying: {cardView?.name}");
            if (cardView != null) {
                Destroy(cardView.gameObject);
            }
        }
    }

    protected override void HandleCardHovered(CardView cardView) {
        cardView.ModifyRenderOrder(hoverRenderOrderBoost);

        //cardView.SetHoverState(true); card dont need to know how to hover in hand
        
        if (!cardLayoutData.TryGetValue(cardView, out var data)) {
            Debug.LogWarning($"No layout data found for hovered card {cardView.name}");
            return;
        }


        Transform target = cardView.transform;
        Vector3 hoverPosition = data.position + hoverOffset;
        //Soon will rotate to face the player
        Vector3 targetRotation = Vector3.zero; // we align so player dont need to tilt his head


        Sequence hoverSequence = DOTween.Sequence();

        hoverSequence.Join(target.DOLocalMove(hoverPosition, cardHoverDuration));
        hoverSequence.Join(target.DOLocalRotate(targetRotation, cardHoverDuration));

        cardView.DoSequence(hoverSequence).Forget();
    }

    protected override void HandleClearCardHovered(CardView cardView) {
        cardView.ModifyRenderOrder(-hoverRenderOrderBoost);

        //cardView.SetHoverState(false); card dont need to know how to hover in hand

        if (!cardLayoutData.TryGetValue(cardView, out var data)) {
            Debug.LogWarning($"No layout data found for hovered card {cardView.name}");
            return;
        }

        Transform target = cardView.transform;

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Join(target.DOLocalMove(data.position, cardHoverDuration));
        returnSequence.Join(target.DOLocalRotate(data.rotation.eulerAngles, cardHoverDuration));

        cardView.DoSequence(returnSequence).Forget(); ;
    }

    public override void UpdateCardPositions(List<CardView> cardViews) {
        if (cardViews == null) return;

        var result = layout.CalculateLayout(cardViews.Count);
        var points = result.Points;
        // ќчищуЇмо застар≥л≥ дан≥
        CleanupLayoutData(cardViews);

        for (int i = 0; i < points.Count && i < cardViews.Count; i++) {
            var cardPoint = points[i];
            var cardView = cardViews[i];
            int cardOrder = cardPoint.orderIndex;

            if (cardView == null) continue;
            //cardPoint.position = cardsContainer.TransformPoint(cardPoint.position); we dont need to transform it in world position
            cardLayoutData[cardView] = cardPoint;

            cardView.SetRenderOrder(baseRenderOrder + cardOrder);

            AnimateToPosition(cardView, cardPoint);
        }
    }

    private void AnimateToPosition(CardView cardView, LayoutPoint cardPoint) {
        Transform cardTransform = cardView.transform;
        Sequence layoutSequence = DOTween.Sequence();

        layoutSequence.Join(cardTransform.DOLocalMove(cardPoint.position, cardsOrganizeDuration)
                                    .SetEase(Ease.OutQuad)
                                    .SetLink(cardTransform.gameObject));
        layoutSequence.Join(cardTransform.DOLocalRotate(cardPoint.rotation.eulerAngles, cardsOrganizeDuration)
                                    .SetEase(Ease.OutQuad)
                                    .SetLink(cardTransform.gameObject));
        cardView.DoSequence(layoutSequence).Forget();
    }

    protected void OnDestroy() {
        cardLayoutData.Clear();
    }
}
