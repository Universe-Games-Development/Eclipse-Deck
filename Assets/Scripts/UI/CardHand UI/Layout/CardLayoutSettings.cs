using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "HandLayoutSettings", menuName = "Cards/3D Hand Layout Settings")]
public class Linear3DHandLayoutSettings : ScriptableObject {
    [Header("Cards Positioning")]
    [Tooltip("Максимальная ширина руки (в локальных координатах)")]
    public float MaxHandWidth = 3.0f;

    [Tooltip("Базовая Y-позиция карт в руке")]
    public float HeightOffset = 0.0f;
    public float CardWidth = 0.1f;
    public float CardHeight= 0.2f;

    [Tooltip("Вертикальное смещение между картами для создания эффекта глубины")]
    public float VerticalOffset = 0.01f;

    [Tooltip("Небольшая вариация позиции для визуального разнообразия")]
    public float PositionVariation = 0.02f;

    [Header("Rotation Settings")]
    [Tooltip("Максимальный угол поворота крайних карт")]
    [Range(0f, 45f)]
    public float MaxRotationAngle = 15.0f;

    [Tooltip("Небольшое смещение поворота для визуального разнообразия")]
    [Range(0f, 5f)]
    public float RotationOffset = 1.0f;

    [Header("Advanced Settings")]
    [Tooltip("Максимальное количество карт, при котором используется полная ширина")]
    public int MaxCardsAtFullWidth = 7;

    [Tooltip("Масштабирование карт при большом количестве")]
    public bool ScaleCardsWhenCrowded = true;

    [Tooltip("Минимальный масштаб карты при большом количестве")]
    [Range(0.5f, 1f)]
    public float MinCardScale = 0.8f;

    // Методы для получения динамических настроек в зависимости от числа карт
    public float GetScaleForCardCount(int cardCount) {
        if (!ScaleCardsWhenCrowded || cardCount <= MaxCardsAtFullWidth) {
            return 1.0f;
        }

        float t = Mathf.Clamp01((float)(cardCount - MaxCardsAtFullWidth) / 10f);
        return Mathf.Lerp(1.0f, MinCardScale, t);
    }

    public float GetSpacingForCardCount(int cardCount) {
        float baseSpacing = MaxHandWidth / Mathf.Max(1, cardCount - 1);
        float scale = GetScaleForCardCount(cardCount);

        // При уменьшении масштаба можно немного уменьшить расстояние между картами
        return baseSpacing * Mathf.Lerp(1.0f, 0.8f, 1f - scale);
    }
}