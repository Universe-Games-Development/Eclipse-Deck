using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CardHand3DView : CardHandView {
    [Inject] private IVisualManager _visualManager;

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
    [SerializeField] private float cardOrganizeDuration = 0.5f;
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
        CardView card = cardPool.Get();
        card.gameObject.SetActive(false);
        card.transform.SetParent(cardsContainer);
        card.transform.position = cardsContainer.position + spawnOffset;
        card.transform.rotation = cardsContainer.rotation;
        return card;
    }

    protected override void AddCard(CardView cardView) {
        var addTask = new AddCardVisualTask(
            cardView,
            layoutComponent,
            cardOrganizeDuration
        );
        _visualManager.Push(addTask);
        UpdateCardPositions();
    }

    protected override void RemoveCard(CardView cardView) {
        var removeTask = new RemoveCardVisualTask(
            cardView,
            layoutComponent,
            cardPool,
            cardOrganizeDuration
        );
        _visualManager.Push(removeTask);
        UpdateCardPositions();
    }

    protected override void AddCards(List<CardView> cardViews) {
        var addTask = new AddCardsVisualTask(
            cardViews,
            layoutComponent,
            cardOrganizeDuration
        );
        _visualManager.Push(addTask);
        UpdateCardPositions();
    }

    protected override void RemoveCards(List<CardView> cardViews) {
        UpdateCardPositions();
    }

    public override void UpdateCardPositions() {
        var updateTask = new UpdateCardLayoutVisualTask(
            layoutComponent,
            cardOrganizeDuration
        );
        _visualManager.Push(updateTask);
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
        hoverSequence.SetLink(target.gameObject);

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
        returnSequence.SetLink(target.gameObject);

        returnSequence.Play();
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

public class AddCardVisualTask : VisualTask {
    private readonly CardView _cardView;
    private readonly CardHandLayoutComponent _layout;

    public AddCardVisualTask(
        CardView cardView,
        CardHandLayoutComponent layout,
        float animationDuration = 0.3f) {
        _cardView = cardView;
        _layout = layout;
    }

    public override async UniTask<bool> ExecuteAsync() {
        _cardView.gameObject.SetActive(true);
        // Додаємо в layout і перераховуємо позиції
        _layout.AddItem(_cardView, recalculate: false);
        await UniTask.CompletedTask;
        return true;
    }
}

public class AddCardsVisualTask : VisualTask {
    private readonly List<CardView> _cards;
    private readonly CardHandLayoutComponent _layout;

    public AddCardsVisualTask(
        List<CardView> cards,
        CardHandLayoutComponent layout,
        float animationDuration = 0.3f) {
        _cards = cards;
        _layout = layout;
    }

    public override async UniTask<bool> ExecuteAsync() {
        foreach (var card in _cards) {
            card.gameObject.SetActive(true);
            _layout.AddItem(card, recalculate: false);
        }

        await UniTask.CompletedTask;
        return true;
    }
}

// Burn effect needs duration soon
public class RemoveCardVisualTask : VisualTask {
    private readonly CardView _cardView;
    private readonly CardHandLayoutComponent _layout;
    private readonly CardPool _cardPool;

    public RemoveCardVisualTask(
        CardView cardView,
        CardHandLayoutComponent layout,
        CardPool cardPool,
        float animationDuration = 0.3f) {
        _cardView = cardView;
        _layout = layout;
        _cardPool = cardPool;
    }

    public override async UniTask<bool> ExecuteAsync() {
        // Видаляємо з layout
        _layout.RemoveItem(_cardView, recalculate: false);
        _cardPool.Release(_cardView);

        await UniTask.CompletedTask;
        return true;
    }
}


public class UpdateCardLayoutVisualTask : VisualTask {
    private readonly CardHandLayoutComponent _layout;
    private readonly float _animationDuration;

    public UpdateCardLayoutVisualTask(
        CardHandLayoutComponent layout,
        float animationDuration = 0.3f) {
        _layout = layout;
        _animationDuration = animationDuration;
    }

    public override async UniTask<bool> ExecuteAsync() {
        _layout.RecalculateLayout();

        float duration = _animationDuration * TimeModifier;
        await _layout.AnimateAllToLayoutPositions(duration);
        return true;
    }
}
