using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Planned to add card rotation to face the player in the future
public class CardHand3DView : CardHandView {
    [Header("Pool")]
    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private Card3DPool cardPool;

    private readonly Dictionary<CardView, LayoutPoint> cardLayoutData = new();

    [SerializeField] Linear3DHandLayoutSettings settings;
    [SerializeField] SummonZone3DLayoutSettings settings2;
    ILayout3DHandler layout;

    [SerializeField] protected float cardsOrganizeDuration = 0.2f;

    [SerializeField] protected int baseRenderOrder = 2800;
    [Header("Hover Settings")]
    [SerializeField] protected int hoverRenderOrderBoost = 50;
    //[SerializeField] protected float cardHoverDuration = 0.2f;
    //[SerializeField] private float hoverOffsetY = 1.0f;
    //[SerializeField] private float hoverOffsetZ = 1.0f;
    

    private void Awake() {
        ClearContainer();
        layout = new Linear3DLayout(settings);
        //layout = new SummonZone3DLayout(settings2);
    }

    private void ClearContainer() {
        if (cardsContainer == null) return;

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
        if (cardView != null && cardsContainer != null) {
            //card3DView.transform.SetParent(cardsContainer);
            cardView.transform.position = cardsContainer.position;
            cardView.transform.rotation = cardsContainer.rotation;
        }
    }

    public override CardView CreateCardView(Card card) {
        if (cardPool == null) {
            Debug.LogError("Card pool is not assigned!");
            return null;
        }

        return cardPool.Get();
    }

    protected override void HandleCardViewRemoval(CardView cardView) {
        cardLayoutData.Remove(cardView);

        //if (cardView is Card3DView card3DView && cardPool != null) {
        //    //cardPool.Release(card3DView);
        //} else {
        //    Debug.LogWarning($"Trying to remove a CardView that is not a Card3DView or pool is null. Destroying: {cardView?.name}");
        //    if (cardView != null) {
        //        Destroy(cardView.gameObject);
        //    }
        //}
    }
    
    protected override void HandleCardHovered(CardView cardView) {
        cardView.ModifyRenderOrder(hoverRenderOrderBoost);

        cardView.SetHoverState(true);
        //Transform target = cardView.transform;
        //if (!cardLayoutData.TryGetValue(cardView, out var data)) {
        //    Debug.LogWarning($"No layout data found for hovered card {cardView.name}");
        //    return;
        //}

        //// Розраховуємо hover позицію
        //Vector3 hoverPosition = data.position + new Vector3(0f, hoverOffsetY, hoverOffsetZ);

        //Sequence hoverSequence = DOTween.Sequence();
        //hoverSequence.Join(target.DOMove(hoverPosition, cardHoverDuration));

        ////Soon will rotate to face the player
        //hoverSequence.Join(target.DORotate(data.rotation.eulerAngles, cardHoverDuration));

        //cardView.DoSequence(hoverSequence);
    }

    protected override void HandleClearCardHovered(CardView cardView) {
        cardView.ModifyRenderOrder(-hoverRenderOrderBoost);

        cardView.SetHoverState(false);
        //Transform target = cardView.transform;

        //if (cardLayoutData.TryGetValue(cardView, out var data)) {
        //    // Анімуємо назад до оригінальної позиції
        //    Sequence returnSequence = DOTween.Sequence();
        //    returnSequence.Join(target.DOMove(data.position, cardHoverDuration));
        //    returnSequence.Join(target.DORotate(data.rotation.eulerAngles, cardHoverDuration));

        //    cardView.DoSequence(returnSequence);
        //}
    }

    public override void UpdateCardPositions(List<CardView> cardViews) {
        if (cardViews == null) return;

        if (layout == null) {
            Debug.LogWarning("Layout strategy is not assigned.");
            return;
        }

        var points = layout.CalculateCardTransforms(cardViews.Count);

        // Очищуємо застарілі дані
        CleanupLayoutData(cardViews);

        for (int i = 0; i < points.Count && i < cardViews.Count; i++) {
            var cardPoint = points[i];
            var cardView = cardViews[i];
            int cardOrder = cardPoint.orderIndex;

            if (cardView == null) continue;
            cardPoint.position = cardsContainer.TransformPoint(cardPoint.position);
            cardLayoutData[cardView] = cardPoint;

            cardView.SetRenderOrder(baseRenderOrder + cardOrder);

            AnimateToPosition(cardView, cardPoint);
        }
    }

    private void AnimateToPosition(CardView cardView, LayoutPoint cardPoint) {
        Transform cardTransform = cardView.transform;

        Tweener moveTween = cardTransform.DOMove(cardPoint.position, cardsOrganizeDuration)
                                    .SetEase(Ease.OutQuad)
                                    .SetLink(cardTransform.gameObject);

        cardView.DoTweener(moveTween).Forget();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        cardLayoutData.Clear();
    }
}


