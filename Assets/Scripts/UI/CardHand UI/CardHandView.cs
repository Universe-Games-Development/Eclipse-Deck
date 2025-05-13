using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardHandView : MonoBehaviour {
    public event Action<string> OnCardClicked;

    protected Dictionary<string, CardView> _cardViews = new();
    protected bool isInteractable = true;

    // Кэш для оптимизации поиска индексов
    protected Dictionary<CardView, int> _cardIndexCache = new();

    public virtual void Toggle(bool value = true) {
        if (gameObject.activeSelf != value) {
            gameObject.SetActive(value);
        }
    }

    public virtual CardView CreateCardView(string id = null) {
        id ??= Guid.NewGuid().ToString();

        CardView cardView = BuildCardView(id);
        cardView.Id = id;
        cardView.OnCardClicked += OnCardViewClicked;
        cardView.OnHoverChanged += HandleCardHover;

        cardView.SetInteractable(isInteractable);
        _cardViews.Add(id, cardView);

        // Обновляем кэш индексов
        RefreshCardIndexCache();
        UpdateCardPositions();

        return cardView;
    }

    protected abstract void HandleCardHover(CardView changedCardView, bool isHovered);

    protected void RefreshCardIndexCache() {
        _cardIndexCache.Clear();
        int index = 0;
        foreach (var kvp in _cardViews) {
            _cardIndexCache[kvp.Value] = index++;
        }
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

    public abstract void UpdateCardPositions();

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

    protected virtual void OnDestroy() {
        foreach (var cardView in _cardViews.Values) {
            cardView.OnCardClicked -= OnCardViewClicked;
        }
        _cardViews.Clear();
    }
}


