using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CardHand3DView : CardHandView {
    [Header("Pool")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private CardPool cardPool;

    [Header("Layout")]
    [SerializeField] private CardHandLayoutComponent layoutComponent;

    [Header("Card Spawning")]
    [SerializeField] private Vector3 spawnOffset;

    [Header("Hover Settings")]
    [SerializeField] private int hoverRenderOrderBoost = 50;
    [SerializeField] private float cardHoverDuration = 0.2f;
    [SerializeField] private Vector3 hoverOffset = new Vector3(0f, 1f, 1f);

    [Header("Card Specific")]
    [SerializeField] private int baseRenderOrder = 2800;

    #region Unity Lifecycle

    protected void Awake() {
        ValidateReferences();
        SetupLayoutComponent();
        ClearContainer();
    }

    protected override void OnDestroy() {
        base.OnDestroy(); // Важливо викликати базову логіку!

        if (layoutComponent != null) {
            layoutComponent.OnLayoutCalculated -= HandleLayoutCalculated;
            layoutComponent.OnItemPositioned -= HandleItemPositioned;
            layoutComponent.ClearItems();
        }
    }

    #endregion

    #region Initialization

    private void ValidateReferences() {
        if (cardsContainer == null)
            throw new UnassignedReferenceException(nameof(cardsContainer));
        if (cardPool == null)
            throw new UnassignedReferenceException(nameof(cardPool));
        if (layoutComponent == null)
            throw new UnassignedReferenceException(nameof(layoutComponent));
    }

    private void SetupLayoutComponent() {
        layoutComponent.OnLayoutCalculated += HandleLayoutCalculated;
        layoutComponent.OnItemPositioned += HandleItemPositioned;
    }

    private void ClearContainer() {
        foreach (Transform child in cardsContainer) {
            if (child != null) {
                Destroy(child.gameObject);
            }
        }
    }

    #endregion

    #region Card View Management (Base Class Overrides)

    public override CardView CreateCardView() {
        return cardPool.Get();
    }

    protected override void OnRegisterView(CardView cardView) {
        // Встановлюємо початкову позицію
        cardView.transform.position = cardsContainer.position + spawnOffset;
        cardView.transform.rotation = cardsContainer.rotation;
        cardView.transform.SetParent(cardsContainer);

        // Додаємо в layout - він сам перерахує позиції
        layoutComponent.AddItem(cardView, recalculate: true);

        // Анімуємо на позицію
        layoutComponent.AnimateToLayoutPosition(cardView).Forget();
    }

    protected override void HandleCardViewRemoval(CardView cardView) {
        // Видаляємо з layout
        layoutComponent.RemoveItem(cardView, recalculate: true);

        // Анімуємо решту карт
        layoutComponent.AnimateAllToLayoutPositions().Forget();

        // Повертаємо в pool
        cardPool.Release(cardView);
    }

    #endregion

    #region Hover (Base Class Overrides)

    protected override void HandleCardHovered(CardView cardView) {
        if (!(cardView is Card3DView card3DView)) return;

        card3DView.ModifyRenderOrder(hoverRenderOrderBoost);

        // Отримуємо оригінальну позицію з layout component
        var originalRotation = layoutComponent.GetRotation(card3DView);
        if (!originalRotation.HasValue) {
            Debug.LogWarning($"No layout data found for hovered card {card3DView.name}");
            return;
        }

        Transform target = card3DView.innerBody;
        Vector3 targetRotation = Vector3.zero; // Вирівнюємо для гравця

        Sequence hoverSequence = DOTween.Sequence();
        hoverSequence.Join(target.DOLocalMove(hoverOffset, cardHoverDuration));
        hoverSequence.Join(card3DView.transform.DOLocalRotate(targetRotation, cardHoverDuration));

        hoverSequence.Play();
    }

    protected override void HandleClearCardHovered(CardView cardView) {
        if (!(cardView is Card3DView card3DView)) return;

        card3DView.ModifyRenderOrder(-hoverRenderOrderBoost);

        // Отримуємо оригінальне обертання з layout component
        var originalRotation = layoutComponent.GetRotation(card3DView);
        if (!originalRotation.HasValue) {
            Debug.LogWarning($"No layout data found for card {card3DView.name}");
            return;
        }

        Transform target = card3DView.innerBody;

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Join(target.DOLocalMove(Vector3.zero, cardHoverDuration));
        returnSequence.Join(card3DView.transform.DOLocalRotate(originalRotation.Value.eulerAngles, cardHoverDuration));

        returnSequence.Play();
    }

    #endregion

    #region Layout Positioning

    public override void UpdateCardPositions() {
        layoutComponent.RecalculateLayout();
        layoutComponent.AnimateAllToLayoutPositions().Forget();
    }

    public Vector3? GetOriginalCardPosition(CardView cardView) {
        return layoutComponent.GetPosition(cardView);
    }

    public Quaternion? GetOriginalCardRotation(CardView cardView) {
        return layoutComponent.GetRotation(cardView);
    }

    #endregion

    #region Layout Events

    private void HandleLayoutCalculated(LayoutResult result) {
        var cardViews = layoutComponent.GetAllItems();
        for (int i = 0; i < cardViews.Count; i++) {
            cardViews[i].SetRenderOrder(baseRenderOrder + i);
        }
    }

    private void HandleItemPositioned(CardView card, LayoutPoint point) {
        // Викликається для кожної карти після перерахунку
        // Можна додати custom логіку
    }

    #endregion
}