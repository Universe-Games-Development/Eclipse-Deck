using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks; // Для використання LINQ

public class CardHandUI : MonoBehaviour {
    // Приватні поля з [SerializeField]
    private Dictionary<string, CardUI> idToCardUIMap = new();
    private UICardFactory cardFactory;
    public CardUI SelectedCard { get; private set; }

    private CardHand cardHand;

    private void Awake() {
        cardFactory = GetComponent<UICardFactory>();
        if (cardFactory == null) {
            Debug.LogError("UICardFactory not found on this GameObject!");
        }
    }

    internal void Initialize(CardHand hand) {
        cardHand = hand ?? throw new System.ArgumentNullException(nameof(hand));

        hand.OnCardAdd += CreateCardUI;
        hand.OnCardRemove += RemoveCardUI;
    }

    #region ADD / REMOVE

    public void CreateCardUI(Card card) {
        if (idToCardUIMap.ContainsKey(card.Id)) {
            Debug.LogWarning($"Card with ID {card.Id} already exists in UI.");
            return;
        }

        CardUI cardUI = cardFactory.CreateCardUI(card);
        updatePositionQu.Enqueue(cardUI);
        idToCardUIMap.Add(card.Id, cardUI);
        

        UpdateCardsPositionsAsync().Forget(); // Use Forget() with care, ensure no exceptions are swallowed silently

        cardUI.OnCardClicked += OnCardSelected;
    }

    private Queue<CardUI> updatePositionQu = new();
    private async UniTask UpdateCardsPositionsAsync() {
        while(updatePositionQu.Count > 0) {
            await UniTask.NextFrame();
            if (updatePositionQu.Count > 0) {
                updatePositionQu.Dequeue().UpdateLayout();
            }
        }
    }

    public void RemoveCardUI(Card card) {
        if (card == null) {
            Debug.LogError("Card is null!");
            return;
        }

        if (!idToCardUIMap.TryGetValue(card.Id, out var cardUI)) {
            Debug.LogWarning($"Card with ID {card.Id} can't be found in UI!");
            return;
        }

        idToCardUIMap.Remove(card.Id); // More efficient than TryRemove in this case

        cardUI.OnCardClicked -= OnCardSelected;
        cardFactory.ReleaseCardUI(cardUI); // Important: Release to pooling if you have it
    }


    #endregion

    #region SELECT / DESELECT

    private void OnCardSelected(CardUI clickedCard) {
        if (clickedCard == null) {
            Debug.LogError("Clicked card is null!");
            return;
        }

        if (SelectedCard == clickedCard) return;

        SelectedCard?.HandleDeselection();
        SelectedCard = clickedCard;
        SelectedCard.HandleSelection();

        Debug.Log($"Selected card: {SelectedCard.name}");
    }


    public void DeselectCurrentCard() {
        SelectedCard?.HandleDeselection(); // Use null-conditional operator
        SelectedCard = null;
    }

    #endregion
}

