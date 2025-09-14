using System;
using UnityEngine;

public class Linear3DHandLayout : HandLayoutStrategy {
    [SerializeField] private Linear3DHandLayoutSettings settings;
    [SerializeField] private HandBoundsVisualizer boundsVisualizer;

    [Header("Bounds Visualization")]
    [SerializeField] private bool showBoundsInRuntime = true;

    // Приватні поля
    [SerializeField] private Transform cardsContainer;
    private Camera targetCamera;

    // Кеш для оптимізації
    private int _lastCardCount = -1;
    private TransformPoint[] _cachedTransforms;
    private bool _needsRecalculation = true;

    #region Initialization

    private void Awake() {
        targetCamera = Camera.main;
        InitializeBoundsVisualizer();
        _needsRecalculation = true;
    }

    private void OnValidate() {
        _needsRecalculation = true;
    }

    #endregion

    #region Layout Calculation

    public override TransformPoint[] CalculateCardTransforms(int cardCount) {
        if (cardsContainer == null) {
            Debug.LogError("Cards container not initialized! Call Initialize() first.");
            return new TransformPoint[0];
        }

        // Використовуємо кеш
        if (!_needsRecalculation && _lastCardCount == cardCount && _cachedTransforms != null) {
            return _cachedTransforms;
        }

        if (cardCount == 0) {
            _cachedTransforms = new TransformPoint[0];
            _lastCardCount = 0;
            return _cachedTransforms;
        }

        var transforms = new TransformPoint[cardCount];
        CalculateLayout(cardCount, transforms);

        _cachedTransforms = transforms;
        _lastCardCount = cardCount;
        _needsRecalculation = false;

        return transforms;
    }

    private void CalculateLayout(int cardCount, TransformPoint[] transforms) {
        var layoutParams = CalculateLayoutParameters(cardCount);

        for (int i = 0; i < cardCount; i++) {
            transforms[i] = CalculateSingleCardTransform(i, cardCount, layoutParams);
        }
    }

    private LayoutParameters CalculateLayoutParameters(int cardCount) {
        var parameters = new LayoutParameters();

        if (cardCount <= 1) {
            parameters.totalWidth = 0f;
            parameters.spacing = 0f;
            parameters.startX = 0f;
            parameters.cardScale = 1f;
            return parameters;
        }

        // Використовуємо методи з ScriptableObject для динамічних налаштувань
        parameters.cardScale = settings.GetScaleForCardCount(cardCount);
        parameters.spacing = settings.GetSpacingForCardCount(cardCount);

        // Обчислюємо загальну ширину
        parameters.totalWidth = (cardCount - 1) * parameters.spacing;
        parameters.totalWidth = Mathf.Min(settings.MaxHandWidth, parameters.totalWidth);

        // Перераховуємо spacing якщо потрібно
        if (parameters.totalWidth < (cardCount - 1) * parameters.spacing) {
            parameters.spacing = parameters.totalWidth / (cardCount - 1);
        }

        parameters.startX = -parameters.totalWidth / 2f;
        return parameters;
    }

    private TransformPoint CalculateSingleCardTransform(int index, int totalCards, LayoutParameters layoutParams) {
        // Локальна позиція
        Vector3 localPosition = CalculateLocalPosition(index, totalCards, layoutParams);

        // Локальне обертання
        Quaternion localRotation = CalculateLocalRotation(index, totalCards);

        // Перетворення в світові координати
        Vector3 worldPosition = cardsContainer.TransformPoint(localPosition);
        Quaternion worldRotation = cardsContainer.rotation * localRotation;

        return new TransformPoint(worldPosition, worldRotation, Vector3.one, index);
    }

    private Vector3 CalculateLocalPosition(int index, int totalCards, LayoutParameters layoutParams) {
        float xPos = layoutParams.startX + index * layoutParams.spacing;
        float yPos = -index * settings.HeightOffset;
        float zPos = -index * settings.VerticalOffset;

        // Додаємо варіацію тільки по X та невелику по Y для природнього вигляду
        if (settings.PositionVariation > 0f && totalCards > 1) {
            float seed = index * 12.9898f; // Детермінований "рандом"
            float randomX = (Mathf.Sin(seed) * 2f - 1f) * settings.PositionVariation;
            float randomY = (Mathf.Sin(seed * 1.5f) * 2f - 1f) * settings.PositionVariation * 0.1f;

            xPos += randomX;
            yPos += randomY;
        }

        return new Vector3(xPos, yPos, zPos);
    }

    private Quaternion CalculateLocalRotation(int index, int totalCards) {
        if (totalCards <= 1) return Quaternion.identity;

        float t = (float)index / (totalCards - 1);
        float angle = Mathf.Lerp(-settings.MaxRotationAngle, settings.MaxRotationAngle, t);

        // Додаємо детерміновану варіацію
        if (settings.RotationOffset > 0f) {
            float seed = index * 23.1406f;
            float randomOffset = (Mathf.Sin(seed) * 2f - 1f) * settings.RotationOffset;
            angle += randomOffset;
        }

        return Quaternion.Euler(0f, angle, 0f);
    }

    #endregion

    #region Bounds Management

    private void Update() {
        if (showBoundsInRuntime && boundsVisualizer != null) {
            CheckHandBounds();
        }
    }

    private void InitializeBoundsVisualizer() {
        if (boundsVisualizer == null && cardsContainer != null) {
            GameObject visualizerObj = new GameObject("HandBoundsVisualizer");
            visualizerObj.transform.SetParent(transform);
            visualizerObj.transform.localPosition = Vector3.zero;

            boundsVisualizer = visualizerObj.AddComponent<HandBoundsVisualizer>();
            boundsVisualizer.Initialize(settings, cardsContainer, targetCamera);
        }

        if (boundsVisualizer != null) {
            boundsVisualizer.Initialize(settings, cardsContainer, targetCamera);
            boundsVisualizer.enabled = showBoundsInRuntime;
        }
    }

    private void CheckHandBounds() {
        if (settings == null || boundsVisualizer == null) return;

        float recommendedWidth = boundsVisualizer.GetRecommendedWidth();
        if (settings.MaxHandWidth > recommendedWidth * 1.1f) {
            Debug.LogWarning($"Hand width ({settings.MaxHandWidth:F2}) exceeds safe bounds. " +
                           $"Recommended: {recommendedWidth:F2}");
        }
    }

    #endregion

    #region Public API

    public void ForceRecalculation() {
        _needsRecalculation = true;
    }

    public void FitHandToScreen() {
        if (boundsVisualizer != null && settings != null) {
            float recommendedWidth = boundsVisualizer.GetRecommendedWidth();
            settings.MaxHandWidth = recommendedWidth;
            ForceRecalculation();
            Debug.Log($"Hand width fitted to screen: {recommendedWidth:F2}");
        }
    }

    public void SetBoundsVisualization(bool enable) {
        showBoundsInRuntime = enable;
        if (boundsVisualizer != null) {
            boundsVisualizer.enabled = enable;
        }
    }

    public Linear3DHandLayoutSettings GetSettings() => settings;

    #endregion

    private struct LayoutParameters {
        public float totalWidth;
        public float spacing;
        public float startX;
        public float cardScale;
    }
}
