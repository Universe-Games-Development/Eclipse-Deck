using UnityEngine;

[CreateAssetMenu(fileName = "HandLayoutSettings", menuName = "Cards/3D Hand Layout Settings")]
public class Linear3DHandLayoutSettings : LayoutSettigs {
    [Header("Cards Positioning")]
    [Tooltip("����������� ������ ���� (� ��������� �����������)")]
    public float MaxHandWidth = 3.0f;

    [Tooltip("³����� �� ������� ��� ����������� �����������")]
    public float CardSpacing = 0.2f;

    [Tooltip("������� Y-������� ���� � ����")]
    public float DepthOffset = 0.0f;

    public float VerticalOffset = 0.01f;

    [Header("Rotation Settings")]
    [Tooltip("������������ ���� �������� ������ ����")]
    [Range(0f, 45f)]
    public float MaxRotationAngle = 15.0f;

    [Tooltip("�������� ������� �������� ��� ���������� ����������")]
    [Range(0f, 5f)]
    public float RotationOffset = 1.0f;

    [Header("Advanced Settings")]
    [Tooltip("����������� ���������� ����, ��� ����� ��������������� ����� ������")]
    public int MaxCardsAtFullWidth = 7;

    public float PositionVariation { get; internal set; }

    // ������ ��� ��������� ��������� ����������� ������� �� ����� ����
    public float GetScaleForCardCount(int cardCount) {
        if (!ScaleCardsWhenCrowded || cardCount <= MaxCardsAtFullWidth) {
            return 1.0f;
        }

        float t = Mathf.Clamp01((float)(cardCount - MaxCardsAtFullWidth) / 10f);
        return Mathf.Lerp(1.0f, MinCardScale, t);
    }

    // ��� ����� ����� �� ��������������� � ���� �����, ��� ��������� ��� �������� ��������
    [System.Obsolete("Use new compression logic in Linear3DLayout instead")]
    public float GetSpacingForCardCount(int cardCount) {
        float baseSpacing = MaxHandWidth / Mathf.Max(1, cardCount - 1);
        float scale = GetScaleForCardCount(cardCount);
        return baseSpacing * Mathf.Lerp(1.0f, 0.8f, 1f - scale);
    }

    // ����� ����� ��� ��������� �������� ������ ����� � ����������� �������������
    public float GetEffectiveCardWidth(int cardCount) {
        float scale = GetScaleForCardCount(cardCount);
        return CardWidth * scale;
    }
}

public class LayoutSettigs : ScriptableObject {
    public float CardWidth = 1.0f; // �������� �� ���� ������������ ��������
    public float CardHeight = 1.4f; // ����������� ��������

    [Tooltip("������������� ���� ��� ������ �������")]
    public bool ScaleCardsWhenCrowded = true;

    [Tooltip("̳�������� ������� ����� ��� ������ �������")]
    [Range(0.5f, 1f)]
    public float MinCardScale = 0.8f;
}