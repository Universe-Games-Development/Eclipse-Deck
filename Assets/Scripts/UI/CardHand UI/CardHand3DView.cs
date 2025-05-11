using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CardHand3DView : CardHandView {
    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private CardLayoutSettings layoutSettings;

    [Inject] private CardTextureRenderer cardTextureRenderer;

    private ICardLayoutStrategy _layoutStrategy;
    private Transform _hoveredCardTransform = null;

    [SerializeField] bool debugMode = false;

    [Inject]
    public void Initialize(ICardLayoutStrategy layoutStrategy = null) {
        // Проверяем, есть ли настройки в инспекторе
        if (layoutSettings == null) {
            Debug.LogWarning("No CardLayoutSettings assigned! Using default values.", this);
        }

        // Если стратегия не внедрена через Zenject, создаем стратегию по умолчанию
        if (layoutStrategy == null) {
            // Используем настройки из инспектора
            _layoutStrategy = new CurvedHandLayoutStrategy(layoutSettings);
        } else {
            _layoutStrategy = layoutStrategy;
            // Передаем настройки в стратегию, если она была внедрена через Zenject
            _layoutStrategy.SetSettings(layoutSettings);
        }
    }

    public void Update() {
        if (debugMode)
        UpdateCardPositions();
    }

    public void SetLayoutStrategy(ICardLayoutStrategy layoutStrategy) {
        if (layoutStrategy == null) return;

        _layoutStrategy = layoutStrategy;
        // Передаем настройки из инспектора в новую стратегию
        _layoutStrategy.SetSettings(layoutSettings);
        // Обновляем расположение карт с новой стратегией
        UpdateCardPositions();
    }

    public void SetLayoutSettings(CardLayoutSettings settings) {
        if (settings == null) return;

        this.layoutSettings = settings;
        // Обновляем настройки в текущей стратегии
        if (_layoutStrategy != null) {
            _layoutStrategy.SetSettings(settings);
            UpdateCardPositions();
        }
    }

    public override CardView BuildCardView(string id) {
        if (cardPrefab == null || cardsContainer == null) {
            Debug.LogError("CardPrefab or CardsContainer not set!", this);
            return null;
        }

        Card3DView card3D = Instantiate(cardPrefab, cardsContainer);
        card3D.OnHoverChanged += HandleCardHover;
        cardTextureRenderer.Register3DCard(card3D);

        return card3D;
    }

    private void HandleCardHover(CardView cardView, bool isHovered) {
        if (_layoutStrategy == null) return;

        // Если уже была наведенная карта, снимаем с нее hover
        if (_hoveredCardTransform != null && !isHovered && _hoveredCardTransform != cardView.transform) {
            _layoutStrategy.SetCardHovered(_hoveredCardTransform, false).Forget();
        }

        // Устанавливаем новую наведенную карту
        _hoveredCardTransform = isHovered ? cardView.transform : null;

        // Устанавливаем hover состояние для текущей карты
        _layoutStrategy.SetCardHovered(cardView.transform, isHovered).Forget();
    }

    public override void UpdateCardPositions() {
        if (_cardViews.Count == 0) return;

        // Собираем массив Transform компонентов всех карт
        Transform[] cardTransforms = new Transform[_cardViews.Count];
        int index = 0;
        foreach (var card in _cardViews.Values) {
            if (card != null) {
                cardTransforms[index] = card.transform;
                index++;
            }
        }

        // Применяем текущую стратегию расположения
        _layoutStrategy.LayoutCards(this, cardTransforms).Forget();
    }

    public override void HandleCardViewRemoval(CardView cardView) {
        // Снимаем обработчик события наведения перед удалением
        if (cardView is Card3DView card3D) {
            card3D.OnHoverChanged -= HandleCardHover;
        }

        // Если удаляемая карта была под наведением, сбрасываем состояние
        if (_hoveredCardTransform == cardView.transform) {
            _hoveredCardTransform = null;
        }

        // Вызываем базовый метод
        base.HandleCardViewRemoval(cardView);
    }

    public override void Cleanup() {
        // Очищаем все обработчики событий
        foreach (var cardView in _cardViews.Values) {
            if (cardView is Card3DView card3D) {
                card3D.OnHoverChanged -= HandleCardHover;
            }
        }

        _hoveredCardTransform = null;
        base.Cleanup();
    }
}
