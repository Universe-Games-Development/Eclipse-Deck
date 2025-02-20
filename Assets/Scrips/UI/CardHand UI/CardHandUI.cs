using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CardHandUI : MonoBehaviour {
    Dictionary<CardUI, Card> cardPairs = new();

    //private UICardFactory cardFactory;
    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private Transform ghostLayoutParent;

    [SerializeField] private CardUI cardPrefab;
    [SerializeField] private CardLayoutGhost ghostPrefab;

    private CardHand cardHand;

    private void Awake() {
        //cardFactory = GetComponent<UICardFactory>();
        //if (cardFactory == null) {
        //    Debug.LogError("UICardFactory not found on this GameObject!");
        //}
    }


    public void Initialize(CardHand hand) {
        cardHand = hand ?? throw new System.ArgumentNullException(nameof(hand));

        hand.OnCardAdd += CreateCardUI;
        hand.OnCardRemove += (Card card) => {
            RemoveCardUIFor(card).Forget();
        };
    }

    #region ADD / REMOVE
    public void CreateCardUI(Card card) {
        CardUI cardUI = CreateNewCardUI(card);
        AttachCardEvents(cardUI, card);
        UpdateCardsPositionsAsync().Forget();
    }

    private CardUI CreateNewCardUI(Card card) {
        CardUI cardUI = Instantiate(cardPrefab, cardSpawnPoint);
        CardLayoutGhost ghost = Instantiate(ghostPrefab, ghostLayoutParent);
        cardUI.DoTweenAnimator.CardLayoutGhost = ghost;

        //cardFactory.CreateCardUI(card);
        card.cardUI = cardUI;
        cardPairs.Add(cardUI, card);
        return cardUI;
    }

    private void AttachCardEvents(CardUI cardUI, Card card) {
        cardUI.OnCardClicked += OnCardSelection;
    }

    private void OnCardSelection(CardUI cardUI) {
        Card card = cardPairs.GetValueOrDefault(cardUI);
        if (card == null) {
            Debug.LogError("Selecting null card");
        }
        cardHand.SelectCard(card);
    }

    private async UniTask RemoveCardUIFor(Card card) {
        CardUI cardUI = card.cardUI;
        if (cardUI == null) Debug.LogWarning("Can`t find card to Remove");

        await cardUI.RemoveCardUI(); // Card will define when it released to pool
        cardPairs.Remove(cardUI);
        UpdateCardsPositionsAsync().Forget();

        
        //cardFactory.ReleaseCardUI(cardUI);
        card.cardUI = null;
    }


    public void DeselectCurrentCard() {
        cardHand.DeselectCurrentCard();
    }
    #endregion

    private CancellationTokenSource updatePositionCts;

    private async UniTask UpdateCardsPositionsAsync(int delayFrames = 3) {
        updatePositionCts?.Cancel();

        var newCts = new CancellationTokenSource();
        updatePositionCts = newCts;

        await UniTask.DelayFrame(delayFrames, cancellationToken: updatePositionCts.Token);

        try {
            foreach (var cardUI in cardPairs.Keys) {
                if (newCts.IsCancellationRequested) return;
                await UniTask.NextFrame(newCts.Token);
                cardUI.UpdateLayout();
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

    private void OnDestroy() {
        if (cardHand != null) {
            cardHand.OnCardAdd -= CreateCardUI;
        }

        foreach (var pair in cardPairs) {
            pair.Key.OnCardClicked -= OnCardSelection;
        }

        updatePositionCts?.Cancel();
        updatePositionCts?.Dispose();
    }
}
