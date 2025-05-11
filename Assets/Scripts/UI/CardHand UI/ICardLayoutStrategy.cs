using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public interface ICardLayoutStrategy {
    void SetSettings(CardLayoutSettings settings);
    UniTask LayoutCards(CardHandView handView, Transform[] cardTransforms, bool immediate = false);
    UniTask SetCardHovered(Transform cardTransform, bool isHovered);
}


public abstract class BaseCardLayoutStrategy : ICardLayoutStrategy {
    protected CardLayoutSettings Settings { get; private set; }
    protected Dictionary<Transform, Vector3> _originalPositions = new ();
    protected Dictionary<Transform, Quaternion> _originalRotations = new ();
    protected Dictionary<Transform, bool> _hoveredStates = new();

    protected BaseCardLayoutStrategy(CardLayoutSettings settings = null) {
        Settings = settings;
    }

    public void SetSettings(CardLayoutSettings settings) {
        if (settings != null) {
            Settings = settings;
        }
    }

    public abstract UniTask LayoutCards(CardHandView handView, Transform[] cardTransforms, bool immediate = false);

    public UniTask SetCardHovered(Transform cardTransform, bool isHovered) {
        if (cardTransform == null) return UniTask.CompletedTask;

        _hoveredStates[cardTransform] = isHovered;

        if (!_originalPositions.ContainsKey(cardTransform) || !_originalRotations.ContainsKey(cardTransform)) {
            return UniTask.CompletedTask;
        }

        // Получаем оригинальную позицию и поворот
        Vector3 originalPosition = _originalPositions[cardTransform];
        Quaternion originalRotation = _originalRotations[cardTransform];

        if (isHovered) {
            // Поднимаем и выдвигаем карту вперед при наведении
            Vector3 hoverPosition = originalPosition + new Vector3(
                0,
                Settings.hoverYOffset,
                Settings.hoverZOffset
            );

            return AnimateCardToPosition(
                cardTransform,
                hoverPosition,
                originalRotation,
                0,
                false,
                Settings.hoverAnimationDuration
            );
        } else {
            // Возвращаем карту на место
            return AnimateCardToPosition(
                cardTransform,
                originalPosition,
                originalRotation,
                0,
                false,
                Settings.hoverAnimationDuration
            );
        }
    }

    // Обновленный метод анимации с возможностью указать длительность
    protected async UniTask AnimateCardToPosition(
        Transform cardTransform,
        Vector3 targetPos,
        Quaternion targetRot,
        float delay = 0,
        bool immediate = false,
        float? customDuration = null) {

        // Сохраняем оригинальную позицию и поворот
        _originalPositions[cardTransform] = targetPos;
        _originalRotations[cardTransform] = targetRot;

        // Если карта в состоянии hover, применяем соответствующее смещение
        if (_hoveredStates.TryGetValue(cardTransform, out bool isHovered) && isHovered) {
            targetPos += new Vector3(0, Settings.hoverYOffset, Settings.hoverZOffset);
        }

        if (immediate) {
            cardTransform.SetLocalPositionAndRotation(targetPos, targetRot);
            return;
        }

        // Добавляем небольшую задержку для последовательной анимации
        if (delay > 0) {
            await UniTask.Delay((int)(delay * 1000));
        }

        float animDuration = customDuration ?? (Settings != null ? Settings.animationDuration : 0.3f);
        float rotDuration = customDuration ?? (Settings != null ? Settings.rotationDuration : 0.2f);

        // Запускаем анимации параллельно
        var moveTask = cardTransform.DOLocalMove(targetPos, animDuration)
            .SetEase(Ease.OutQuad)
            .ToUniTask();

        var rotateTask = cardTransform.DOLocalRotateQuaternion(targetRot, rotDuration)
            .SetEase(Ease.OutQuad)
            .ToUniTask();

        await UniTask.WhenAll(moveTask, rotateTask);
    }

    // Обновленный метод для получения правильной последовательности карт
    // с поддержкой направления справа налево или слева направо
    protected int[] GetCardOrderSequence(int count) {
        var order = new int[count];
        bool rightToLeft = Settings == null || Settings.rightToLeftOrder;

        // Если нужно чтобы первой была правая карта
        if (rightToLeft) {
            for (int i = 0; i < count; i++) {
                order[i] = count - i - 1;
            }
        } else {
            // Обычный порядок слева направо
            for (int i = 0; i < count; i++) {
                order[i] = i;
            }
        }

        return order;
    }

    protected float GetCardThickness() {
        return Settings != null ? Settings.cardThickness : 0.1f;
    }

    protected float GetMaxAnimationDelay() {
        return Settings != null ? Settings.maxAnimationDelay : 0.1f;
    }

    protected float GetMaxCardYOffset() {
        return Settings != null ? Settings.maxCardYOffset : 0.2f;
    }
}

// Обновленная стратегия размещения по кривой
public class CurvedHandLayoutStrategy : BaseCardLayoutStrategy {
    public CurvedHandLayoutStrategy(CardLayoutSettings settings = null) : base(settings) { }

