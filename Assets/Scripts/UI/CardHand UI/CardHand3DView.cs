using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class CardHand3DView : CardHandView {
    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private CardLayoutSettings layoutSettings;

    [Inject] private CardTextureRenderer cardTextureRenderer;
    private ICardLayoutStrategy _layoutStrategy;

    [Inject]
    public void Initialize(ICardLayoutStrategy layoutStrategy = null) {
        // Проверяем, есть ли настройки в инспекторе
        if (layoutSettings == null) {
            Debug.LogWarning("No CardLayoutSettings assigned! Using default values.", this);
        }

        // Если стратегия не внедрена через Zenject, создаем стратегию по умолчанию
        if (layoutStrategy == null) {
            // Используем настройки из инспектора
            _layoutStrategy = new LinearHandLayoutStrategy(layoutSettings);
        } else {
            _layoutStrategy = layoutStrategy;
            // Передаем настройки в стратегию, если она была внедрена через Zenject
            _layoutStrategy.SetSettings(layoutSettings);
        }
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
        cardTextureRenderer.Register3DCard(card3D);

        UpdateCardPositions();
        return card3D;
    }

    private void Update() {
        UpdateCardPositions();
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
        _layoutStrategy.LayoutCards(this, cardTransforms);
    }

    public override void Cleanup() {
        base.Cleanup();
        // Дополнительная очистка ресурсов при необходимости
    }
}