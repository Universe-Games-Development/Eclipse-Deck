using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class Deck {
    private Stack<Card> cards = new();
    private GameEventBus eventBus;

    private Opponent owner;

    public Deck(Opponent owner, GameEventBus eventBus) {
        this.owner = owner;
        this.eventBus = eventBus;
    }

    public async UniTask Initialize(CardCollection collection) {
        await collection.GenerateTestCollection(20);

        foreach (var cardEntry in collection.cardEntries) {
            for (int i = 0; i < cardEntry.Value; i++) {
                CardData cardSO = cardEntry.Key;
                Card newCard = CreateCard(cardSO);
                if (newCard == null) continue;

                AddCard(newCard);
                newCard.ChangeState(CardState.InDeck);
            }
        }
        ShuffleDeck();
    }

    private Card CreateCard(CardData cardSO) {
        return cardSO switch {
            CreatureCardData creatureCard => new CreatureCard(creatureCard, owner, eventBus),
            SpellCardSO spellCard => new SpellCard(spellCard, owner, eventBus),
            SupportCardSO supportCard => new SupportCard(supportCard, owner, eventBus),
            _ => null
        };
    }


    public Card DrawCard() {
        return cards.Count > 0 ? cards.Pop() : null;
    }

    public void ShuffleDeck() {
        List<Card> tempCards = new(cards);
        cards.Clear();
        ShuffleList(tempCards);
        foreach (var card in tempCards) {
            cards.Push(card);
        }
    }

    private void ShuffleList(List<Card> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public void AddCard(Card card) {
        cards.Push(card);
    }

    public void CleanDeck() {
        cards.Clear();
    }

    public int GetCount() {
        return cards.Count;
    }
}
