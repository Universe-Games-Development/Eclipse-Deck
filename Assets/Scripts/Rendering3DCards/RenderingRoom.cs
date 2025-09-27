using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ����������� "������" ��� ���������� ����
/// </summary>
public class RenderingRoom : MonoBehaviour {
    [SerializeField] private Camera renderCamera;
    [SerializeField] private Canvas renderCanvas;
    [SerializeField] private CardUIView cardUIPrefab;

    [SerializeField] RectTransform uiContainer;

    [SerializeField] RenderTexture renderTexture;

    private List<CardUIView> activeCards = new List<CardUIView>();
    private Dictionary<CardUIView, Texture2D> cardTextures = new Dictionary<CardUIView, Texture2D>();

    private void Awake() {
        renderCamera.targetTexture = renderTexture;
        renderCanvas.worldCamera = renderCamera;
    }


    /// <summary>
    /// ������� ���� UI-����� � ��� ����� ����������
    /// </summary>
    public CardUIView CreateCardUI() {
        CardUIView newCardUI = Instantiate(cardUIPrefab, renderCanvas.transform);
        RectTransform rectTransform = newCardUI.RectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        activeCards.Add(newCardUI);
        return newCardUI;
    }

    /// <summary>
    /// ���� ������� UI-����� � �� ������
    /// </summary>
    public void AddCardUI(CardUIView cardUI) {
        cardUI.transform.SetParent(renderCanvas.transform);

        cardUI.gameObject.layer = renderCanvas.gameObject.layer;
        activeCards.Add(cardUI);
    }

    /// <summary>
    /// ������� UI-����� � ���� ������
    /// </summary>
    public void RemoveCardUI(CardUIView cardUI) {
        // �� ������� GameObject, ������� ���� ����� ���������� � ���� ������
        if (activeCards.Remove(cardUI)) {
        }
    }

    /// <summary>
    /// ��������� �������� ��� ������� UI-�����
    /// </summary>
    public Texture2D RenderCardTexture(CardUIView cardUI) {
        if (!activeCards.Contains(cardUI))
            return null;

        foreach (var card in activeCards) {
            card.gameObject.SetActive(false);
        }

        cardUI.gameObject.SetActive(true);
        cardUI.RectTransform.anchoredPosition = Vector2.zero;

        // ������ �� ������� ����������/������������ ������!
        renderCamera.Render();

        Texture2D resultTexture;
        if (!cardTextures.TryGetValue(cardUI, out resultTexture)) {
            resultTexture = CreateTextureFromTemplate();
            resultTexture.name = $"CardTexture_{GetInstanceID()}";
            cardTextures[cardUI] = resultTexture;
        }

        RenderTexture.active = renderTexture;
        resultTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        resultTexture.Apply();
        RenderTexture.active = null;

        return resultTexture;
    }

    /// <summary>
    /// ������� ��������� �������� ��� �����
    /// </summary>
    public Texture2D GetTextureFor(CardUIView cardUI) {
        if (cardTextures.TryGetValue(cardUI, out Texture2D texture))
            return texture;

        // ���� �������� ����, ��������� ��
        return RenderCardTexture(cardUI);
    }

    private Texture2D CreateTextureFromTemplate() {
        return new Texture2D(
            renderTexture.width,
            renderTexture.height,
            TextureFormat.RGBA32,
            false
        );
    }


    private void OnDestroy() {
        // ��������� �������
        if (renderTexture != null)
            renderTexture.Release();
    }
}