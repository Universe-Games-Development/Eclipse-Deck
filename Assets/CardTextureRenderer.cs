using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Відповідає за рендеринг UI-карт як текстур для 3D-карт
/// </summary>
public class CardTextureRenderer : MonoBehaviour {
    [SerializeField] private RenderingRoom staticRenderingRoom;
    [SerializeField] private RenderingRoom dynamicRenderingRoom;

    private Dictionary<Card3DView, CardUIView> cardMappings = new Dictionary<Card3DView, CardUIView>();
    private HashSet<CardUIView> dynamicCards = new HashSet<CardUIView>();

    private void LateUpdate() {
        // Рендеримо динамічні карти в кожному кадрі
        foreach (var cardUI in dynamicCards) {
            UpdateCardTexture(cardUI);
        }
    }

    /// <summary>
    /// Реєструє 3D-карту та створює для неї UI-представлення
    /// </summary>
    public void Register3DCard(Card3DView card3D, bool isDynamic = false) {
        // Створюємо UI-карту у відповідній кімнаті рендерингу
        CardUIView cardUIView = isDynamic
            ? dynamicRenderingRoom.CreateCardUI()
            : staticRenderingRoom.CreateCardUI();

        // Ініціалізуємо 3D-карту з UI-представленням
        card3D.Initialize(cardUIView);

        // Зберігаємо зв'язок між 3D та UI картами
        cardMappings[card3D] = cardUIView;

        if (isDynamic) {
            dynamicCards.Add(cardUIView);
        }

        // Перший рендер текстури
        UpdateCardTexture(cardUIView);

        // Підписуємось на зміни в UI-карті
        cardUIView.OnChanged += () => OnCardUIViewChanged(cardUIView);
    }

    /// <summary>
    /// Змінює режим карти між статичним і динамічним
    /// </summary>
    public void SetCardDynamic(Card3DView card3D, bool isDynamic) {
        if (!cardMappings.TryGetValue(card3D, out CardUIView cardUI))
            return;

        if (isDynamic && !dynamicCards.Contains(cardUI)) {
            // Переміщення карти з статичної в динамічну кімнату
            staticRenderingRoom.RemoveCardUI(cardUI);
            dynamicRenderingRoom.AddCardUI(cardUI);
            dynamicCards.Add(cardUI);
        } else if (!isDynamic && dynamicCards.Contains(cardUI)) {
            // Переміщення карти з динамічної в статичну кімнату
            dynamicRenderingRoom.RemoveCardUI(cardUI);
            staticRenderingRoom.AddCardUI(cardUI);
            dynamicCards.Remove(cardUI);

            // Фінальний рендер для статичної карти
            UpdateCardTexture(cardUI);
        }
    }

    /// <summary>
    /// Обробляє зміни в UI-карті (виклик з івенту OnCardChanged)
    /// </summary>
    private void OnCardUIViewChanged(CardUIView cardUI) {
        // Якщо карта статична - рендеримо її текстуру негайно
        if (!dynamicCards.Contains(cardUI)) {
            UpdateCardTexture(cardUI);
        }
        // Для динамічних карт оновлення відбувається в LateUpdate
    }

    /// <summary>
    /// Оновлює текстуру для конкретної карти
    /// </summary>
    private void UpdateCardTexture(CardUIView cardUI) {
        RenderingRoom room = dynamicCards.Contains(cardUI) ? dynamicRenderingRoom : staticRenderingRoom;
        Texture2D texture = room.RenderCardTexture(cardUI);

        // Знаходимо всі 3D-карти, які використовують цей UI і оновлюємо їх текстури
        foreach (var pair in cardMappings) {
            if (pair.Value == cardUI) {
                pair.Key.UpdateTexture(texture);
            }
        }
    }

    /// <summary>
    /// Очищує ресурси при видаленні карти
    /// </summary>
    public void UnregisterCard(Card3DView card3D) {
        if (cardMappings.TryGetValue(card3D, out CardUIView cardUI)) {
            if (dynamicCards.Contains(cardUI)) {
                dynamicCards.Remove(cardUI);
                dynamicRenderingRoom.RemoveCardUI(cardUI);
            } else {
                staticRenderingRoom.RemoveCardUI(cardUI);
            }

            cardMappings.Remove(card3D);
            // Додайте додаткову логіку для очищення ресурсів CardUIView якщо потрібно
        }
    }
}
