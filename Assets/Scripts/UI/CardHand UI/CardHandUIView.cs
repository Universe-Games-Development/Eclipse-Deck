using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class CardHandUIView : CardHandViewBase<CardUIView> {
    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private Transform ghostLayoutParent;
    [SerializeField] private CardUIView cardPrefab;
    [SerializeField] private CardLayoutGhost ghostPrefab;
    private CancellationTokenSource updatePositionCts;

    public override CardUIView BuildCardView(string id) {
        if (cardPrefab == null || ghostPrefab == null) {
            Debug.LogError("CardPrefab or CardsContainer not set!", this);
            return null;
        }
        CardUIView cardView = Instantiate(cardPrefab, cardSpawnPoint);
        CardLayoutGhost ghost = Instantiate(ghostPrefab, ghostLayoutParent);
        cardView.SetGhost(ghost);

        UpdateCardsPositionsAsync().Forget();
        return cardView;
    }

    public override void HandleCardViewRemoval(CardUIView cardView) {
        cardView.RemoveCardView().Forget();
        UpdateCardsPositionsAsync().Forget();
    }

    private async UniTask UpdateCardsPositionsAsync(int delayFrames = 3) {
        updatePositionCts?.Cancel();
        var newCts = new CancellationTokenSource();
        updatePositionCts = newCts;

        // Wait for ghost layout update
        await UniTask.DelayFrame(delayFrames, cancellationToken: updatePositionCts.Token).SuppressCancellationThrow(); ;

        try {
            foreach (var viewPair in _cardViews) {
                if (newCts.IsCancellationRequested) return;
                await UniTask.NextFrame(newCts.Token);

                // No type casting needed now
                CardUIView cardUIView = viewPair.Value;
                cardUIView.UpdatePosition();
            }
        } catch (OperationCanceledException) {
            Debug.Log("UpdateCardsPositionsAsync cancelled.");
        } finally {
            if (updatePositionCts == newCts) {
                updatePositionCts.Dispose();
                updatePositionCts = null;
            }
        }
    }

    public override void Cleanup() {
        base.Cleanup();
        updatePositionCts?.Cancel();
        updatePositionCts?.Dispose();
    }
}
