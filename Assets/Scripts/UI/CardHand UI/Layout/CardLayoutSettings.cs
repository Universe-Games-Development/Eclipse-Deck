using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "HandLayoutSettings", menuName = "Cards/3D Hand Layout Settings")]
public class Linear3DHandLayoutSettings : ScriptableObject {
    [Header("Cards Positioning")]
    [Tooltip("������������ ������ ���� (� ��������� �����������)")]
    public float MaxHandWidth = 3.0f;

    [Tooltip("������� ����� ��� ������� �������������")]
    public float CardThickness = 0.1f;

    [Tooltip("������� Y-������� ���� � ����")]
    public float DefaultYPosition = 0.0f;

    [Tooltip("������������ �������� ����� ������� ��� �������� ������� �������")]
    public float VerticalOffset = 0.01f;

    [Tooltip("��������� �������� ������� ��� ����������� ������������")]
    public float PositionVariation = 0.02f;

    [Header("Hover Settings")]
    [Tooltip("������ ������� ����� ��� ���������")]
    public float HoverHeight = 0.3f;

    [Header("Rotation Settings")]
    [Tooltip("������������ ���� �������� ������� ����")]
    [Range(0f, 45f)]
    public float MaxRotationAngle = 15.0f;

    [Tooltip("��������� �������� �������� ��� ����������� ������������")]
    [Range(0f, 5f)]
    public float RotationOffset = 1.0f;

    [Header("Animation Settings")]
    [Tooltip("������������ �������� �����������")]
    public float MoveDuration = 0.3f;

    [Tooltip("������������ �������� ����������� ��� ���������")]
    public float HoverMoveDuration = 0.2f;

    [Tooltip("������������ �������� ��������")]
    public float RotationDuration = 0.3f;

    [Tooltip("������ �������� ��� ��������")]
    public Ease MoveEase = Ease.OutQuad;

    [Tooltip("������ �������� ��� ��������")]
    public Ease RotationEase = Ease.OutQuad;

    [Header("Advanced Settings")]
    [Tooltip("������������ ���������� ����, ��� ������� ������������ ������ ������")]
    public int MaxCardsAtFullWidth = 7;

    [Tooltip("��������������� ���� ��� ������� ����������")]
    public bool ScaleCardsWhenCrowded = true;

    [Tooltip("����������� ������� ����� ��� ������� ����������")]
    [Range(0.5f, 1f)]
    public float MinCardScale = 0.8f;

    // ������ ��� ��������� ������������ �������� � ����������� �� ����� ����
    public float GetScaleForCardCount(int cardCount) {
        if (!ScaleCardsWhenCrowded || cardCount <= MaxCardsAtFullWidth) {
            return 1.0f;
        }

        float t = Mathf.Clamp01((float)(cardCount - MaxCardsAtFullWidth) / 10f);
        return Mathf.Lerp(1.0f, MinCardScale, t);
    }

    public float GetSpacingForCardCount(int cardCount) {
        float baseSpacing = MaxHandWidth / Mathf.Max(1, cardCount - 1);
        float scale = GetScaleForCardCount(cardCount);

        // ��� ���������� �������� ����� ������� ��������� ���������� ����� �������
        return baseSpacing * Mathf.Lerp(1.0f, 0.8f, 1f - scale);
    }
}