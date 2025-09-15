using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Zone3DView : MonoBehaviour {
    [SerializeField] TextMeshPro text;
    [SerializeField] float spacing = 1.5f;
    [SerializeField] SummonZone3DLayoutSettings settings;
    [SerializeField] Transform creaturesContainer;

    ILayout3DHandler layout;
    private void Awake() {
        layout = new SummonZone3DLayout(settings);
    }

    public void UpdateSummonedCount(int count) {
        text.text = $"Units: {count}";
    }

    public List<LayoutPoint> GetCreaturePoints(int count) {
        
        List<LayoutPoint> points = layout.CalculateCardTransforms(count);

        for (int i = 0; i < points.Count; i++) {
            var point = points[i];

            Vector3 position = transform.position;
            Vector3 pointPosition = point.position;
            Vector3 result = creaturesContainer.TransformPoint(point.position);
            point.position = result;
            Debug.DrawRay(point.position, Vector3.up, Color.red, 5f);
        }

        return new(points);
    }

    public void Highlight(bool enable) {
        // Реалізація підсвічування
    }
}