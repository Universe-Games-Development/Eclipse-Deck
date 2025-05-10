using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Linq;

public interface ICardLayoutStrategy {
    UniTask LayoutCards(CardHandView handView, Transform[] cardTransforms, bool immediate = false);
    void SetSettings(CardLayoutSettings settings);
}


public abstract class BaseCardLayoutStrategy : ICardLayoutStrategy {
    protected CardLayoutSettings Settings { get; private set; }

    protected BaseCardLayoutStrategy(CardLayoutSettings settings = null) {
        Settings = settings;
    }

    public void SetSettings(CardLayoutSettings settings) {
        if (settings != null) {
            Settings = settings;
        }
    }

    public abstract UniTask LayoutCards(CardHandView handView, Transform[] cardTransforms, bool immediate = false);

    protected async UniTask AnimateCardToPosition(Transform cardTransform, Vector3 targetPos,
        Quaternion targetRot, float delay = 0, bool immediate = false) {
        if (immediate) {
            cardTransform.localPosition = targetPos;
            cardTransform.localRotation = targetRot;
            return;
        }

        // Добавляем небольшую задержку для последовательной анимации
        if (delay > 0) {
            await UniTask.Delay((int)(delay * 1000));
        }

        float animDuration = Settings != null ? Settings.animationDuration : 0.3f;
        float rotDuration = Settings != null ? Settings.rotationDuration : 0.2f;

        // Запускаем анимации параллельно
        var moveTask = cardTransform.DOLocalMove(targetPos, animDuration)
            .SetEase(Ease.OutQuad)
            .ToUniTask();

        var rotateTask = cardTransform.DOLocalRotateQuaternion(targetRot, rotDuration)
            .SetEase(Ease.OutQuad)
            .ToUniTask();

        await UniTask.WhenAll(moveTask, rotateTask);
    }

    protected int[] GetCardOrderSequence(int count) {
        // Определяем порядок анимации: сначала центральные карты, затем к краям
        var order = new int[count];
        int mid = count / 2;
        int sign = 1;
        int counter = 0;

        for (int i = 0; i < count; i++) {
            order[i] = mid + sign * counter;
            sign *= -1;
            if (sign < 0) counter++;
        }

        return count % 2 == 0 ? order.Take(count).ToArray() : order.Take(count).ToArray();
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
        float cardThickness = GetCardThickness();

        // Get settings for curved layout
        float curveHeight = Settings != null ? Settings.cardCurveHeight : 0.5f;
        float rotationAngle = Settings != null ? Settings.cardRotationAngle : 15f;

        for (int i = 0; i < totalCards; i++) {
            int cardIndex = orderSequence[i];
            var cardTransform = cardTransforms[cardIndex];
            if (cardTransform == null) continue;

            // Рассчитываем позицию на кривой
            float xPos = (cardIndex * cardSpacing) - centerOffset;
            float yPos = CalculateYPosition(xPos, centerOffset, curveHeight);

            // Z-позиция для правильного порядка отрисовки (центральные карты поверх)
            float zPos = -Mathf.Abs(xPos) * cardThickness;

            // Рассчитываем поворот (карты веером)
            float rotAngle = CalculateRotationAngle(xPos, centerOffset, rotationAngle);

            // Небольшой случайный Y-отступ для естественности
            float randomYOffset = Random.Range(-maxCardYOffset, maxCardYOffset) * 0.1f;

            Vector3 targetPosition = new Vector3(xPos, yPos + randomYOffset, zPos);
            Quaternion targetRotation = Quaternion.Euler(0, rotAngle, 0);

            // Задержка для последовательной анимации
            float delay = immediate ? 0 : maxDelay * (i / (float)totalCards);

            animationTasks[cardIndex] = AnimateCardToPosition(
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
        return -curveHeight * (1 - normalizedX * normalizedX);
    }

    private float CalculateRotationAngle(float xPos, float maxOffset, float maxRotation) {
        if (maxOffset == 0) return 0;
        float normalizedX = xPos / maxOffset;
        return normalizedX * maxRotation;
    }
}
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
        float cardThickness = GetCardThickness();

        for (int i = 0; i < totalCards; i++) {
            int cardIndex = orderSequence[i];
            var cardTransform = cardTransforms[cardIndex];
            if (cardTransform == null) continue;

            // Линейное расположение
            float xPos = (cardIndex * cardSpacing) - centerOffset;

            // Небольшой случайный Y-отступ для естественности
            float yPos = Random.Range(-maxCardYOffset, maxCardYOffset);

            // Z-позиция для правильного порядка отрисовки
            float zPos = -i * cardThickness;

            Quaternion targetRotation = Quaternion.identity;
            Vector3 targetPosition = new Vector3(xPos, yPos, zPos);

            // Задержка для последовательной анимации
            float delay = immediate ? 0 : maxDelay * (i / (float)totalCards);

            animationTasks[cardIndex] = AnimateCardToPosition(
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