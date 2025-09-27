using UnityEngine;

[CreateAssetMenu(fileName = "LayoutSettings", menuName = "Layouts/Linear")]
public class LayoutSettings : ScriptableObject {
    [Header("Total Settings")]
    public float MaxTotalWidth = 3.0f;

    [Header ("Item settings")]
    public float ItemWidth = 1.0f;
    public float ItemLength = 1.4f;

    [Header ("Layout Settings")]
    public float DepthOffset = 0.0f;
    public float VerticalOffset = 0.01f;
    public float ItemSpacing = 0.2f;

    public float PositionVariation = 0.00f;

    public float RowSpacing = 0.2f;


    [Header("Rotation Settings")]
    [Range(0f, 45f)]
    public float MaxRotationAngle = 15.0f;

    [Range(0f, 5f)]
    public float RotationOffset = 1.0f;

    
}