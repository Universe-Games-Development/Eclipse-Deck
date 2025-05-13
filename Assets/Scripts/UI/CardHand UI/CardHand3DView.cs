using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CardHand3DView : CardHandView {
    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private Linear3DHandLayoutSettings layoutSettings;
    [SerializeField] private bool debugMode = false;
    [Header("Hover Settings")]
    [SerializeField] private int baseRenderQueueValue = 3000; // Базовое значение для прозрачных материалов
    [SerializeField] private int hoverRenderQueueBoost = 100; // Увеличение для карты под наведением

    private LinearHandLayoutStrategy _layoutStrategy;
    [Inject] private CardTextureRenderer cardTextureRenderer;

    private Card3DView _hoveredCard = null;

    private void Awake() {
        _layoutStrategy = new LinearHandLayoutStrategy(layoutSettings);
    }

    public void Update() {
        if (debugMode)
            UpdateCardPositions();
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



    private void UpdateCardRenderOrder(Card3DView card3D, bool isHovered) {
        if (card3D == null) return;

        // Получаем базовый индекс сортировки из позиции карты в руке
        int index = GetCardIndex(card3D);

        // Рассчитываем порядок рендеринга:
        // - Базовый порядок согласно положению в руке
        // - Если карта под наведением, добавляем большое смещение
        int sortingOrder = baseRenderQueueValue + index;
        if (isHovered) {
            sortingOrder += hoverRenderQueueBoost;
        }

        // Устанавливаем порядок рендеринга для карты
        card3D.SetSortingOrder(sortingOrder);
    }

    private int GetCardIndex(CardView cardView) {
        int index = 0;
        foreach (var kvp in _cardViews) {
            if (kvp.Value == cardView) {
                return index;
            }
            index++;
        }
        return 0;
    }

    private void HandleCardHover(CardView changedCardView, bool isHovered) {
        Card3DView changedCard3D = changedCardView as Card3DView;
        if (changedCard3D == null) return;

        if (isHovered) {
            // Якщо інша карта вже була під курсором — скидаємо її
            if (_hoveredCard != null && _hoveredCard != changedCardView) {
                _hoveredCard.ResetRenderingOrder();
            }

            _hoveredCard = changedCard3D;
            UpdateCardRenderOrder(changedCard3D, true);
        } else {
            // Якщо забирається наведення з поточної карти
            if (_hoveredCard == changedCardView) {
                _hoveredCard = null;
            }

            UpdateCardRenderOrder(changedCard3D, false);
        }
    }


    public override void UpdateCardPositions() {
        if (_layoutStrategy != null && _cardViews.Count == 0) {
            return;
        }
        UpdateAllCardsRenderOrder();
        _layoutStrategy.UpdateLayout(new List<CardView>(_cardViews.Values), cardsContainer).Forget();
    }

    public override void HandleCardViewRemoval(CardView cardView) {
        // Снимаем обработчик события наведения перед удалением
        if (cardView is Card3DView card3D) {
            card3D.OnHoverChanged -= HandleCardHover;
        }

        // Если удаляемая карта была под наведением, сбрасываем состояние
        if (_hoveredCard == cardView) {
            _hoveredCard = null;
        }

        // Вызываем базовый метод
        base.HandleCardViewRemoval(cardView);
    }

    private void UpdateAllCardsRenderOrder() {
        int index = 0;
        foreach (var cardView in _cardViews.Values) {
            if (cardView is Card3DView card3D) {
                bool isHovered = cardView == _hoveredCard;
                int sortingOrder = baseRenderQueueValue + index;
                if (isHovered) {
                    sortingOrder += hoverRenderQueueBoost;
                }
                card3D.SetSortingOrder(sortingOrder);
            }
            index++;
        }
    }

    public override void Cleanup() {
        // Очищаем все обработчики событий
        foreach (var cardView in _cardViews.Values) {
            if (cardView is Card3DView card3D) {
                card3D.OnHoverChanged -= HandleCardHover;
            }
        }

        _hoveredCard = null;
        base.Cleanup();
    }
}

public class CardObjectPool : MonoBehaviour {
    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private Transform poolContainer;

    private Queue<Card3DView> _pool = new Queue<Card3DView>();

    private void Awake() {
        InitializePool();
    }

    private void InitializePool() {
        for (int i = 0; i < initialPoolSize; i++) {
            CreateNewCardInPool();
        }
    }

    private Card3DView CreateNewCardInPool() {
        Card3DView instance = Instantiate(cardPrefab, poolContainer);
        instance.gameObject.SetActive(false);
        _pool.Enqueue(instance);
        return instance;
    }

    public Card3DView GetCard() {
        if (_pool.Count == 0) {
            CreateNewCardInPool();
        }

        Card3DView card = _pool.Dequeue();
        card.gameObject.SetActive(true);
        return card;
    }

    public void ReturnCard(Card3DView card) {
        if (card == null) return;

        // Сбрасываем состояние карты перед возвратом в пул
        card.Reset();
        card.gameObject.SetActive(false);

        // Перемещаем в контейнер пула и добавляем обратно в очередь
        card.transform.SetParent(poolContainer);
        _pool.Enqueue(card);
    }
}