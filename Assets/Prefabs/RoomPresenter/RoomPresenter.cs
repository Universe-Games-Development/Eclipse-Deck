using System;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class RoomPresenter : MonoBehaviour {
    [SerializeField] private MeshRenderer renderer;
    [SerializeField] private Material wrongMaterial;

    private DungeonNode self;
    private float spacing;

    public void Initialize(DungeonNode self, float spacing) {
        this.self = self;
        this.spacing = spacing;
    }

    public void MarkWrong() {
        renderer.material = wrongMaterial;
    }

    internal void AddConnection(DungeonNode connectedNode, Color color, float width) {
        Vector3 endPosition = new Vector3(connectedNode.position.x * spacing, 0, connectedNode.position.y * spacing);

        // Створюємо новий об'єкт для кожного LineRenderer
        GameObject lineObj = new GameObject("ConnectionLine");
        lineObj.transform.SetParent(transform); // Робимо дочірнім об'єктом для зручності
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

        if (lineRenderer == null) {
            Debug.LogError("Failed to add LineRenderer component");
            return;
        }

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, endPosition);

        // Безпечне встановлення матеріалу
        Material lineMaterial = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Standard"));
        if (lineMaterial != null) {
            lineRenderer.material = lineMaterial;
            lineRenderer.material.color = color;
        }
    }

    public void MarkWithColor(Color selfReferenceColor) {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", selfReferenceColor);
        renderer.SetPropertyBlock(block);
    }
}
