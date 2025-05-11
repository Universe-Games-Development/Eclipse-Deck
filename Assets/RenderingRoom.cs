using NUnit.Framework;
using System;
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

    [SerializeField] RenderTexture renderTextureTemplate;
    private RenderTexture renderTexture;

    private List<CardUIView> activeCards = new List<CardUIView>();
    private Dictionary<CardUIView, Texture2D> cardTextures = new Dictionary<CardUIView, Texture2D>();

    private void Awake() {
        // Ініціалізуємо RenderTexture для камери
        renderTexture = new RenderTexture(renderTextureTemplate);
        renderCamera.targetTexture = renderTexture;

        // Встановлюємо Canvas для відображення в нашій камері
        renderCanvas.worldCamera = renderCamera;

        // Приховуємо об'єкти від гравця
        renderCamera.gameObject.SetActive(false);
        renderCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Створює нову UI-карту в цій кімнаті рендерингу
    /// </summary>
    public CardUIView CreateCardUI() {
        CardUIView newCardUI = Instantiate(cardUIPrefab, renderCanvas.transform);
        RectTransform rectTransform = newCardUI.RectTransform;
        rectTransform.anchorMin = Vector2.zero; // Нижній лівий кут (0,0)
        rectTransform.anchorMax = Vector2.one;  // Верхній правий кут (1,1)
        rectTransform.offsetMin = Vector2.zero; // Виправлення нижнього краю
        rectTransform.offsetMax = Vector2.zero; // Виправлення верхнього краю
        activeCards.Add(newCardUI);
        return newCardUI;
    }

    /// <summary>
    /// Додає існуючу UI-карту в цю кімнату
    /// </summary>
    public void AddCardUI(CardUIView cardUI) {
        cardUI.transform.SetParent(renderCanvas.transform);
        activeCards.Add(cardUI);
    }

    /// <summary>
    /// Видаляє UI-карту з цієї кімнати
    /// </summary>
    public void RemoveCardUI(CardUIView cardUI) {
        activeCards.Remove(cardUI);
        // Не знищуємо GameObject, оскільки його можна перемістити в іншу кімнату
        cardUI.gameObject.SetActive(false);
    }

    /// <summary>
    /// Рендерить текстуру для вказаної UI-карти
    /// </summary>
    public Texture2D RenderCardTexture(CardUIView cardUI) {
        // Перевіряємо, чи карта в нашій кімнаті
        if (!activeCards.Contains(cardUI))
            return null;

        // Приховуємо всі карти
        foreach (var card in activeCards) {
            card.gameObject.SetActive(false);
        }

        // Показуємо тільки потрібну картку і позиціонуємо її по центру
        cardUI.gameObject.SetActive(true);
        cardUI.RectTransform.anchoredPosition = Vector2.zero;

        
        // Активуємо камеру і канвас для рендеру
        renderCamera.gameObject.SetActive(true);
        renderCanvas.gameObject.SetActive(true);

        // Рендеримо сцену
        renderCamera.Render();

        // Створюємо або отримуємо Texture2D для цієї карти
        Texture2D resultTexture;
        if (!cardTextures.TryGetValue(cardUI, out resultTexture)) {
            resultTexture = CreateTextureFromTemplate();
            resultTexture.name = $"CardTexture_{GetInstanceID()}";
            cardTextures[cardUI] = resultTexture;
        }

        
        // Копіюємо пікселі з RenderTexture в Texture2D
        RenderTexture.active = renderTexture;
        Texture2D texture = resultTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;

        // Деактивуємо камеру і канвас після рендерингу
        renderCamera.gameObject.SetActive(false);
        renderCanvas.gameObject.SetActive(false);

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
            renderTextureTemplate.width,
            renderTextureTemplate.height,
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