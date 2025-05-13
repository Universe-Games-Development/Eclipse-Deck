using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

public class CardHand3DView : CardHandView {
    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private Linear3DHandLayoutSettings layoutSettings;
    [SerializeField] private bool debugMode = false;

    [Header("Hover Settings")]
    [SerializeField] private int baseRenderQueueValue = 3000; // Базовое значение для прозрачных материалов
    [SerializeField] private int hoverRenderQueueBoost = 100; // Увеличение для карты под наведением

    [Header("Performance")]
    [SerializeField] private int initialPoolSize = 10;

    private LinearHandLayoutStrategy _layoutStrategy;
    [Inject] private CardTextureRenderer _cardTextureRenderer;

    private Card3DView _hoveredCard = null;
    private ObjectPool<Card3DView> _cardPool;


    #region Unity Lifecycle

    private void Awake() {
        _layoutStrategy = new LinearHandLayoutStrategy(layoutSettings);

        InitializeCardPool();
    }

    private void OnEnable() {
        // Подписка на события, если необходимо
    }

    private void OnDisable() {
        // Отписка от событий
    }


    public void Update() {
        if (debugMode)
            UpdateCardPositions();
    }

    protected override void OnDestroy() {
        // Очищаем все состояния
        _hoveredCard = null;
        _cardIndexCache.Clear();

        // Отписываем все обработчики событий
        foreach (var cardView in _cardViews.Values) {
            if (cardView is Card3DView card3D) {
                card3D.OnHoverChanged -= HandleCardHover;
                _cardTextureRenderer.UnRegister3DCard(card3D);
            }
        }
        _cardPool?.Clear();
        base.OnDestroy();
    }

    #endregion

    #region Pool Management

    private void InitializeCardPool() {
        if (_cardPool == null) {
            _cardPool = new ObjectPool<Card3DView>(
                createFunc: () => Instantiate(cardPrefab, cardsContainer),
                actionOnGet: card => {
                    card.gameObject.SetActive(true);
                    
                },
                actionOnRelease: card => {
                    card.Reset(); // ваш Reset метод
                    card.gameObject.SetActive(false);
                    card.transform.SetParent(cardsContainer); // або спеціальний контейнер
                },
                actionOnDestroy: card => Destroy(card.gameObject),
                collectionCheck: false, // або true для дебагу
                defaultCapacity: initialPoolSize,
                maxSize: 100
            );
        }
    }

    #endregion

    #region Card Management

    public override CardView BuildCardView(string id) {
        Card3DView card3D = _cardPool.Get();
        card3D.Id = id;

        // Регистрируем карту для создания текстуры через ICardTextureRenderer
        var uiView = _cardTextureRenderer.Register3DCard(card3D);

        

        return card3D;
    }

    public override void HandleCardViewRemoval(CardView cardView) {
        if (cardView is Card3DView card3D) {
            _cardTextureRenderer.UnRegister3DCard(card3D);
            _cardPool.Release(card3D);
        }

        if (_hoveredCard == cardView) {
            _hoveredCard = null;
        }

        base.HandleCardViewRemoval(cardView);
        RefreshCardIndexCache();
    }

    #endregion

    #region Hover and Rendering

    protected override void HandleCardHover(CardView changedCardView, bool isHovered) {
        Card3DView changedCard3D = changedCardView as Card3DView;
        if (changedCard3D == null) return;

        if (isHovered) {
            // Если другая карта уже была под курсором — сбрасываем её
            if (_hoveredCard != null && _hoveredCard != changedCardView) {
                _hoveredCard.ResetRenderingOrder();
            }

            _hoveredCard = changedCard3D;
            UpdateCardRenderOrder(changedCard3D, true);
        } else {
            // Если убирается наведение с текущей карты
            if (_hoveredCard == changedCardView) {
                _hoveredCard = null;
            }

            UpdateCardRenderOrder(changedCard3D, false);
        }
    }

    private void UpdateCardRenderOrder(Card3DView card3D, bool isHovered) {
        if (card3D == null) return;

        // Получаем индекс карты из кэша для оптимизации
        int index;
        if (!_cardIndexCache.TryGetValue(card3D, out index)) {
            index = GetCardIndex(card3D);
            _cardIndexCache[card3D] = index;
        }

        // Рассчитываем порядок рендеринга
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

    #endregion

    #region Layout

    public override void UpdateCardPositions() {
        if (_layoutStrategy == null || _cardViews.Count == 0) {
            return;
        }

        // Обновляем порядок рендеринга всех карт
        UpdateAllCardsRenderOrder();

        // Обновляем позиции через стратегию размещения
        _layoutStrategy.UpdateLayout(new List<CardView>(_cardViews.Values), cardsContainer).Forget();
    }

    #endregion
}
