using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CardHandUIView : CardHandView {
    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private Transform ghostLayoutParent;
    [SerializeField] private CardUIView cardPrefab;
    [SerializeField] private CardLayoutGhost ghostPrefab;

    private CancellationTokenSource updatePositionCts;

    private Dictionary<CardUIView, CardLayoutGhost> layoutMap = new();

    public override CardView BuildCardView(string id) {
        if (cardPrefab == null || ghostPrefab == null) {
            Debug.LogError("CardPrefab or GhostPrefab not set!", this);
            return null;
        }

        CardUIView cardView = Instantiate(cardPrefab, cardSpawnPoint);
        CardLayoutGhost ghost = Instantiate(ghostPrefab, ghostLayoutParent);

        AddCardViewToLayout(cardView, ghost);
        UpdateCardPositions();

        return cardView;
    }

    private void AddCardViewToLayout(CardUIView cardView, CardLayoutGhost ghost) {
        layoutMap[cardView] = ghost;
    }

    public override void HandleCardViewRemoval(CardView cardView) {
        if (cardView is CardUIView uiView) {
            if (layoutMap.TryGetValue(uiView, out var ghost)) {
                Destroy(ghost.gameObject);
                layoutMap.Remove(uiView);
            }

            uiView.PlayRemovalAnimation().Forget();
            UpdateCardPositions();
        }
    }

    public override void UpdateCardPositions() {
        UpdateCardsPositionsAsync().Forget();
    }

    protected override void OnCardHover(CardView changedCardView, bool isHovered) {
        CardUIView changedCard3D = changedCardView as CardUIView;
    }

    private async UniTask UpdateCardsPositionsAsync(int delayFrames = 3) {
        updatePositionCts?.Cancel();
        var newCts = new CancellationTokenSource();
        updatePositionCts = newCts;

        await UniTask.DelayFrame(delayFrames, cancellationToken: updatePositionCts.Token).SuppressCancellationThrow();

        try {
            foreach (var pair in layoutMap) {
                if (newCts.IsCancellationRequested) return;
                await UniTask.NextFrame(newCts.Token);

                var card = pair.Key;
                var ghost = pair.Value;

                if (card != null && ghost != null) {
                    Vector3 newLocalPosition = card.transform.parent.InverseTransformPoint(ghost.transform.position);

                    await card.transform.DOLocalMove(newLocalPosition, 0.8f)
                        .SetEase(Ease.InOutSine);
                }
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

    protected override void OnDestroy() {
        updatePositionCts?.Cancel();
        updatePositionCts?.Dispose();

        foreach (var ghost in layoutMap.Values) {
            if (ghost != null) Destroy(ghost.gameObject);
        }

        layoutMap.Clear();
    }
}

