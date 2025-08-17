using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class LinearHandLayoutStrategy {
    private readonly Linear3DHandLayoutSettings _settings;
    private CancellationTokenSource _cts = new();

    private List<UniTask> _animationTasks = new List<UniTask>();

    public LinearHandLayoutStrategy(Linear3DHandLayoutSettings settings) {
        _settings = settings;
    }

    public async UniTask UpdateLayout(List<CardView> cards, Transform handTransform) {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        if (cards == null || cards.Count == 0) return;

        // Робимо копію списку карт на випадок, якщо він зміниться
        var cardsCopy = new List<CardView>(cards);

        CalculateLayoutParameters(cardsCopy.Count, out float totalWidth, out float spacing, out float startX);

        _animationTasks.Clear();

        float speedFactor = Mathf.Clamp01(10f / Mathf.Max(1f, cardsCopy.Count));
        float moveDuration = _settings.MoveDuration * speedFactor;

        for (int i = 0; i < cardsCopy.Count; i++) {
            if (token.IsCancellationRequested) return;

            CardView card = cardsCopy[i];

            // Перевірка, чи карта ще існує
            if (card == null || card.transform == null)
                continue;

            (Vector3 targetPosition, Quaternion targetRotation) = CalculateCardTransform(
                handTransform, i, cardsCopy.Count, startX, spacing);

            _animationTasks.Add(AnimateCard(card, targetPosition, targetRotation, moveDuration, token));
        }

        try {
            if (_animationTasks.Count > 0) {
                await UniTask.WhenAll(_animationTasks);
            }
        } catch (OperationCanceledException) {
            // Ігноруємо скасування
        }
    }

    private void CalculateLayoutParameters(int cardCount, out float totalWidth, out float spacing, out float startX) {
        totalWidth = Mathf.Max(_settings.MaxHandWidth, (cardCount - 1) * _settings.CardThickness);
        spacing = cardCount > 1 ? totalWidth / (cardCount - 1) : 0f;
        startX = -totalWidth / 2f;
    }

    private (Vector3, Quaternion) CalculateCardTransform(Transform handTransform, int index, int totalCards, float startX, float spacing) {
        float xPos = startX + index * spacing;
        float yPos = _settings.DefaultYPosition;
        float zPos = -index * _settings.VerticalOffset;

        float randomOffset = (index % 3 - 1) * _settings.PositionVariation;
        xPos += randomOffset;

        Vector3 targetPosition = handTransform.TransformPoint(new Vector3(xPos, yPos, zPos));
        float rotationAngle = CalculateRotationAngle(index, totalCards);
        Quaternion targetRotation = handTransform.rotation * Quaternion.Euler(0f, rotationAngle, 0f);

        return (targetPosition, targetRotation);
    }

    private float CalculateRotationAngle(int index, int totalCards) {
        if (totalCards == 1) return 0f;

        float t = (float)index / (totalCards - 1);
        float angle = Mathf.Lerp(-_settings.MaxRotationAngle, _settings.MaxRotationAngle, t);
        float randomOffset = (index % 2 == 0 ? 1 : -1) * _settings.RotationOffset;
        angle += randomOffset;

        return angle;
    }

    private async UniTask AnimateCard(CardView card, Vector3 targetPosition, Quaternion targetRotation, float duration, CancellationToken token) {
        var moveTask = card.transform.DOMove(targetPosition, duration)
            .SetEase(_settings.MoveEase)
            .ToUniTask(cancellationToken: token);

        var rotateTask = card.transform.DORotateQuaternion(targetRotation, duration)
            .SetEase(_settings.RotationEase)
            .ToUniTask(cancellationToken: token);

        try {
            await UniTask.WhenAll(moveTask, rotateTask);
        } catch (OperationCanceledException) {
            card.transform.DOKill();
        }
    }

    public void Cleanup() {
        _cts.Cancel();
    }
}
