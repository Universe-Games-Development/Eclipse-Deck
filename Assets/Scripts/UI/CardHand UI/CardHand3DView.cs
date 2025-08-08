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
    [SerializeField] private int baseRenderQueueValue = 3000;
    [SerializeField] private int hoverRenderQueueBoost = 100;

    [Header("Card Orientation")]
    [SerializeField] private Vector3 defaultCardRotation = Vector3.zero; // Поворот "лицом вверх"
    [SerializeField] private bool inheritContainerRotation = true;


    [Header("Performance")]
    [SerializeField] private int initialPoolSize = 10;

    [Header("Bounds Visualization")]
    [SerializeField] private bool showBoundsInRuntime = true;
    [SerializeField] private HandBoundsVisualizer boundsVisualizer;

    private LinearHandLayoutStrategy _layoutStrategy;
    [Inject] private CardTextureRenderer _cardTextureRenderer;

    private Card3DView _hoveredCard = null;
    private ObjectPool<Card3DView> _cardPool;

    #region Unity Lifecycle

    private void Awake() {
        _layoutStrategy = new LinearHandLayoutStrategy(layoutSettings);

        foreach (Transform child in cardsContainer.transform) {
            Destroy(child.gameObject);
        }

        InitializeCardPool();
        InitializeBoundsVisualizer();
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

        // Проверка выхода карт за границы экрана
        if (showBoundsInRuntime && boundsVisualizer != null) {
            CheckHandBounds();
        }
    }

    protected override void OnDestroy() {
        // Очищаем все состояния
        _hoveredCard = null;
        _cardIndexCache.Clear();

        // Отписываем все обработчики событий
        foreach (var cardView in _cardViews.Values) {
            if (cardView is Card3DView card3D) {
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
                    card.Reset();
                    card.gameObject.SetActive(false);
                    card.transform.SetParent(cardsContainer);
                },
                actionOnDestroy: card => { if (card && card.gameObject) Destroy(card.gameObject); },
                collectionCheck: false,
                defaultCapacity: initialPoolSize,
                maxSize: 100
            );
        }
    }

    #endregion

    #region Bounds Visualization

    private void InitializeBoundsVisualizer() {
        if (boundsVisualizer == null) {
            // Создаем визуализатор границ если его нет
            GameObject visualizerObj = new GameObject("HandBoundsVisualizer");
            visualizerObj.transform.SetParent(transform);
            visualizerObj.transform.localPosition = Vector3.zero;

            boundsVisualizer = visualizerObj.AddComponent<HandBoundsVisualizer>();

            // Настраиваем визуализатор
            var visualizerField = typeof(HandBoundsVisualizer).GetField("layoutSettings",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            visualizerField?.SetValue(boundsVisualizer, layoutSettings);

            var containerField = typeof(HandBoundsVisualizer).GetField("cardsContainer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            containerField?.SetValue(boundsVisualizer, cardsContainer);
        }
    }

    private void CheckHandBounds() {
        if (layoutSettings == null || boundsVisualizer == null) return;

        float recommendedWidth = boundsVisualizer.GetRecommendedWidth();

        if (layoutSettings.MaxHandWidth > recommendedWidth) {
            // Автоматически корректируем ширину руки если она выходит за границы
            if (Application.isPlaying) {
                Debug.LogWarning($"Hand width ({layoutSettings.MaxHandWidth:F2}) exceeds safe bounds. " +
                               $"Recommended: {recommendedWidth:F2}");

                // Можно автоматически корректировать
                // layoutSettings.MaxHandWidth = recommendedWidth;
            }
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

    protected override void OnCardHover(CardView changedCardView, bool isHovered) {
        base.OnCardHover(changedCardView, isHovered);
    }

    public override void SetCardHover(CardView changedCardView, bool isHovered) {
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

        var cardList = new List<CardView>(_cardViews.Values);

        // Устанавливаем правильную ориентацию для всех карт ПЕРЕД layout
        SetupCardsOrientation(cardList);

        // Обновляем позиции через стратегию размещения
        _layoutStrategy.UpdateLayout(new List<CardView>(_cardViews.Values), cardsContainer).Forget();
    }

    #endregion

    private void SetupCardsOrientation(List<CardView> cards) {
        foreach (var cardView in cards) {
            if (cardView is Card3DView card3D) {
                SetCardOrientation(card3D);
            }
        }
    }

    private void SetCardOrientation(Card3DView card3D) {
        if (inheritContainerRotation) {
            // Наследуем поворот от контейнера (обычно для рук игрока)
            card3D.transform.rotation = cardsContainer.rotation;
        } else {
            // Используем фиксированный поворот (для особых случаев)
            card3D.transform.rotation = Quaternion.Euler(defaultCardRotation);
        }
    }
    #region Public API для настройки границ

    /// <summary>
    /// Подгоняет ширину руки под размер экрана
    /// </summary>
    public void FitHandToScreen() {
        if (boundsVisualizer != null && layoutSettings != null) {
            float recommendedWidth = boundsVisualizer.GetRecommendedWidth();
            layoutSettings.MaxHandWidth = recommendedWidth;
            UpdateCardPositions();
            Debug.Log($"Hand width fitted to screen: {recommendedWidth:F2}");
        }
    }

    /// <summary>
    /// Устанавливает ширину руки
    /// </summary>
    public void SetHandWidth(float width) {
        if (layoutSettings != null) {
            layoutSettings.MaxHandWidth = Mathf.Max(0.1f, width);
            UpdateCardPositions();
        }
    }

    /// <summary>
    /// Получает рекомендуемую ширину руки для текущего экрана
    /// </summary>
    public float GetRecommendedHandWidth() {
        return boundsVisualizer != null ? boundsVisualizer.GetRecommendedWidth() : 3f;
    }

    /// <summary>
    /// Включает/выключает визуализацию границ
    /// </summary>
    public void SetBoundsVisualization(bool enable) {
        showBoundsInRuntime = enable;
        if (boundsVisualizer != null) {
            boundsVisualizer.enabled = enable;
        }
    }

    #endregion
}