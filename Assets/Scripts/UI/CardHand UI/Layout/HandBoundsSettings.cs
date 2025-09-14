using UnityEngine;

[System.Serializable]
public class HandBoundsSettings {
    [Header("Bounds Visualization")]
    [Tooltip("Показать границы руки в сцене")]
    public bool ShowBounds = true;

    [Tooltip("Цвет границ руки")]
    public Color BoundsColor = Color.green;

    [Tooltip("Толщина линий границ")]
    [Range(1f, 10f)]
    public float BoundsLineWidth = 2f;

    [Header("Grid Settings")]
    [Tooltip("Показать сетку внутри границ")]
    public bool ShowGrid = true;

    [Tooltip("Цвет сетки")]
    public Color GridColor = Color.gray;

    [Tooltip("Размер клеток сетки")]
    [Range(0.1f, 1f)]
    public float GridCellSize = 0.5f;

    [Header("Card Preview")]
    [Tooltip("Показать предварительный просмотр расположения карт")]
    public bool ShowCardPreview = true;

    [Tooltip("Цвет предварительного просмотра карт")]
    public Color CardPreviewColor = Color.yellow;

    [Tooltip("Количество карт для предварительного просмотра")]
    [Range(1, 15)]
    public int PreviewCardCount = 5;

    [Header("Safety Zones")]
    [Tooltip("Показать безопасные зоны")]
    public bool ShowSafetyZones = true;

    [Tooltip("Цвет безопасных зон")]
    public Color SafetyZoneColor = Color.red;

    [Tooltip("Отступ от границ экрана")]
    [Range(0.1f, 2f)]
    public float SafetyMargin = 0.5f;
}
