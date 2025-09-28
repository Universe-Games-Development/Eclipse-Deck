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

    #region Card View Management 
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
    #endregion

    #region Hover
    // We move inner body because hovering start glitch when cursor under hovered object collider
    // And rotate global body to match user input
    protected override void HandleCardHovered(CardView cardView) {
        Card3DView card3DView = (Card3DView)cardView;

        card3DView.ModifyRenderOrder(hoverRenderOrderBoost);
        
        if (!cardLayoutData.TryGetValue(card3DView, out var data)) {
            Debug.LogWarning($"No layout data found for hovered card {card3DView.name}");
            return;
        }


        Transform target = card3DView.innerBody;
        Vector3 hoverPosition = data.position + hoverOffset;
        //Soon will rotate to face the player
        Vector3 targetRotation = Vector3.zero; // we align so player dont need to tilt his head


        Sequence hoverSequence = DOTween.Sequence();

        hoverSequence.Join(target.DOLocalMove(hoverOffset, cardHoverDuration));
        hoverSequence.Join(card3DView.transform.DOLocalRotate(targetRotation, cardHoverDuration));

        card3DView.DoSequenceInner(hoverSequence).Forget();
    }

    protected override void HandleClearCardHovered(CardView cardView) {
        Card3DView card3DView = (Card3DView)cardView;
        card3DView.ModifyRenderOrder(-hoverRenderOrderBoost);

        if (!cardLayoutData.TryGetValue(card3DView, out var data)) {
            Debug.LogWarning($"No layout data found for hovered card {card3DView.name}");
            return;
        }

        Transform target = card3DView.innerBody;

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Join(target.DOLocalMove(Vector3.zero, cardHoverDuration));
        returnSequence.Join(card3DView.transform.DOLocalRotate(data.rotation.eulerAngles, cardHoverDuration));

        card3DView.DoSequenceInner(returnSequence).Forget(); ;
    }
    #endregion

    #region Layout Poistioning
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
    public Vector3? GetOriginalCardPosition(CardView cardView) {
        return cardLayoutData.TryGetValue(cardView, out var data)
            ? data.position
            : null;
    }

    private void CleanupLayoutData(List<CardView> activeCardViews) {
        var activeCardViewsSet = new HashSet<CardView>(activeCardViews);
        var keysToRemove = new List<CardView>();

        foreach (var key in cardLayoutData.Keys) {
            if (!activeCardViewsSet.Contains(key)) {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove) {
            cardLayoutData.Remove(key);
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
    #endregion

    private void ClearContainer() {
        foreach (Transform child in cardsContainer) {
            if (child != null) {
                Destroy(child.gameObject);
            }
        }
    }

    protected void OnDestroy() {
        cardLayoutData.Clear();
    }
}
