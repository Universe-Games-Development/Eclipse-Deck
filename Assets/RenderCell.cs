using System;
using UnityEngine;

public class RenderCell : MonoBehaviour
{
    [SerializeField] Camera renderCamera; // Camera used to render the card
    [SerializeField] RectTransform uiContainer;
    [SerializeField] CardUIView cardUIPrefab; // Prefab for the card

    private CardUIView _uiReference;
    private Card3DView targetCard;

    [SerializeField] RenderTexture renderTextureTemplate;
    private RenderTexture renderTexture;
    public CardUIView Register3DCard(Card3DView card3DView) {
        targetCard = card3DView;

        _uiReference = Instantiate(cardUIPrefab, uiContainer);
        RectTransform rectTransform = _uiReference.RectTransform;
        rectTransform.anchorMin = Vector2.zero; // Нижній лівий кут (0,0)
        rectTransform.anchorMax = Vector2.one;  // Верхній правий кут (1,1)
        rectTransform.offsetMin = Vector2.zero; // Виправлення нижнього краю
        rectTransform.offsetMax = Vector2.zero; // Виправлення верхнього краю

        _uiReference.OnChanged += UpdateTexture;
        renderTexture = new RenderTexture(renderTextureTemplate);
        renderTexture.name = $"CardTexture_{GetInstanceID()}";
        return _uiReference;
    }

    public void UpdateTexture() {
        if (targetCard == null || cardUIPrefab == null) return;

        renderCamera.targetTexture = renderTexture;
        renderCamera.Render();

        RenderTexture.active = renderTexture;

        // Створити текстуру з форматом, що сумісний із RenderTexture
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        RenderTexture.active = null;

        targetCard.UpdateTexture(texture);
    }

}
