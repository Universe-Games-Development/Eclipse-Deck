using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardHandViewBase<TCardView> : MonoBehaviour, ICardHandView
    where TCardView : CardView {
    public event Action<string> OnCardClicked;
    protected Dictionary<string, TCardView> _cardViews = new();
    protected bool isInteractable = true;

    public CardView CreateCardView(string id) {
        TCardView cardView = BuildCardView(id);
       
        cardView.Id = id;
        cardView.OnCardClicked += OnCardViewClicked;
        cardView.SetInteractable(isInteractable);

        _cardViews.Add(id, cardView);
        return cardView;
    }

    public abstract TCardView BuildCardView(string id);

    public virtual void Toggle(bool value = true) {
        if (gameObject.activeSelf != value) {
            gameObject.SetActive(value);
        }
    }

    public virtual void RemoveCardView(string id) {
        if (_cardViews.TryGetValue(id, out TCardView cardView)) {
            // Unsubscribe from events
            cardView.OnCardClicked -= OnCardViewClicked;

            // Remove the view from dictionary
            _cardViews.Remove(id);

            // Handle removal in derived class
            HandleCardViewRemoval(cardView);
        }
    }

    public abstract void HandleCardViewRemoval(TCardView cardView);


    protected virtual void OnCardViewClicked(CardView cardView) {
        OnCardClicked?.Invoke(cardView.Id);
    }

    public virtual void SetInteractable(bool value) {
        isInteractable = value;
        foreach (var cardView in _cardViews.Values) {
            cardView.SetInteractable(value);
        }
    }

    public virtual void SelectCardView(string id) {
        if (_cardViews.TryGetValue(id, out TCardView cardView)) {
            cardView.Select();
        }
    }

    public virtual void DeselectCardView(string id) {
        if (_cardViews.TryGetValue(id, out TCardView cardView)) {
            cardView.Deselect();
        }
    }

    public virtual void Cleanup() {
        // Unsubscribe from events
        foreach (var cardView in _cardViews.Values) {
            cardView.OnCardClicked -= OnCardViewClicked;
        }

        _cardViews.Clear();
    }

    protected virtual void OnDestroy() {
        Cleanup();
    }
}


public interface ICardHandView {
    event Action<string> OnCardClicked;

    void Cleanup();
    CardView CreateCardView(string id);
    void DeselectCardView(string id);
    void RemoveCardView(string id);
    void SelectCardView(string id);
    void SetInteractable(bool value);
    void Toggle(bool value = true);
}
