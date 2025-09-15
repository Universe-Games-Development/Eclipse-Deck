using UnityEngine;

[CreateAssetMenu(fileName = "HandLayoutSettings", menuName = "Cards/3D Hand Layout Settings")]
public class Linear3DHandLayoutSettings : LayoutSettigs {
    [Header("Cards Positioning")]
    [Tooltip("Максимальна ширина руки (в локальних координатах)")]
    public float MaxHandWidth = 3.0f;

    [Tooltip("Відступ між картами при нормальному розташуванні")]
    public float CardSpacing = 0.2f;

    [Tooltip("Базовая Y-позиция карт в руке")]
    public float DepthOffset = 0.0f;

    public float VerticalOffset = 0.01f;

    [Header("Rotation Settings")]
    [Tooltip("Максимальний угол повороту крайніх карт")]
    [Range(0f, 45f)]
    public float MaxRotationAngle = 15.0f;

    [Tooltip("Невелике зміщення повороту для візуального різноманіття")]
    [Range(0f, 5f)]
    public float RotationOffset = 1.0f;

    [Header("Advanced Settings")]
    [Tooltip("Максимальне количество карт, при якому використовується повна ширина")]
    public int MaxCardsAtFullWidth = 7;

    public float PositionVariation { get; internal set; }

    // Методи для отримання динамічних налаштувань залежно від числа карт
    public float GetScaleForCardCount(int cardCount) {
        if (!ScaleCardsWhenCrowded || cardCount <= MaxCardsAtFullWidth) {
            return 1.0f;
        }

        float t = Mathf.Clamp01((float)(cardCount - MaxCardsAtFullWidth) / 10f);
        return Mathf.Lerp(1.0f, MinCardScale, t);
    }

    // Цей метод тепер не використовується в новій логіці, але залишений для зворотної сумісності
    [System.Obsolete("Use new compression logic in Linear3DLayout instead")]
    public float GetSpacingForCardCount(int cardCount) {
        float baseSpacing = MaxHandWidth / Mathf.Max(1, cardCount - 1);
        float scale = GetScaleForCardCount(cardCount);
        return baseSpacing * Mathf.Lerp(1.0f, 0.8f, 1f - scale);
    }

    // Новий метод для отримання фактичної ширини карти з урахуванням масштабування
    public float GetEffectiveCardWidth(int cardCount) {
        float scale = GetScaleForCardCount(cardCount);
        return CardWidth * scale;
    }
}

public class LayoutSettigs : ScriptableObject {
    public float CardWidth = 1.0f; // Збільшено до більш реалістичного значення
    public float CardHeight = 1.4f; // Пропорційно збільшено

    [Tooltip("Масштабування карт при великій кількості")]
    public bool ScaleCardsWhenCrowded = true;

    [Tooltip("Мінімальний масштаб карти при великій кількості")]
    [Range(0.5f, 1f)]
    public float MinCardScale = 0.8f;
}