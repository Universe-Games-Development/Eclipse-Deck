using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ZoneView : AreaView {
    [SerializeField] TextMeshPro text;
    [SerializeField] Transform _creaturesContainer;

    ILayout3DHandler _layout;
    [SerializeField] LayoutSettings settings;

    [SerializeField] Renderer zoneRenderer;
    [SerializeField] Color unAssignedColor;

    private void Awake() {
        if (settings == null) {
            Debug.LogWarning("Settings layout null for ", gameObject);
            return;
        }
        _layout = new Linear3DLayout(settings);

        if (zoneRenderer == null) {
            zoneRenderer = GetComponent<Renderer>();
        }
    }

    public void UpdateSummonedCount(int count) {
        text.text = $"Units: {count}";
    }

    public List<LayoutPoint> GetCreaturePoints(int count) {
        LayoutResult layoutResult = _layout.CalculateLayout(count);
        var transformedPoints = new List<LayoutPoint>();

        foreach (var point in layoutResult.Points) {
            // Створюємо новий об'єкт, не змінюємо оригінал
            Vector3 worldPosition = _creaturesContainer.TransformPoint(point.position);
            transformedPoints.Add(new LayoutPoint(worldPosition, point.rotation, point.orderIndex, point.rowIndex, point.columnIndex));
        }

        return transformedPoints;
    }

    public Vector3 CalculateSize(int creaturesCapacity) {
        float areaWidth = settings.ItemWidth;
        float areaLength = settings.ItemLength;

        float totalWidth = areaWidth * creaturesCapacity;
        float totalLength = areaLength;
        var layoutResult = _layout.CalculateLayout(creaturesCapacity);
        LayoutMetadata metadata = layoutResult.Metadata;
        totalLength = metadata.TotalLength;
        totalWidth = metadata.TotalWidth;

        Vector3 localScale = transform.localScale;
        Vector3 newScale = new Vector3(totalWidth, localScale.y, totalLength);
        SetScale(newScale);
        return newScale;
    }

    public void ChangeOwnerColor(Color newColor) {
        Color color = unAssignedColor;

        if (newColor != null) {
            color = newColor;
        }

        zoneRenderer.material.color = color;
    }
}
