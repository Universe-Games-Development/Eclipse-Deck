using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour {
    [SerializeField] private Camera uiRenderCamera;
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private LayerMask uiMask;
    [SerializeField] private RenderTexture renderTexture; // Screen UI simulation texture
    public Image debugPoint; // UI Debug pointer Image to see where is pointer on real UI

    private PointerEventData pointerData;
    private Vector2 imitationUISize;

    private void Update() {
        // ������� ��� �� �������� ������
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, uiMask)) {
            // ����������� ����� ��������� � UV
            if (hit.collider is MeshCollider) {
                Vector2 uv = hit.textureCoord;

                // ����������� UV � ���������� Canvas
                Vector2 canvasPoint = GetTexturePointToCanvasPoint(uv);

                // ³�������� �����: ��������� ������� debugPoint
                UpdateDebugPoint(canvasPoint);

                // �������������� PointerEventData
                pointerData = new PointerEventData(EventSystem.current) {
                    position = uiRenderCamera.WorldToScreenPoint(canvasPoint),
                    delta = Input.mouseScrollDelta, // ��� ��������� ���������
                    button = PointerEventData.InputButton.Left // ����� ������ ����
                };

            }
        }
    }

    private void UpdateDebugPoint(Vector2 canvasPoint) {
        // ��������� ������� ���������� Canvas � RectTransform debugPoint
        RectTransform rectTransform = debugPoint.GetComponent<RectTransform>();
        if (rectTransform != null) {
            rectTransform.anchoredPosition = canvasPoint;
        }

        // ������ debugPoint �������
        if (!debugPoint.gameObject.activeSelf) {
            debugPoint.gameObject.SetActive(true);
        }
    }

    // Returns associated point of screen texture to canvas sizes
    private Vector2 GetTexturePointToCanvasPoint(Vector2 uvCoordinates) {
        Vector2 textureCoordinates = new Vector2(renderTexture.width * uvCoordinates.x, renderTexture.height * uvCoordinates.y);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiCanvas.transform as RectTransform,
            textureCoordinates,
            uiRenderCamera,
            out Vector2 canvasPoint
        );
        return canvasPoint;
    }
}
