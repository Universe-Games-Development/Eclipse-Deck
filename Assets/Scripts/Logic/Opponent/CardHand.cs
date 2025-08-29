using System;
using System.Collections.Generic;
using UnityEngine;

public class CardHand : UnitInfo {
    private CardContainer cardContainer;

    public CardHand() {
        cardContainer = new();
        cardContainer.OnCardAdded += (card) => OnCardAdded?.Invoke(card);
        cardContainer.OnCardRemoved += (card) => OnCardRemoved?.Invoke(card);
    }

    public Action<Card> OnCardAdded;
    public Action<Card> OnCardRemoved;
    public IEnumerable<Card> Cards { 
        get { 
            return cardContainer.Cards; 
        }
    }


    public void Clear() {
        cardContainer.Clear();
    }

    public bool Remove(Card card) {
        return cardContainer.Remove(card);
    }

    public bool Add(Card card) {
        return cardContainer.Add(card);
    }

    public bool Contains(Card card) {
        return cardContainer.Contains(card);
    }
}