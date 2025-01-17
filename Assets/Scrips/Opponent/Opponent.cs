using System;
using UnityEngine;
using Zenject;

public class Opponent {
    public string Name = "Opponent";
    public Health health;

    public CardHand hand;

    public Deck deck;
    public Deck discardDeck;

    public CardCollection cardCollection;

    private ResourceManager resourceManager;
    private IEventManager eventManager;

    public Action<Opponent> OnDefeat { get; internal set; }

    [Inject]
    public Opponent(IEventManager eventManager, ResourceManager resourceManager) {
        this.eventManager = eventManager;
        this.resourceManager = resourceManager;
        Initialize();
    }

    protected virtual void Initialize() {
        health = new Health(0, 20);
        cardCollection = new CardCollection(resourceManager);
        cardCollection.GenerateTestDeck(20);

        deck = new Deck(this, cardCollection, eventManager);
        Debug.Log("deck initialized with cards : " + deck.GetCount());

        hand = new CardHand(this, eventManager);
    }

    public Card GetTestCard() {
        return hand.GetRandomCard();
    }
}
