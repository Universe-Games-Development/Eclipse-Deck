using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Представляє "кімнату" для рендерингу карт
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
    /// Створює нову UI-карту в цій кімнаті рендерингу
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
    /// Додає існуючу UI-карту в цю кімнату
    /// </summary>
    public void AddCardUI(CardUIView cardUI) {
        cardUI.transform.SetParent(renderCanvas.transform);

        cardUI.gameObject.layer = renderCanvas.gameObject.layer;
        activeCards.Add(cardUI);
    }

    /// <summary>
    /// Видаляє UI-карту з цієї кімнати
    /// </summary>
    public void RemoveCardUI(CardUIView cardUI) {
        // Не знищуємо GameObject, оскільки його можна перемістити в іншу кімнату
        if (activeCards.Remove(cardUI)) {
        }
    }

    /// <summary>
    /// Рендерить текстуру для вказаної UI-карти
    /// </summary>
    public Texture2D RenderCardTexture(CardUIView cardUI) {
        if (!activeCards.Contains(cardUI))
            return null;

        foreach (var card in activeCards) {
            card.gameObject.SetActive(false);
        }

        cardUI.gameObject.SetActive(true);
        cardUI.RectTransform.anchoredPosition = Vector2.zero;

        // Більше не потрібно активувати/деактивувати камеру!
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
    /// Повертає збережену текстуру для карти
    /// </summary>
    public Texture2D GetTextureFor(CardUIView cardUI) {
        if (cardTextures.TryGetValue(cardUI, out Texture2D texture))
            return texture;

        // Якщо текстури немає, рендеримо її
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
        // Звільняємо ресурси
        if (renderTexture != null)
            renderTexture.Release();
    }
}