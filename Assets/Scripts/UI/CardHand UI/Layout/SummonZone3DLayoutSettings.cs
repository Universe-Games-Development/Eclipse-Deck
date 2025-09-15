using UnityEngine;

[CreateAssetMenu(fileName = "SummonZoneLayoutSettings", menuName = "Cards/3D Summon Zone Layout Settings")]
public class SummonZone3DLayoutSettings : LayoutSettigs {
    [Header("Summon Zone Positioning")]
    [Tooltip("Відступ між картами в зоні виклику")]
    public float CardSpacing = 0.2f;

    [Tooltip("Максимальна кількість карт в одному ряду")]
    public int MaxCardsPerRow = 6;

    [Tooltip("Відступ між рядами")]
    public float RowSpacing = 1.5f;

    [Tooltip("Чи використовувати декілька рядів при великій кількості карт")]
    public bool UseMultipleRows = true;
}
