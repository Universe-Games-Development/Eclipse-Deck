using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Deck {
    private Stack<Card> deck = new();
    private IEventQueue eventQueue;

    private Opponent owner;

    public Deck(Opponent owner, CardCollection collection, IEventQueue eventQueue) {
        this.owner = owner;
        this.eventQueue = eventQueue;
        InitializeDeck(collection);
    }

    private void InitializeDeck(CardCollection collection) {
        foreach (var cardEntry in collection.cardEntries) {
            for (int i = 0; i < cardEntry.quantity; i++) {
                CardSO cardSO = cardEntry.cardSO;

                Card newCard;
                switch (cardSO.cardType) {
                    case CardType.CREATURE:
                        CreatureCardSO creatureCardData = (CreatureCardSO)cardSO;
                        newCard = new CreatureCard(creatureCardData, owner, eventQueue);
                        break;
                    case CardType.SPELL:
                        SpellCardSO spellCardSO = (SpellCardSO)cardSO;
                        newCard = new SpellCard(spellCardSO, owner, eventQueue);
                        break;
                    case CardType.SUPPORT:
                        SupportCardSO supportCardSO = (SupportCardSO)cardSO;
                        newCard = new SupportCard(supportCardSO, owner, eventQueue);
                        break;
                    default:
                        Debug.LogWarning("Wrong CardType for card initializations");
                        continue;
                }

                AddCard(newCard);
                newCard.ChangeState(CardState.InDeck);
            }
        }
        ShuffleDeck();
    }


    public Card DrawCard() {
        return deck.Count > 0 ? deck.Pop() : null;
    }

    public void ShuffleDeck() {
        var cards = deck.ToArray();
        deck.Clear();
        foreach (var card in cards.OrderBy(x => UnityEngine.Random.value)) {
            deck.Push(card);
        }
    }

    public void AddCard(Card card) {
        deck.Push(card);
    }

    public void CleanDeck() {
        deck.Clear();
    }

    public int GetCount() {
        return deck.Count();
    }
}
