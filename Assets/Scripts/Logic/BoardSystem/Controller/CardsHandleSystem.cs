using Cysharp.Threading.Tasks;
using ModestTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CardsHandleSystem : MonoBehaviour {
    
    [Inject] IEventBus<IEvent> _eventBus;
    [Inject] CommandManager _commandManager;
    [Inject] CardProvider _cardProvider;
    [Inject] ICardFactory<Card3DView> _cardFactory;

    private BoardPlayerPresenter playerPresenter;

    public void Initialize(BoardPlayerPresenter boardPlayer) {
        playerPresenter = boardPlayer;
        CharacterData data = boardPlayer.Opponent.Data; // soon opponent data will define deck and cards

        // Move to the global game manager
        
    }

    public void StartBattleActions(ref BattleStartedEvent eventData) {
        _eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
        _eventBus.UnsubscribeFrom<BattleEndEventData>(EndBattleActions);

        _eventBus.SubscribeTo<TurnStartEvent>(TurnStartActions);

        Deck deck = playerPresenter.Opponent.Deck;
        List<Card> _cards = GenerateRandomCards(40);
        deck.AddRange(_cards);
    }

    protected virtual void TurnStartActions(ref TurnStartEvent eventData) {
        if (eventData.StartingOpponent == playerPresenter)
            _commandManager.EnqueueCommand(new DrawCardCommand(playerPresenter));
    }

    private void EndBattleActions(ref BattleEndEventData eventData) {
        _eventBus.UnsubscribeFrom<TurnStartEvent>(TurnStartActions);
        playerPresenter.Opponent.Deck.Clear();
        playerPresenter.Opponent.Hand.Clear();
    }
    

    public List<Card> GenerateRandomCards(int cardAmount) {
        CardCollection collection = new();
        List<CardData> _unclokedCards = _cardProvider.GetRandomUnlockedCards(cardAmount);
        if (_unclokedCards.IsEmpty()) return new List<Card>();
        
        for (int i = 0; i < cardAmount; i++) {
            var randomIndex = UnityEngine.Random.Range(0, _unclokedCards.Count);
            var randomCard = _unclokedCards[randomIndex];
            collection.AddCardToCollection(randomCard);
        }

        List<Card> cards = new();
        foreach (var cardEntry in collection.cardEntries) {
            for (int i = 0; i < cardEntry.Value; i++) {
                CardData cardData = cardEntry.Key;
                Card newCard = _cardFactory.CreateCard(cardData);
                if (newCard == null) continue;
                cards.Add(newCard);
            }
        }
        return cards;
    }

    

    private void OnDestroy() {
        if (_eventBus != null) {
            _eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
            _eventBus.UnsubscribeFrom<BattleEndEventData>(EndBattleActions);

            _eventBus.UnsubscribeFrom<TurnStartEvent>(TurnStartActions);
        }
    }
}
