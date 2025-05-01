using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CardHandUIView : MonoBehaviour, ICardHandView {
    public event Action<string> CardClicked;

    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private Transform ghostLayoutParent;
    [SerializeField] private CardUIView cardPrefab;
    [SerializeField] private CardLayoutGhost ghostPrefab;
    private CancellationTokenSource updatePositionCts;
    private bool isInteractable = true;
    private Dictionary<string, CardUIView> _cardViews = new();

    public void Toggle(bool value = true) {
        if (gameObject.activeSelf) {
            gameObject.SetActive(value);
        }
        UpdateCardsPositionsAsync().Forget();
    }

    public ICardView CreateCardView(string id) {
        CardUIView cardView = Instantiate(cardPrefab, cardSpawnPoint);
        CardLayoutGhost ghost = Instantiate(ghostPrefab, ghostLayoutParent);
        cardView.SetGhost(ghost);
        _cardViews.Add(id, cardView);

        cardView.SetInteractable(isInteractable);

        // Підписуємося на подію кліку
        cardView.OnCardClicked += OnCardUIClicked;
        UpdateCardsPositionsAsync().Forget();
        return cardView;
    }

    public void RemoveCardView(string id) {
        if (_cardViews.TryGetValue(id, out CardUIView cardUIView)) {
            cardUIView.RemoveCardView().Forget();
        }

        UpdateCardsPositionsAsync().Forget();
    }

    private async UniTask UpdateCardsPositionsAsync(int delayFrames = 3) {
        updatePositionCts?.Cancel();

        var newCts = new CancellationTokenSource();
        updatePositionCts = newCts;

        // Strange deal with Unity to wait ghost layout update
        await UniTask.DelayFrame(delayFrames, cancellationToken: updatePositionCts.Token);

        try {
            foreach (var viewId in _cardViews) {
                if (newCts.IsCancellationRequested) return;
                await UniTask.NextFrame(newCts.Token);

                CardUIView cardUIView = viewId.Value;

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
    private void OnCardUIClicked(CardUIView cardView) {
        CardClicked?.Invoke(cardView.Id);
    }

    public void SetInteractable(bool value) {
        isInteractable = value;
        foreach (var cardView in _cardViews.Values) {
            cardView.SetInteractable(value);
        }
    }

    public void Cleanup() {
        // Відписуємося від подій
        foreach (var cardView in _cardViews.Values) {
            cardView.OnCardClicked -= OnCardUIClicked;
        }

        updatePositionCts?.Cancel();
        updatePositionCts?.Dispose();
    }

    // Events From CardHand Model
    public void SelectCardView(string id) {
    }

    public void DeselectCardView(string id) {
    }

    private void OnDestroy() {
        Cleanup();
    }
}


public interface ICardHandView {
    event Action<string> CardClicked;

    void Cleanup();
    ICardView CreateCardView(string id);
    void DeselectCardView(string id);
    void RemoveCardView(string id);
    void SelectCardView(string id);
    void SetInteractable(bool value);
    void Toggle(bool value = true);
}
