using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class LinearHandLayoutStrategy {
    private readonly Linear3DHandLayoutSettings _settings;
    private readonly Dictionary<CardView, int> _cardSortingOrders = new Dictionary<CardView, int>();
    private CardView _hoveredCard;
    private CancellationTokenSource _cts = new();

    public LinearHandLayoutStrategy(Linear3DHandLayoutSettings settings) {
        _settings = settings;
    }

    public async UniTask UpdateLayout(List<CardView> cards, Transform handTransform) {
        _cts.Cancel(); // Скасовуємо попереднє оновлення
        _cts = new CancellationTokenSource(); // Створюємо новий токен
        var token = _cts.Token;

        if (cards == null || cards.Count == 0) return;

        float totalWidth = Mathf.Max(_settings.MaxHandWidth, (cards.Count - 1) * _settings.CardThickness);
        float spacing = cards.Count > 1 ? totalWidth / (cards.Count - 1) : 0f;
        float startX = -totalWidth / 2f;

        // Список для всіх анімаційних задач
        List<UniTask> animationTasks = new();

        for (int i = 0; i < cards.Count; i++) {
            if (token.IsCancellationRequested) return;

            CardView card = cards[i];
            bool isHovered = card == _hoveredCard;

            float xPos = startX + i * spacing;
            float yPos = _settings.DefaultYPosition + (isHovered ? _settings.HoverHeight : 0f);
            float zPos = -i * _settings.VerticalOffset;

            Vector3 targetPosition = handTransform.TransformPoint(new Vector3(xPos, yPos, zPos));
            float rotationAngle = CalculateRotationAngle(i, cards.Count);
            Quaternion targetRotation = handTransform.rotation * Quaternion.Euler(0f, rotationAngle, 0f);

            // Додаємо задачу в список, але не чекаємо одразу
            animationTasks.Add(AnimateCard(card, targetPosition, targetRotation, isHovered, token));
        }

        try {
            // Чекаємо завершення всіх анімацій одночасно
            await UniTask.WhenAll(animationTasks);
        } catch (OperationCanceledException) {
            // Можеш обробити скасування, якщо потрібно
        }
    }

    private float CalculateRotationAngle(int index, int totalCards) {
        if (totalCards == 1) return 0f;

        // Calculate rotation angle based on position in hand
        float t = (float)index / (totalCards - 1);
        float angle = Mathf.Lerp(-_settings.MaxRotationAngle, _settings.MaxRotationAngle, t);

        // Add small offset to prevent perfect alignment
        angle += (index % 2 == 0 ? 1 : -1) * _settings.RotationOffset;

        return angle;
    }

    private async UniTask AnimateCard(CardView card, Vector3 targetPosition, Quaternion targetRotation, bool isHovered, CancellationToken token) {
        int cardCount = _cardSortingOrders.Count;

        float speedFactor = Mathf.Clamp01(10f / (float)cardCount);
        float moveDuration = (isHovered ? _settings.HoverMoveDuration : _settings.MoveDuration) * speedFactor;
        float rotationDuration = _settings.RotationDuration * speedFactor;

        // Создаем задачи анимации движения и вращения
        var moveTask = card.transform.DOMove(targetPosition, moveDuration).SetEase(_settings.MoveEase).ToUniTask(cancellationToken: token);
        var rotateTask = card.transform.DORotateQuaternion(targetRotation, rotationDuration).SetEase(_settings.RotationEase).ToUniTask(cancellationToken: token);

        try {
            // Ждем выполнения обеих анимаций одновременно
            await UniTask.WhenAll(moveTask, rotateTask);
        } catch (OperationCanceledException) {
            // Если задача отменена, отменяем все твины
            card.transform.DOKill();
        }
    }

    public void Cleanup() {
        _cardSortingOrders.Clear();
        _hoveredCard = null;
    }
}