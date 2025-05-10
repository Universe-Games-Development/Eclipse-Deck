using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardHandView : MonoBehaviour {
    public event Action<string> OnCardClicked;

    protected Dictionary<string, CardView> _cardViews = new();
    protected bool isInteractable = true;

    public virtual void Toggle(bool value = true) {
        if (gameObject.activeSelf != value) {
            gameObject.SetActive(value);
        }
    }

    public virtual CardView CreateCardView(string id) {
        CardView cardView = BuildCardView(id);
        cardView.Id = id;
        cardView.OnCardClicked += OnCardViewClicked;
        cardView.SetInteractable(isInteractable);
        _cardViews.Add(id, cardView);

        return cardView;
    }

    public abstract CardView BuildCardView(string id);

    public virtual void RemoveCardView(string id) {
        if (_cardViews.TryGetValue(id, out var cardView)) {
            cardView.OnCardClicked -= OnCardViewClicked;
            _cardViews.Remove(id);
            HandleCardViewRemoval(cardView);
        }
    }

    public virtual void HandleCardViewRemoval(CardView cardView) {
        cardView.RemoveCardView().Forget(); // если RemoveCardView возвращает UniTask
        UpdateCardPositions();
    }

    protected virtual void OnCardViewClicked(CardView cardView) {
        OnCardClicked?.Invoke(cardView.Id);
    }

    public virtual void UpdateCardPositions() {
        // Optional: можно переопределить
    }

    public virtual void SetInteractable(bool value) {
        isInteractable = value;
        foreach (var cardView in _cardViews.Values) {
            cardView.SetInteractable(value);
        }
    }

    public virtual void SelectCardView(string id) {
        if (_cardViews.TryGetValue(id, out var cardView)) {
            cardView.Select();
        }
    }

    public virtual void DeselectCardView(string id) {
        if (_cardViews.TryGetValue(id, out var cardView)) {
            cardView.Deselect();
        }
    }

    public virtual void Cleanup() {
        foreach (var cardView in _cardViews.Values) {
            cardView.OnCardClicked -= OnCardViewClicked;
        }
        _cardViews.Clear();
    }

    protected virtual void OnDestroy() {
        Cleanup();
    }
}


