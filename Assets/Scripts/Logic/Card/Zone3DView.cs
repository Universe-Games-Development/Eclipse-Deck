using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Zone3DView : MonoBehaviour {
    [SerializeField] TextMeshPro text;
    [SerializeField] float spacing = 1.5f;

    public void UpdateSummonedCount(int count) {
        text.text = $"Units: {count}";
    }

    public List<TransformPoint> GetCreaturePoints(int count) {
        List<TransformPoint> points = new();

        if (count == 0) return points;

        float startX = -((count - 1) * spacing) / 2;

        for (int i = 0; i < count; i++) {
            points.Add(new TransformPoint {
                position = transform.TransformPoint(new Vector3(startX + i * spacing, 0, 0)),
                rotation = transform.rotation,
                scale = Vector3.one
            });
        }

        return points;
    }

    public void Highlight(bool enable) {
        // Реалізація підсвічування
    }
}