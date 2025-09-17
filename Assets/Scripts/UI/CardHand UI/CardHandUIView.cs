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

    public override CardView CreateCardView(Card card) {
        if (cardPrefab == null || ghostPrefab == null) {
            Debug.LogError("CardPrefab or GhostPrefab not set!", this);
            return null;
        }

        CardUIView cardView = Instantiate(cardPrefab, cardSpawnPoint);
        CardLayoutGhost ghost = Instantiate(ghostPrefab, ghostLayoutParent);

        AddCardViewToLayout(cardView, ghost);

        return cardView;
    }

    private void AddCardViewToLayout(CardUIView cardView, CardLayoutGhost ghost) {
        layoutMap[cardView] = ghost;
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
        base.OnDestroy();
    }

    public override void RemoveCardView(CardView cardView) {
        throw new NotImplementedException();
    }

    protected override void HandleCardViewRemoval(CardView cardView) {
        throw new NotImplementedException();
    }

    protected override void HandleCardHovered(CardView cardView) {
        throw new NotImplementedException();
    }

    public override void UpdateCardPositions(List<CardView> cardViews) {
        throw new NotImplementedException();
    }

    protected override void HandleClearCardHovered(CardView cardView) {
        throw new NotImplementedException();
    }

    public override void RegisterView(CardView cardView) {
        throw new NotImplementedException();
    }
}

