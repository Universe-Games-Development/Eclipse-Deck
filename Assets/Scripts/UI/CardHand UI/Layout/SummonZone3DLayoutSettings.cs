using UnityEngine;

[CreateAssetMenu(fileName = "SummonZoneLayoutSettings", menuName = "Cards/3D Summon Zone Layout Settings")]
public class SummonZone3DLayoutSettings : LayoutSettigs {
    [Header("Summon Zone Positioning")]
    [Tooltip("³����� �� ������� � ��� �������")]
    public float CardSpacing = 0.2f;

    [Tooltip("����������� ������� ���� � ������ ����")]
    public int MaxCardsPerRow = 6;

    [Tooltip("³����� �� ������")]
    public float RowSpacing = 1.5f;

    [Tooltip("�� ��������������� ������� ���� ��� ������ ������� ����")]
    public bool UseMultipleRows = true;
}
