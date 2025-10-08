using UnityEngine;

[CreateAssetMenu(fileName = "LinearLayoutSettings", menuName = "Layouts/Linear")]
public class LinearLayoutSettings : ScriptableObject {
    [Header("Total Settings")]
    public float MaxTotalWidth = 3.0f;
    [Header("Layout Settings")]
    public float DepthOffset = 0.0f;
    public float VerticalOffset = 0.01f;
    [Min(0)]
    public float ItemSpacing = 0.2f;
    [Header("Rotation Settings")]
    [Range(0f, 45f)]
    public float MaxRotationAngle = 15.0f;
    [Header("Compression Settings")]
    public CompressionMode compressionMode = CompressionMode.ReduceSpacing;
}

public enum CompressionMode {
    None,              // Не стискати, виходити за межі
    ReduceSpacing,     // Зменшити відступи між елементами
    CompressPositions  // Агресивне стискання (можливе накладання)
}
