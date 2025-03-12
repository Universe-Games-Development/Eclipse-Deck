using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {

    [SerializeField] private GameObject worldSpaceCanvasPrefab; // ������ World Space Canvas
    [SerializeField] private GameObject textPrefab; // ������ ������

    public Canvas WorldSpaceCanvas { get; private set; }
    public Action<string> OnInfoRequested;

    private List<GameObject> activeTexts = new(); // ������ �������� �������
    private Camera mainCamera;


    private void Awake() {
        mainCamera= Camera.main;
        // ���������� �� ��� ���� World Space Canvas
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        WorldSpaceCanvas = canvases.FirstOrDefault(canvas => canvas.renderMode == RenderMode.WorldSpace);

        if (WorldSpaceCanvas == null || WorldSpaceCanvas.renderMode != RenderMode.WorldSpace) {
            // ���� ����, ��������� ����
            GameObject canvasObject = Instantiate(worldSpaceCanvasPrefab);
            WorldSpaceCanvas = canvasObject.GetComponent<Canvas>();
        }
    }

    public void ShowTextAt(string message, Vector3 position) {
        if (textPrefab == null) {
            Debug.LogError("Text prefab is not assigned!");
            return;
        }

        GameObject textObject = Instantiate(textPrefab, WorldSpaceCanvas.transform);
        textObject.transform.position = position;
        Quaternion quaternion = Quaternion.LookRotation(textObject.transform.position - mainCamera.transform.position);
        textObject.transform.rotation = Quaternion.Euler(90, 90, 0);

        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        if (textComponent != null) {
            textComponent.text = message;
        }

        activeTexts.Add(textObject);
    }

    public void ShowTip(string info) {
        OnInfoRequested?.Invoke(info);
    }
}
