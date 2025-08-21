using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardHandView : MonoBehaviour {
    public event Action<CardView> OnCardClicked;
    public event Action<CardView, bool> OnCardHovered;

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
        cardView.OnCardClicked += OnCardClickDown;
        cardView.OnHoverChanged += OnCardHover;

        _cardViews.Add(id, cardView);

        // Обновляем кэш индексов
        RefreshCardIndexCache();
        UpdateCardPositions();

        return cardView;
    }

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
            cardView.OnCardClicked -= OnCardClickDown;
            cardView.OnHoverChanged -= OnCardHover;
            _cardViews.Remove(id);
            HandleCardViewRemoval(cardView);
        }
    }

    public virtual void HandleCardViewRemoval(CardView cardView) {
        cardView.PlayRemovalAnimation().Forget(); 
        UpdateCardPositions();
    }

    protected virtual void OnCardClickDown(CardView cardView) {
        OnCardClicked?.Invoke(cardView);
    }

    protected virtual void OnCardHover(CardView view, bool isHovered) {
        OnCardHovered?.Invoke(view, isHovered);
    }

    public abstract void UpdateCardPositions();


    protected virtual void OnDestroy() {
        foreach (var cardView in _cardViews.Values) {
            cardView.OnCardClicked -= OnCardClickDown;
            cardView.OnHoverChanged -= OnCardHover;
        }
        _cardViews.Clear();
    }

    public virtual void SetCardHover(CardView changedCardView, bool isHovered) {
        throw new NotImplementedException();
    }
}


