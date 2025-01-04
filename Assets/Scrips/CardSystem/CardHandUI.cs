using System;
using System.Collections.Generic;
using UnityEngine;

public class CardHandUI : MonoBehaviour {
    public Transform cardParent;
    public Transform layoutParent;
    public GameObject layoutElementPrefab;
    [SerializeField] private GameObject cardPrefab;

    private Dictionary<string, CardUI> idToCardUIMap = new();
    private Dictionary<CardUI, RectTransform> cardToLayoutMap = new();
    public CardUI SelectedCard { get; private set; }

    private void Awake() {
        if (!cardParent || !layoutParent) {
            Debug.LogError("CardParent or LayoutParent не задано!");
        }
    }

    internal void Initialize(CardHand hand) {
        hand.OnCardAdd += AddCard;
        hand.OnCardRemove += RemoveCard;
    }

    #region ADD / REMOVE
    public void AddCard(Card card) {
        var cardUI = CreateCardUI(card);
        var emptyLayoutCopy = CreateLayoutElement();

        idToCardUIMap[card.Id] = cardUI; // Зберігаємо зв’язок між id та CardUI
        LinkCardToLayoutElement(cardUI, emptyLayoutCopy);
        SubscribeToCardEvents(cardUI);
    }

    public void RemoveCard(Card card) {
        if (!idToCardUIMap.TryGetValue(card.Id, out CardUI cardUI)) {
            Debug.LogWarning($"Card з ID {card.Id} не знайдено в UI!");
            return;
        }

        if (cardToLayoutMap.TryGetValue(cardUI, out RectTransform layoutElement)) {
            Destroy(layoutElement.gameObject);
            cardToLayoutMap.Remove(cardUI);
        }

        if (cardUI == SelectedCard) {
            DeselectCurrentCard();
        }

        UnsubscribeCardEvents(cardUI);
        idToCardUIMap.Remove(card.Id); // Видаляємо запис про карту
        Destroy(cardUI.gameObject);

        Debug.Log($"Card {card} була успішно видалена з UI.");
    }
    #endregion

    #region ASSIST Methods
    private CardUI CreateCardUI(Card card) {
        var cardUIObj = Instantiate(cardPrefab, cardParent);
        var cardUI = cardUIObj.GetComponent<CardUI>();
        cardUI.Initialize(card);
        cardUI.transform.SetParent(cardParent);
        return cardUI;
    }

    private RectTransform CreateLayoutElement() {
        GameObject layoutElementObject = Instantiate(layoutElementPrefab, layoutParent);
        return layoutElementObject.GetComponent<RectTransform>();
    }

    private void LinkCardToLayoutElement(CardUI cardUI, RectTransform layoutElement) {
        if (cardUI.TryGetComponent(out SmoothLayoutElement smoothLayoutElement)) {
            smoothLayoutElement.Initialize(layoutElement);
        }
        cardToLayoutMap[cardUI] = layoutElement;
    }

    private void SubscribeToCardEvents(CardUI cardUI) {
        cardUI.OnCardClicked += OnCardSelected;
    }

    private void UnsubscribeCardEvents(CardUI cardUI) {
        cardUI.OnCardClicked -= OnCardSelected;
    }

    private CardUI GetCardUIById(string id) {
        idToCardUIMap.TryGetValue(id, out CardUI cardUI);
        return cardUI;
    }
    #endregion

    #region SELECT / DESELECT
    private void OnCardSelected(CardUI clickedCard) {
        if (SelectedCard != null && SelectedCard != clickedCard) {
            SelectedCard.DeselectCard();
        }

        clickedCard.SelectCard();
        SelectedCard = clickedCard;
        Debug.Log($"Selected card: {SelectedCard.name}");
    }

    public void DeselectCurrentCard() {
        if (SelectedCard != null) {
            SelectedCard.DeselectCard();
            SelectedCard = null;
            Debug.Log("Card deselected.");
        }
    }
    #endregion

    public event Action OnHandUpdated;

    public void UpdateHand() {
        foreach (var pair in cardToLayoutMap) {
            CardUI card = pair.Key;
            RectTransform layoutElement = pair.Value;
            card.transform.SetSiblingIndex(layoutElement.GetSiblingIndex());
        }

        OnHandUpdated?.Invoke();
    }
}