    public override async UniTask LayoutCards(CardHandView handView, Transform[] cardTransforms, bool immediate = false) {
        if (cardTransforms == null || cardTransforms.Length == 0) return;

        int totalCards = cardTransforms.Length;

        // Calculate appropriate spacing based on card count
        float cardSpacing = Settings != null
            ? Settings.CalculateCardSpacing(totalCards)
            : 1.2f;

        float centerOffset = (totalCards - 1) * cardSpacing * 0.5f;
        var orderSequence = GetCardOrderSequence(totalCards);

        var animationTasks = new UniTask[totalCards];
        float maxDelay = GetMaxAnimationDelay();
        float maxCardYOffset = GetMaxCardYOffset();

        // Get settings for curved layout
        float curveHeight = Settings != null ? Settings.cardCurveHeight : 0.5f;
        float rotationAngle = Settings != null ? Settings.cardRotationAngle : 15f;

        for (int i = 0; i < totalCards; i++) {
            int cardIndex = orderSequence[i];
            var cardTransform = cardTransforms[i]; // Используем i вместо cardIndex, так как порядок уже учтен в orderSequence
            if (cardTransform == null) continue;

            // Рассчитываем позицию на кривой
            float xPos = (cardIndex * cardSpacing) - centerOffset;
            float yPos = CalculateYPosition(xPos, centerOffset, curveHeight);

            // Z-позиция учитывает приоритет отображения согласно настройкам
            float zPos = Settings != null
                ? Settings.CalculateZOrder(cardIndex, totalCards, xPos)
                : -Mathf.Abs(xPos) * GetCardThickness();

            // Рассчитываем поворот (карты веером)
            float rotAngle = CalculateRotationAngle(xPos, centerOffset, rotationAngle);

            // Небольшой случайный Y-отступ для естественности
            float randomYOffset = Random.Range(-maxCardYOffset, maxCardYOffset) * 0.1f;

            Vector3 targetPosition = new(xPos, yPos + randomYOffset, zPos);
            Quaternion targetRotation = Quaternion.Euler(0, rotAngle, 0);

            // Задержка для последовательной анимации
            float delay = immediate ? 0 : maxDelay * (i / (float)totalCards);

            animationTasks[i] = AnimateCardToPosition(
                cardTransform,
                targetPosition,
                targetRotation,
                delay,
                immediate
            );
        }

        await UniTask.WhenAll(animationTasks);
    }

    private float CalculateYPosition(float xPos, float maxOffset, float curveHeight) {
        if (maxOffset == 0) return 0;
        float normalizedX = xPos / maxOffset;
        // Пересмотренная функция для лучшей кривой
        return -curveHeight * (1 - normalizedX * normalizedX);
    }

    private float CalculateRotationAngle(float xPos, float maxOffset, float maxRotation) {
        if (maxOffset == 0) return 0;
        float normalizedX = xPos / maxOffset;
        return normalizedX * maxRotation;
    }
}

// Обновленная линейная стратегия размещения
public class LinearHandLayoutStrategy : BaseCardLayoutStrategy {
    public LinearHandLayoutStrategy(CardLayoutSettings settings = null) : base(settings) { }

    public override async UniTask LayoutCards(CardHandView handView, Transform[] cardTransforms, bool immediate = false) {
        if (cardTransforms == null || cardTransforms.Length == 0) return;

        int totalCards = cardTransforms.Length;

        // Calculate appropriate spacing based on card count
        float cardSpacing = Settings != null
            ? Settings.CalculateCardSpacing(totalCards, true)
            : 1.5f;

        float centerOffset = (totalCards - 1) * cardSpacing * 0.5f;
        var orderSequence = GetCardOrderSequence(totalCards);

        var animationTasks = new UniTask[totalCards];
        float maxDelay = GetMaxAnimationDelay();
        float maxCardYOffset = GetMaxCardYOffset();

        for (int i = 0; i < totalCards; i++) {
            int cardIndex = orderSequence[i];
            var cardTransform = cardTransforms[i]; // Используем i вместо cardIndex
            if (cardTransform == null) continue;

            // Линейное расположение
            float xPos = (cardIndex * cardSpacing) - centerOffset;

            // Небольшой случайный Y-отступ для естественности
            float yPos = Random.Range(-maxCardYOffset, maxCardYOffset);

            // Z-позиция с учетом настроек приоритета
            float zPos = Settings != null
                ? Settings.CalculateZOrder(cardIndex, totalCards, xPos)
                : -i * GetCardThickness();

            Quaternion targetRotation = Quaternion.identity;
            Vector3 targetPosition = new (xPos, yPos, zPos);

            // Задержка для последовательной анимации
            float delay = immediate ? 0 : maxDelay * (i / (float)totalCards);

            animationTasks[i] = AnimateCardToPosition(
                cardTransform,
                targetPosition,
                targetRotation,
                delay,
                immediate
            );
        }

        await UniTask.WhenAll(animationTasks);
    }
}