using UnityEngine;
using UnityEditor;

public class HandBoundsVisualizer : MonoBehaviour {
    [SerializeField] private Linear3DHandLayoutSettings layoutSettings;
    [SerializeField] private HandBoundsSettings boundsSettings = new HandBoundsSettings();
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform cardsContainer;

    // Для настройки в реальном времени
    [Header("Runtime Adjustment")]
    [Tooltip("Настройка ширины руки в реальном времени")]
    public bool EnableRuntimeAdjustment = true;

    [SerializeField] private KeyCode increaseWidthKey = KeyCode.Plus;
    [SerializeField] private KeyCode decreaseWidthKey = KeyCode.Minus;
    [SerializeField] private float adjustmentStep = 0.1f;

    private void Start() {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (cardsContainer == null)
            cardsContainer = transform;
    }

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
        }
    }

    private void OnDrawGizmos() {
        if (layoutSettings == null || !boundsSettings.ShowBounds)
            return;

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

        Vector3 center = cardsContainer != null ? cardsContainer.position : transform.position;
        Vector3 size = new Vector3(layoutSettings.MaxHandWidth, 0.1f, layoutSettings.CardThickness * 2f);

        // Рисуем основную границу руки
        Gizmos.DrawWireCube(center, size);

        // Рисуем дополнительные линии для лучшей видимости
        Vector3 leftEdge = center + Vector3.left * layoutSettings.MaxHandWidth * 0.5f;
        Vector3 rightEdge = center + Vector3.right * layoutSettings.MaxHandWidth * 0.5f;

        Gizmos.color = boundsSettings.BoundsColor * 1.5f;
        Gizmos.DrawLine(leftEdge + Vector3.up * 0.2f, leftEdge - Vector3.up * 0.2f);
        Gizmos.DrawLine(rightEdge + Vector3.up * 0.2f, rightEdge - Vector3.up * 0.2f);

        // Подпись с размерами
        Vector3 labelPos = center + Vector3.up * 0.3f;
#if UNITY_EDITOR
        UnityEditor.Handles.Label(labelPos, $"Width: {layoutSettings.MaxHandWidth:F2}");
#endif
    }

    private void DrawGrid() {
        Gizmos.color = boundsSettings.GridColor;

        Vector3 center = cardsContainer != null ? cardsContainer.position : transform.position;
        float halfWidth = layoutSettings.MaxHandWidth * 0.5f;

        // Вертикальные линии сетки
        float gridLines = Mathf.Floor(layoutSettings.MaxHandWidth / boundsSettings.GridCellSize);
        for (int i = 0; i <= gridLines; i++) {
            float x = -halfWidth + (i * boundsSettings.GridCellSize);
            Vector3 start = center + new Vector3(x, -0.1f, 0);
            Vector3 end = center + new Vector3(x, 0.1f, 0);
            Gizmos.DrawLine(start, end);
        }

        // Горизонтальные линии сетки (меньше)
        for (int i = -1; i <= 1; i++) {
            float y = i * boundsSettings.GridCellSize * 0.5f;
            Vector3 start = center + new Vector3(-halfWidth, y, 0);
            Vector3 end = center + new Vector3(halfWidth, y, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    private void DrawCardPreview() {
        Gizmos.color = boundsSettings.CardPreviewColor;

        Vector3 center = cardsContainer != null ? cardsContainer.position : transform.position;
        int cardCount = boundsSettings.PreviewCardCount;

        if (cardCount <= 1) {
            // Одна карта в центре
            Vector3 cardPos = center + Vector3.up * layoutSettings.DefaultYPosition;
            Gizmos.DrawWireCube(cardPos, new Vector3(0.2f, 0.01f, 0.3f));
        } else {
            // Множество карт
            float spacing = layoutSettings.MaxHandWidth / (cardCount - 1);
            float startX = -layoutSettings.MaxHandWidth * 0.5f;

            for (int i = 0; i < cardCount; i++) {
                float x = startX + (i * spacing);
                float rotationAngle = Mathf.Lerp(-layoutSettings.MaxRotationAngle, layoutSettings.MaxRotationAngle,
                    cardCount > 1 ? (float)i / (cardCount - 1) : 0f);

                Vector3 cardPos = center + new Vector3(x, layoutSettings.DefaultYPosition, 0);

                // Рисуем карту с поворотом
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(cardPos, Quaternion.Euler(0, rotationAngle, 0), Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.2f, 0.01f, 0.3f));
                Gizmos.matrix = oldMatrix;

                // Номер карты
#if UNITY_EDITOR
                UnityEditor.Handles.Label(cardPos + Vector3.up * 0.2f, $"{i + 1}");
#endif
            }
        }
    }

    private void DrawSafetyZones() {
        if (targetCamera == null) return;

        Gizmos.color = boundsSettings.SafetyZoneColor;

        Vector3 center = cardsContainer != null ? cardsContainer.position : transform.position;

        // Получаем границы экрана в world space
        Vector3 screenBounds = GetScreenBoundsInWorldSpace();

        // Рисуем безопасные зоны
        float safeWidth = screenBounds.x - boundsSettings.SafetyMargin * 2f;
        Vector3 safeZoneSize = new Vector3(safeWidth, 0.05f, 0.1f);

        Gizmos.DrawWireCube(center, safeZoneSize);

        // Предупреждение если рука выходит за безопасную зону
        if (layoutSettings.MaxHandWidth > safeWidth) {
            Gizmos.color = Color.red;
#if UNITY_EDITOR
            Vector3 warningPos = center + Vector3.up * 0.5f;
            UnityEditor.Handles.Label(warningPos, "?? HAND TOO WIDE!");
#endif
        }
    }

    private Vector3 GetScreenBoundsInWorldSpace() {
        if (targetCamera == null) return Vector3.zero;

        Vector3 center = cardsContainer != null ? cardsContainer.position : transform.position;
        float distance = Vector3.Distance(targetCamera.transform.position, center);

        float height = 2f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * targetCamera.aspect;

        return new Vector3(width, height, distance);
    }

    // Методы для программного управления
    public void AdjustHandWidth(float delta) {
        if (layoutSettings != null) {
            layoutSettings.MaxHandWidth = Mathf.Max(0.1f, layoutSettings.MaxHandWidth + delta);
        }
    }

    public void SetHandWidth(float width) {
        if (layoutSettings != null) {
            layoutSettings.MaxHandWidth = Mathf.Max(0.1f, width);
        }
    }

    public float GetRecommendedWidth() {
        if (targetCamera == null) return 3f;

        Vector3 screenBounds = GetScreenBoundsInWorldSpace();
        return screenBounds.x - boundsSettings.SafetyMargin * 2f;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HandBoundsVisualizer))]
    public class HandBoundsVisualizerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            HandBoundsVisualizer visualizer = (HandBoundsVisualizer)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fit to Screen")) {
                float recommendedWidth = visualizer.GetRecommendedWidth();
                visualizer.SetHandWidth(recommendedWidth);
                Debug.Log($"Hand width set to recommended: {recommendedWidth:F2}");
            }

            if (GUILayout.Button("Reset Width")) {
                visualizer.SetHandWidth(3f);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Wider (+0.5)")) {
                visualizer.AdjustHandWidth(0.5f);
            }

            if (GUILayout.Button("Narrower (-0.5)")) {
                visualizer.AdjustHandWidth(-0.5f);
            }
            EditorGUILayout.EndHorizontal();

            if (visualizer.layoutSettings != null) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Current Width: {visualizer.layoutSettings.MaxHandWidth:F2}", EditorStyles.helpBox);

                float recommended = visualizer.GetRecommendedWidth();
                EditorGUILayout.LabelField($"Recommended Width: {recommended:F2}", EditorStyles.helpBox);

                if (visualizer.layoutSettings.MaxHandWidth > recommended) {
                    EditorGUILayout.HelpBox("Hand width exceeds safe screen bounds!", MessageType.Warning);
                }
            }
        }
    }
#endif
}