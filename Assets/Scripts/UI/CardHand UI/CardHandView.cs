using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardHandView : UnitView {
    public event Action<string> OnCardClicked;
    public event Action<string, bool> OnCardHovered;

    private readonly Dictionary<CardView, string> cardViewToIdMap = new Dictionary<CardView, string>();

    private bool isInteractable;
    private CardView hoveredCard;

    #region Public API

    public void SetInteractable(bool value) {
        isInteractable = value;

        //Debug.Log("HandInteractable : " + value);
        // Застосовуємо до всіх карт
        foreach (var cardView in cardViewToIdMap.Keys) {
            cardView.SetInteractable(value);
        }

        if (value == false && hoveredCard != null) {
            ClearHoveredCard();
        }
    }

    public abstract void UpdateCardPositions();

    public CardView GetCardView(string cardInstanceId) {
        foreach (var kvp in cardViewToIdMap) {
            if (kvp.Value == cardInstanceId) {
                return kvp.Key;
            }
        }
        return null;
    }

    public string GetCardInstanceId(CardView cardView) {
        return cardViewToIdMap.TryGetValue(cardView, out string id) ? id : null;
    }

    /// <summary>
    /// Створює нову CardView з пулу
    /// </summary>
    public abstract CardView CreateCardView();

    public virtual void AddCardView(CardView cardView, string cardInstanceId) {
        if (cardView == null) {
            Debug.LogWarning("Trying to add null CardView");
            return;
        }

        if (string.IsNullOrEmpty(cardInstanceId)) {
            Debug.LogError("CardInstanceId cannot be null or empty");
            return;
        }

        RegisterCard(cardView, cardInstanceId);

        // Викликаємо спеціалізовану логіку реєстрації
        AddCard(cardView);
    }

    public virtual void RemoveCardView(CardView cardView) {
        if (cardView == null) return;
        UnregisterCard(cardView);

        // Викликаємо спеціалізовану логіку видалення
        RemoveCard(cardView);
    }

    public virtual void AddCardViews(List<CardView> cardViews) {

    }

    public virtual void RemoveCardViews(List<CardView> cardViews) {

    }

    private void RegisterCard(CardView cardView, string cardInstanceId) {
        cardViewToIdMap[cardView] = cardInstanceId;

        // Підписуємось на події CardView
        cardView.OnClicked += HandleCardViewClicked;
        cardView.OnHoverChanged += HandleCardViewHoverChanged;
    }

    private void UnregisterCard(CardView cardView) {
        cardView.OnClicked -= HandleCardViewClicked;
        cardView.OnHoverChanged -= HandleCardViewHoverChanged;

        // Видаляємо з маппінгу
        cardViewToIdMap.Remove(cardView);
    } 

    
    #endregion

    #region Event Handlers

    private void HandleCardViewClicked(CardView cardView) {
        if (!isInteractable) return;

        if (cardViewToIdMap.TryGetValue(cardView, out string cardInstanceId)) {
            OnCardClicked?.Invoke(cardInstanceId);
        } else {
            Debug.LogWarning($"No cardInstanceId found for CardView: {cardView.name}");
        }
    }

    private void HandleCardViewHoverChanged(CardView cardView, bool isHovered) {
        if (!isInteractable && isHovered) return;

        if (cardViewToIdMap.TryGetValue(cardView, out string cardInstanceId)) {
            
            if (isHovered) {
                //Debug.Log("Do Hover");
                hoveredCard = cardView;
                HandleCardHovered(cardView);
            } else {
                ClearHoveredCard();
            }

            OnCardHovered?.Invoke(cardInstanceId, isHovered);
        }
    }

    private void ClearHoveredCard() {
        if (hoveredCard == null) return;

        // Debug.Log("Clearing Hover");
        HandleClearCardHovered(hoveredCard);
        hoveredCard = null;
    }

    #endregion

    #region Abstract Methods - для перевизначення в нащадках

    /// <summary>
    /// Викликається після реєстрації CardView (для setup позицій, анімацій тощо)
    /// </summary>
    protected abstract void AddCard(CardView cardView);

    /// <summary>
    /// Викликається при видаленні CardView (для cleanup, повернення в pool тощо)
    /// </summary>
    protected abstract void RemoveCard(CardView cardView);

    protected abstract void AddCards(List<CardView> cardViews);

    protected abstract void RemoveCards(List<CardView> cardViews);

    /// <summary>
    /// Викликається при наведенні на карту (для hover анімацій)
    /// </summary>
    protected abstract void HandleCardHovered(CardView cardView);

    /// <summary>
    /// Викликається при забиранні курсору з карти (для повернення в нормальний стан)
    /// </summary>
    protected abstract void HandleClearCardHovered(CardView cardView);

    #endregion

    #region Unity Lifecycle

    protected virtual void OnDestroy() {
        // Відписуємось від всіх CardView
        foreach (var cardView in new List<CardView>(cardViewToIdMap.Keys)) {
            if (cardView != null) {
                cardView.OnClicked -= HandleCardViewClicked;
                cardView.OnHoverChanged -= HandleCardViewHoverChanged;
            }
        }
        cardViewToIdMap.Clear();
    }

    #endregion

    #region Debug

    [ContextMenu("Debug: Print Card Mappings")]
    private void DebugPrintMappings() {
        Debug.Log($"=== Card Mappings ({cardViewToIdMap.Count}) ===");
        foreach (var kvp in cardViewToIdMap) {
            Debug.Log($"{kvp.Key.name} -> {kvp.Value}");
        }
    }

    #endregion
}