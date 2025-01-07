using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CardHandUI : MonoBehaviour {

    private Dictionary<string, CardUI> idToCardUIMap = new();
    private Dictionary<CardUI, RectTransform> cardToLayoutMap = new();
    public CardUI SelectedCard { get; private set; }
    [Inject] private UIManager uiManager;

    [SerializeField] private ObjectDistributer cardDistributer;
    [SerializeField] private ObjectDistributer layoutDistributer;  // Новий дистриб'ютор для layout

    private void Awake() {
        if (cardDistributer == null || layoutDistributer == null) {
            Debug.LogError("Object distributors are not set!");
            return; // Ensure distributors are set
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
        if (idToCardUIMap.ContainsKey(card.Id)) {
            Debug.LogWarning($"Card with ID {card.Id} already exists in UI.");
            return;
        }

        idToCardUIMap[card.Id] = cardUI; // Зберігаємо зв’язок між id та CardUI
        LinkCardToLayoutElement(cardUI, emptyLayoutCopy);
        SubscribeToCardEvents(cardUI);
    }

    public void RemoveCard(Card card) {
        if (!idToCardUIMap.TryGetValue(card.Id, out CardUI cardUI)) {
            Debug.LogWarning($"Card з ID {card.Id} can`t find in UI!");
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
    }
    #endregion

    #region ASSIST Methods
    private CardUI CreateCardUI(Card card) {
        var cardUIObj = cardDistributer.CreateObject();  // Використовуємо дистриб'ютор карт

        // Прив'язуємо панель до даних
        var cardUI = cardUIObj.GetComponent<CardUI>();
        cardUI.Initialize(cardDistributer, card);

        return cardUI;
    }

    private RectTransform CreateLayoutElement() {
        GameObject layoutElementObject = layoutDistributer.CreateObject();  // Використовуємо дистриб'ютор для розмітки
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
        if (SelectedCard == clickedCard) {
            return; // Do nothing if the clicked card is already selected
        }

        if (SelectedCard != null) {
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
