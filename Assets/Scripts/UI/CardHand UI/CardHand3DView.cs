using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

public class CardHand3DView : CardHandView {
    
    [SerializeField] private bool debugMode = false;
    
    [Header("Card Orientation")]
    [SerializeField] private Vector3 defaultCardRotation = Vector3.zero;
    [SerializeField] private bool inheritContainerRotation = true;

    [Header("Pool")]
    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private Transform cardsContainer;
    private ObjectPool<Card3DView> _cardPool;
    [SerializeField] private int initialPoolSize = 10;
    

    [Header("Bounds Visualization")]
    [SerializeField] private bool showBoundsInRuntime = true;
    [SerializeField] private HandBoundsVisualizer boundsVisualizer;

    private LinearHandLayoutStrategy _layoutStrategy;
    [SerializeField] private Linear3DHandLayoutSettings layoutSettings;

    [Header("Hover Settings")]
    [SerializeField] private int baseRenderQueueValue = 3000;
    [SerializeField] private int hoverRenderQueueBoost = 100;

    [Inject] private CardTextureRenderer _cardTextureRenderer;

    private Card3DView _hoveredCard = null;
    

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

        var cardsViews = new List<CardView>(_cardViews.Values);

        // Устанавливаем правильную ориентацию для всех карт ПЕРЕД layout
        SetupCardsOrientation(cardsViews);

        // Обновляем позиции через стратегию размещения
        //_layoutStrategy.UpdateLayout(new List<CardView>(_cardViews.Values), cardsContainer).Forget();

        CalculateLayoutParameters(cardsViews.Count, out float totalWidth, out float spacing, out float startX);
        for (int i = 0; i < cardsViews.Count; i++) {
            CardView cardView = cardsViews[i];
            if (cardView == null || cardView.transform == null) continue;
            (Vector3 targetPosition, Quaternion targetRotation) = CalculateCardTransform(
                cardsContainer, i, cardsViews.Count, startX, spacing);
            float speedFactor = Mathf.Clamp01(10f / Mathf.Max(1f, cardsViews.Count));
            float moveDuration = layoutSettings.MoveDuration * speedFactor;
            cardView.MoveTo(targetPosition, targetRotation, moveDuration);
        }
    }

    private void CalculateLayoutParameters(int cardCount, out float totalWidth, out float spacing, out float startX) {
        totalWidth = Mathf.Max(layoutSettings.MaxHandWidth, (cardCount - 1) * layoutSettings.CardThickness);
        spacing = cardCount > 1 ? totalWidth / (cardCount - 1) : 0f;
        startX = -totalWidth / 2f;
    }

    private (Vector3, Quaternion) CalculateCardTransform(Transform handTransform, int index, int totalCards, float startX, float spacing) {
        float xPos = startX + index * spacing;
        float yPos = layoutSettings.DefaultYPosition;
        float zPos = -index * layoutSettings.VerticalOffset;

        float randomOffset = (index % 3 - 1) * layoutSettings.PositionVariation;
        xPos += randomOffset;

        Vector3 targetPosition = handTransform.TransformPoint(new Vector3(xPos, yPos, zPos));
        float rotationAngle = CalculateRotationAngle(index, totalCards);
        Quaternion targetRotation = handTransform.rotation * Quaternion.Euler(0f, rotationAngle, 0f);

        return (targetPosition, targetRotation);
    }

    private float CalculateRotationAngle(int index, int totalCards) {
        if (totalCards == 1) return 0f;

        float t = (float)index / (totalCards - 1);
        float angle = Mathf.Lerp(-layoutSettings.MaxRotationAngle, layoutSettings.MaxRotationAngle, t);
        float randomOffset = (index % 2 == 0 ? 1 : -1) * layoutSettings.RotationOffset;
        angle += randomOffset;

        return angle;
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

    public void FitHandToScreen() {
        if (boundsVisualizer != null && layoutSettings != null) {
            float recommendedWidth = boundsVisualizer.GetRecommendedWidth();
            layoutSettings.MaxHandWidth = recommendedWidth;
            UpdateCardPositions();
            Debug.Log($"Hand width fitted to screen: {recommendedWidth:F2}");
        }
    }

    public void SetHandWidth(float width) {
        if (layoutSettings != null) {
            layoutSettings.MaxHandWidth = Mathf.Max(0.1f, width);
            UpdateCardPositions();
        }
    }

    public float GetRecommendedHandWidth() {
        return boundsVisualizer != null ? boundsVisualizer.GetRecommendedWidth() : 3f;
    }

    public void SetBoundsVisualization(bool enable) {
        showBoundsInRuntime = enable;
        if (boundsVisualizer != null) {
            boundsVisualizer.enabled = enable;
        }
    }

    #endregion
}