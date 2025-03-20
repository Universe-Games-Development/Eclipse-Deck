using System.Linq;
using TMPro;
using UnityEngine;

public class TextSpawner : MonoBehaviour {
    [SerializeField] private GameObject worldSpaceCanvasPrefab; // Префаб World Space Canvas
    [SerializeField] private GameObject textPrefab; // Префаб тексту
    public Canvas WorldSpaceCanvas { get; private set; }
    private Camera mainCamera;

    private void Awake() {
        mainCamera = Camera.main;
        // Перевіряємо чи вже існує World Space Canvas
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        WorldSpaceCanvas = canvases.FirstOrDefault(canvas => canvas.renderMode == RenderMode.WorldSpace);

        if (WorldSpaceCanvas == null || WorldSpaceCanvas.renderMode != RenderMode.WorldSpace) {
            // Якщо немає, створюємо його
            GameObject canvasObject = Instantiate(worldSpaceCanvasPrefab);
            WorldSpaceCanvas = canvasObject.GetComponent<Canvas>();
        }
    }

    public TextMeshProUGUI CreateTextAt(string message, Vector3 position) {
        if (textPrefab == null) {
            Debug.LogError("Text prefab is not assigned!");
            return null;
        }

        GameObject textObject = Instantiate(textPrefab, WorldSpaceCanvas.transform);
        textObject.transform.position = position;
        Quaternion quaternion = Quaternion.LookRotation(textObject.transform.position - mainCamera.transform.position);
        textObject.transform.rotation = Quaternion.Euler(90, 90, 0);

        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        if (textComponent != null) {
            textComponent.text = message;
        }
        return textComponent;
    }

    internal void RemoveText(TextMeshProUGUI text) {
        Destroy(text);
    }
}
