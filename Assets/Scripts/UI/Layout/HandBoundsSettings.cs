using UnityEngine;

[System.Serializable]
public class HandBoundsSettings {
    [Header("Bounds Visualization")]
    [Tooltip("�������� ������� ���� � �����")]
    public bool ShowBounds = true;

    [Tooltip("���� ������ ����")]
    public Color BoundsColor = Color.green;

    [Tooltip("������� ����� ������")]
    [Range(1f, 10f)]
    public float BoundsLineWidth = 2f;

    [Header("Grid Settings")]
    [Tooltip("�������� ����� ������ ������")]
    public bool ShowGrid = true;

    [Tooltip("���� �����")]
    public Color GridColor = Color.gray;

    [Tooltip("������ ������ �����")]
    [Range(0.1f, 1f)]
    public float GridCellSize = 0.5f;

    [Header("Card Preview")]
    [Tooltip("�������� ��������������� �������� ������������ ����")]
    public bool ShowCardPreview = true;

    [Tooltip("���� ���������������� ��������� ����")]
    public Color CardPreviewColor = Color.yellow;

    [Tooltip("���������� ���� ��� ���������������� ���������")]
    [Range(1, 15)]
    public int PreviewCardCount = 5;

    [Header("Safety Zones")]
    [Tooltip("�������� ���������� ����")]
    public bool ShowSafetyZones = true;

    [Tooltip("���� ���������� ���")]
    public Color SafetyZoneColor = Color.red;

    [Tooltip("������ �� ������ ������")]
    [Range(0.1f, 2f)]
    public float SafetyMargin = 0.5f;
}
