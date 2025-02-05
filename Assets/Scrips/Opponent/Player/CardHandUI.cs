using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;
using System; // Для використання LINQ

public class CardHandUI : MonoBehaviour {
    // Приватні поля з [SerializeField]
    [SerializeField] private ObjectDistributer cardDistributer;
    [SerializeField] private ObjectDistributer ghostDistributer;

    private Dictionary<string, CardUI> idToCardUIMap = new();
    private Dictionary<CardUI, RectTransform> cardToLayoutMap = new();
    private UICardFactory cardFactory;

    public CardUI SelectedCard { get; private set; }

    [Inject] private UIManager uiManager;

    private void Awake() {
        if (cardDistributer == null || ghostDistributer == null) {
            Debug.LogError("Object distributors are not set!");
            return;
        }
        cardFactory = new UICardFactory(cardDistributer, ghostDistributer);
    }

    internal void Initialize(CardHand hand) {
        if (hand == null) {
            Debug.LogError("CardHand is null!");
            return;
        }

        hand.OnCardAdd += AddCard;
        hand.OnCardRemove += RemoveCard;
    }

    #region ADD / REMOVE

    public void AddCard(Card card) {
        if (card == null) {
            Debug.LogError("Card is null!");
            return;
        }

        if (idToCardUIMap.ContainsKey(card.Id)) {
            Debug.LogWarning($"Card with ID {card.Id} already exists in UI.");
            return;
        }

        var cardUI = cardFactory.CreateCard(card);
        var ghostElement = cardFactory.CreateLayoutElement();

        if (cardUI == null || ghostElement == null) {
            Debug.LogError("Failed to create CardUI or LayoutElement!");
            return;
        }

        idToCardUIMap[card.Id] = cardUI;
        LinkCardToGhostElement(cardUI, ghostElement);
        SubscribeToCardEvents(cardUI);

        UpdateCardsPositions();
    }

    private void UpdateCardsPositions() {
        foreach (var kvp in cardToLayoutMap) {
            CardUI cardUI = kvp.Key;
            RectTransform layoutElement = kvp.Value;

            cardUI.UpdatePosition(layoutElement.p);
        }
    }

    public void RemoveCard(Card card) {
        if (card == null) {
            Debug.LogError("Card is null!");
            return;
        }

        if (!idToCardUIMap.TryGetValue(card.Id, out CardUI cardUI)) {
            Debug.LogWarning($"Card with ID {card.Id} can't be found in UI!");
            return;
        }

        if (cardToLayoutMap.TryGetValue(cardUI, out RectTransform layoutElement)) {
            ghostDistributer.ReleaseObject(layoutElement.gameObject);
            cardToLayoutMap.Remove(cardUI);
        }

        UnsubscribeCardEvents(cardUI);
        idToCardUIMap.Remove(card.Id);
        cardDistributer.ReleaseObject(cardUI.gameObject);

        UpdateCardsPositions();
    }

    #endregion

    #region ASSIST METHODS

    private void LinkCardToGhostElement(CardUI cardUI, RectTransform layoutElement) {
        if (cardUI == null || layoutElement == null) {
            Debug.LogError("CardUI or LayoutElement is null!");
            return;
        }

        if (cardUI.TryGetComponent(out SmoothLayoutElement smoothLayoutElement)) {
            smoothLayoutElement.Initialize(layoutElement);
        }

        cardUI.SetOriginPosition(smoothLayoutElement.transform.position);
        cardToLayoutMap[cardUI] = layoutElement;
    }

    private void SubscribeToCardEvents(CardUI cardUI) {
        if (cardUI == null) {
            Debug.LogError("CardUI is null!");
            return;
        }

        cardUI.OnCardClicked += OnCardSelected;
    }

    private void UnsubscribeCardEvents(CardUI cardUI) {
        if (cardUI == null) {
            Debug.LogError("CardUI is null!");
            return;
        }

        cardUI.OnCardClicked -= OnCardSelected;
    }

    private CardUI GetCardUIById(string id) {
        return idToCardUIMap.TryGetValue(id, out CardUI cardUI) ? cardUI : null;
    }

    #endregion

    #region SELECT / DESELECT

    private void OnCardSelected(CardUI clickedCard) {
        if (clickedCard == null) {
            Debug.LogError("Clicked card is null!");
            return;
        }

        if (SelectedCard == clickedCard) {
            return; // Do nothing if the clicked card is already selected
        }

        SelectedCard?.HandleDeselection();
        clickedCard.HandleSelection();
        SelectedCard = clickedCard;

        Debug.Log($"Selected card: {SelectedCard.name}");
    }

    public void DeselectCurrentCard() {
        if (SelectedCard != null) {
            SelectedCard.HandleDeselection();
            SelectedCard = null;
        }
    }

    #endregion
}