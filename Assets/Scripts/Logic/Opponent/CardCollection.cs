using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class CardCollection {
    public Dictionary<CardData, int> cardEntries = new();

    public void AddCardToCollection(CardData cardData) {
        if (cardData == null) {
            Debug.LogWarning("Collection adding null card!");
            return;
        }

        if (cardEntries.TryGetValue(cardData, out int quantity)) {
            cardEntries[cardData] = quantity + 1;
        } else {
            cardEntries[cardData] = 1;
        }
    }

    // Видалення карти з колекції
    public void RemoveCardFromCollection(CardData cardData) {
        if (cardData == null) {
            Debug.LogWarning("Collection removing null card!");
            return;
        }

        if (cardEntries.TryGetValue(cardData, out int quantity)) {
            if (quantity > 1) {
                cardEntries[cardData] = quantity - 1;
            } else {
                cardEntries.Remove(cardData);  // ✅ Delete card key if it last
            }
        } else {
            Debug.LogWarning("Collection removing non-existent card!");
        }
    }
}
