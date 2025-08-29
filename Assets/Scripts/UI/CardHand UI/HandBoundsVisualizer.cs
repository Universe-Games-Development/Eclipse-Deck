using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HandBoundsVisualizer : MonoBehaviour {
    [SerializeField] private HandBoundsSettings boundsSettings = new HandBoundsSettings();

    // Зовнішні залежності
    private Linear3DHandLayoutSettings layoutSettings;
    private Camera targetCamera;
    private Transform cardsContainer;

    // Runtime контроли
    [Header("Runtime Controls")]
    public bool EnableRuntimeAdjustment = true;
    [SerializeField] private KeyCode increaseWidthKey = KeyCode.Plus;
    [SerializeField] private KeyCode decreaseWidthKey = KeyCode.Minus;
    [SerializeField] private float adjustmentStep = 0.1f;

    private bool isVisible = true;

    #region Initialization

    public void Initialize(Linear3DHandLayoutSettings settings, Transform container, Camera camera) {
        layoutSettings = settings;
        cardsContainer = container;
        targetCamera = camera ?? Camera.main;

        isVisible = boundsSettings.ShowBounds;
    }

    private void Start() {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (cardsContainer == null)
            cardsContainer = transform;
    }

    #endregion

    #region Unity Lifecycle

    private void Update() {
        if (EnableRuntimeAdjustment && layoutSettings != null) {
            HandleRuntimeAdjustment();
        }
    }

    private void HandleRuntimeAdjustment() {
        bool changed = false;

        if (Input.GetKeyDown(increaseWidthKey)) {
            layoutSettings.MaxHandWidth += adjustmentStep;
            changed = true;
        } else if (Input.GetKeyDown(decreaseWidthKey)) {
            layoutSettings.MaxHandWidth = Mathf.Max(0.1f, layoutSettings.MaxHandWidth - adjustmentStep);
            changed = true;
        }

        if (changed) {
            Debug.Log($"Hand Width adjusted to: {layoutSettings.MaxHandWidth:F2}");
            // Повідомляємо layout про зміни
            var layout = GetComponentInParent<Linear3DHandLayout>();
            layout?.ForceRecalculation();
        }
    }

    #endregion

    #region Gizmos Drawing

    private void OnDrawGizmos() {
        if (layoutSettings == null || !isVisible) return;

        if (boundsSettings.ShowBounds)
            DrawHandBounds();

        if (boundsSettings.ShowGrid)
            DrawGrid();

        if (boundsSettings.ShowCardPreview)
            DrawCardPreview();

        if (boundsSettings.ShowSafetyZones && targetCamera != null)
            DrawSafetyZones();
    }

    private void DrawHandBounds() {
        Gizmos.color = boundsSettings.BoundsColor;
        Vector3 center = GetContainerPosition();
        Vector3 size = new Vector3(layoutSettings.MaxHandWidth, 0.1f, 0.5f);

        Gizmos.DrawWireCube(center, size);

        // Додаткові візуальні маркери
        Vector3 leftEdge = center + Vector3.left * layoutSettings.MaxHandWidth * 0.5f;
        Vector3 rightEdge = center + Vector3.right * layoutSettings.MaxHandWidth * 0.5f;

        Gizmos.color = boundsSettings.BoundsColor * 1.5f;
        Gizmos.DrawLine(leftEdge + Vector3.up * 0.2f, leftEdge - Vector3.up * 0.2f);
        Gizmos.DrawLine(rightEdge + Vector3.up * 0.2f, rightEdge - Vector3.up * 0.2f);

#if UNITY_EDITOR
        Vector3 labelPos = center + Vector3.up * 0.3f;
        Handles.Label(labelPos, $"Width: {layoutSettings.MaxHandWidth:F2}");
#endif
    }

    private void DrawGrid() {
        Gizmos.color = boundsSettings.GridColor;
        Vector3 center = GetContainerPosition();
        float halfWidth = layoutSettings.MaxHandWidth * 0.5f;

        // Вертикальні лінії
        int gridLines = Mathf.FloorToInt(layoutSettings.MaxHandWidth / boundsSettings.GridCellSize);
        for (int i = 0; i <= gridLines; i++) {
            float x = -halfWidth + (i * boundsSettings.GridCellSize);
            Vector3 start = center + new Vector3(x, -0.1f, 0);
            Vector3 end = center + new Vector3(x, 0.1f, 0);
            Gizmos.DrawLine(start, end);
        }

        // Горизонтальні лінії
        for (int i = -1; i <= 1; i++) {
            float y = i * boundsSettings.GridCellSize * 0.5f;
            Vector3 start = center + new Vector3(-halfWidth, y, 0);
            Vector3 end = center + new Vector3(halfWidth, y, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    private void DrawCardPreview() {
        Gizmos.color = boundsSettings.CardPreviewColor;
        Vector3 center = GetContainerPosition();
        int cardCount = boundsSettings.PreviewCardCount;

        if (cardCount <= 1) {
            DrawSingleCardPreview(center);
        } else {
            DrawMultipleCardsPreview(center, cardCount);
        }
    }

    private void DrawSingleCardPreview(Vector3 center) {
        Vector3 cardPos = center + Vector3.up * layoutSettings.DefaultYPosition;
        Gizmos.DrawWireCube(cardPos, new Vector3(0.2f, 0.01f, 0.3f));
    }

    private void DrawMultipleCardsPreview(Vector3 center, int cardCount) {
        float scale = layoutSettings.GetScaleForCardCount(cardCount);
        float spacing = layoutSettings.GetSpacingForCardCount(cardCount);
        float startX = -((cardCount - 1) * spacing) * 0.5f;

        for (int i = 0; i < cardCount; i++) {
            float x = startX + (i * spacing);
            float rotationAngle = Mathf.Lerp(-layoutSettings.MaxRotationAngle,
                layoutSettings.MaxRotationAngle, (float)i / (cardCount - 1));

            Vector3 cardPos = center + new Vector3(x, layoutSettings.DefaultYPosition, 0);
            Vector3 cardSize = new Vector3(0.2f, 0.01f, 0.3f) * scale;

            // Малюємо карту з обертанням
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(cardPos, Quaternion.Euler(0, rotationAngle, 0), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, cardSize);
            Gizmos.matrix = oldMatrix;

#if UNITY_EDITOR
            if (scale < 1f) {
                Handles.Label(cardPos + Vector3.up * 0.15f, $"{scale:F1}x");
            }
#endif
        }
    }

    private void DrawSafetyZones() {
        if (targetCamera == null) return;

        Vector3 center = GetContainerPosition();
        Vector3 screenBounds = GetScreenBoundsInWorldSpace();

        float safeWidth = screenBounds.x - boundsSettings.SafetyMargin * 2f;

        float safeHeight = screenBounds.y - boundsSettings.SafetyMargin * 2f;

        Gizmos.color = boundsSettings.SafetyZoneColor;
        Gizmos.DrawWireCube(center, new Vector3(safeWidth, 0.05f, safeHeight));

        // Попередження про перевищення меж
        if (layoutSettings.MaxHandWidth > safeWidth) {
            Gizmos.color = Color.red;
#if UNITY_EDITOR
            Vector3 warningPos = center + Vector3.up * 0.5f;
            Handles.Label(warningPos, "⚠ HAND TOO WIDE!");
#endif
        }

#if UNITY_EDITOR
        // Показуємо рекомендовану ширину
        Vector3 infoPos = center - Vector3.up * 0.3f;
        Handles.Label(infoPos, $"Recommended: {safeWidth:F1}");
#endif
    }

    #endregion

    #region Utility Methods

    private Vector3 GetContainerPosition() {
        return cardsContainer != null ? cardsContainer.position : transform.position;
    }

    private Vector3 GetScreenBoundsInWorldSpace() {
        if (targetCamera == null) return Vector3.zero;

        Vector3 center = GetContainerPosition();
        float distance = Vector3.Distance(targetCamera.transform.position, center);

        float height = 2f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * targetCamera.aspect;

        return new Vector3(width, height, distance);
    }

    #endregion

    #region Public API

    public float GetRecommendedWidth() {
        if (targetCamera == null) return 3f;

        Vector3 screenBounds = GetScreenBoundsInWorldSpace();
        return Mathf.Max(0.1f, screenBounds.x - boundsSettings.SafetyMargin * 2f);
    }

    public void SetVisible(bool enable) {
        if (enable == isVisible) return;
        isVisible = enable;
        boundsSettings.ShowBounds = enable;
    }

    public void AdjustHandWidth(float delta) {
        if (layoutSettings != null) {
            layoutSettings.MaxHandWidth = Mathf.Max(0.1f, layoutSettings.MaxHandWidth + delta);
            var layout = GetComponentInParent<Linear3DHandLayout>();
            layout?.ForceRecalculation();
        }
    }

    public void SetHandWidth(float width) {
        if (layoutSettings != null) {
            layoutSettings.MaxHandWidth = Mathf.Max(0.1f, width);
            var layout = GetComponentInParent<Linear3DHandLayout>();
            layout?.ForceRecalculation();
        }
    }

    #endregion

#if UNITY_EDITOR
    [CustomEditor(typeof(HandBoundsVisualizer))]
    public class HandBoundsVisualizerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            HandBoundsVisualizer visualizer = (HandBoundsVisualizer)target;

            if (visualizer.layoutSettings == null) {
                EditorGUILayout.HelpBox("Layout Settings not initialized. Call Initialize() first.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fit to Screen")) {
                Undo.RecordObject(visualizer.layoutSettings, "Fit Hand to Screen");
                float recommendedWidth = visualizer.GetRecommendedWidth();
                visualizer.SetHandWidth(recommendedWidth);
                EditorUtility.SetDirty(visualizer.layoutSettings);
            }

            if (GUILayout.Button("Reset to Default")) {
                Undo.RecordObject(visualizer.layoutSettings, "Reset Hand Width");
                visualizer.SetHandWidth(3f);
                EditorUtility.SetDirty(visualizer.layoutSettings);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Wider (+0.5)")) {
                Undo.RecordObject(visualizer.layoutSettings, "Increase Hand Width");
                visualizer.AdjustHandWidth(0.5f);
                EditorUtility.SetDirty(visualizer.layoutSettings);
            }

            if (GUILayout.Button("Narrower (-0.5)")) {
                Undo.RecordObject(visualizer.layoutSettings, "Decrease Hand Width");
                visualizer.AdjustHandWidth(-0.5f);
                EditorUtility.SetDirty(visualizer.layoutSettings);
            }
            EditorGUILayout.EndHorizontal();

            // Інформаційна панель
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Information", EditorStyles.boldLabel);

            float current = visualizer.layoutSettings.MaxHandWidth;
            float recommended = visualizer.GetRecommendedWidth();

            EditorGUILayout.LabelField($"Current Width: {current:F2}", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Recommended Width: {recommended:F2}", EditorStyles.helpBox);

            if (current > recommended) {
                EditorGUILayout.HelpBox("Hand width exceeds safe screen bounds!", MessageType.Warning);
            } else {
                EditorGUILayout.HelpBox("Hand width is within safe bounds.", MessageType.Info);
            }

            // Тестування різної кількості карт
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Card Scale Testing", EditorStyles.boldLabel);
            for (int i = 3; i <= 12; i += 3) {
                float scale = visualizer.layoutSettings.GetScaleForCardCount(i);
                EditorGUILayout.LabelField($"{i} cards: scale {scale:F2}x");
            }
        }
    }
#endif
}