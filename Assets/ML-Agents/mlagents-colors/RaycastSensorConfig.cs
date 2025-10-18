using UnityEngine;

[CreateAssetMenu(fileName = "VisionConfig", menuName = "AI/Vision Config")]
public class VisionConfig : ScriptableObject {
    [Header("Ray Counts")]
    [Min(0)] public int frontRays = 5;
    [Min(0)] public int backRays = 3;
    [Min(0)] public int sideRays = 2;

    [Header("Angles (degrees)")]
    [Range(0f, 180f)] public float frontAngle = 60f;
    [Range(0f, 180f)] public float backAngle = 60f;
    [Range(0f, 180f)] public float sideAngle = 45f;

    [Header("Ray Lengths")]
    [Min(0.1f)] public float frontRayLength = 5f;
    [Min(0.1f)] public float backRayLength = 3f;
    [Min(0.1f)] public float sideRayLength = 4f;

    [Header("Layers")]
    public LayerMask obstacleMask = ~0;

    [Header("Performance")]
    [Tooltip("Використовувати QueryTriggerInteraction для raycast")]
    public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Debug Colors")]
    [Tooltip("Колір променя який нічого не зачепив")]
    public Color debugFreeColor = new Color(0.2f, 1f, 0.2f, 1f); // Зелений

    [Tooltip("Колір для далеких перешкод (на максимальній відстані)")]
    public Color debugFarColor = new Color(1f, 1f, 1f, 1f); // Білий

    [Tooltip("Колір для близьких перешкод (дуже близько)")]
    public Color debugCloseColor = new Color(1f, 0f, 0f, 1f); // Червоний
    public bool showBestColorMatch;
    public bool useColorSimilarityForDebug;

    /// <summary>
    /// Загальна кількість променів
    /// </summary>
    public int TotalRayCount => frontRays + backRays + (sideRays * 2);

    /// <summary>
    /// Середня довжина променів (для зворотної сумісності)
    /// </summary>
    public float AverageRayLength => (frontRayLength + backRayLength + sideRayLength * 2) / 4f;

    /// <summary>
    /// Обчислює колір для відстані (для використання поза компонентом)
    /// </summary>
    public Color GetColorForDistance(float distance, float rayLength) {
        float normalizedDistance = Mathf.Clamp01(distance / rayLength);
        float intensity = 1f - normalizedDistance;
        return Color.Lerp(debugFarColor, debugCloseColor, intensity);
    }

    /// <summary>
    /// Обчислює колір для нормалізованої відстані (0-1)
    /// </summary>
    public Color GetColorForNormalizedDistance(float normalizedDistance) {
        float intensity = 1f - Mathf.Clamp01(normalizedDistance);
        return Color.Lerp(debugFarColor, debugCloseColor, intensity);
    }

    private void OnValidate() {
        // Ensure values are reasonable
        frontRayLength = Mathf.Max(0.1f, frontRayLength);
        backRayLength = Mathf.Max(0.1f, backRayLength);
        sideRayLength = Mathf.Max(0.1f, sideRayLength);

        if (TotalRayCount > 100) {
            Debug.LogWarning($"VisionConfig '{name}': Велика кількість променів ({TotalRayCount}). Це може вплинути на продуктивність.");
        }

        // Ensure colors have alpha = 1 for better visibility
        debugFreeColor.a = 1f;
        debugFarColor.a = 1f;
        debugCloseColor.a = 1f;
    }

    [ContextMenu("Reset to Default Colors")]
    private void ResetColors() {
        debugFreeColor = new Color(0.2f, 1f, 0.2f, 1f);
        debugFarColor = new Color(1f, 1f, 1f, 1f);
        debugCloseColor = new Color(1f, 0f, 0f, 1f);
    }

    [ContextMenu("Set Alternative Color Scheme (Yellow-Red)")]
    private void SetAlternativeColors() {
        debugFreeColor = new Color(0.5f, 0.5f, 1f, 1f); // Блакитний
        debugFarColor = new Color(1f, 1f, 0f, 1f); // Жовтий
        debugCloseColor = new Color(1f, 0.3f, 0f, 1f); // Помаранчево-червоний
    }
}