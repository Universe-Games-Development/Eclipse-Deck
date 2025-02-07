using System.Collections.Generic;
using UnityEngine;
public class CardCollection {
    public List<CardEntry> cardEntries;
    private AssetLoader assetLoader;
    public CardCollection(AssetLoader assetLoader) {
        this.assetLoader = assetLoader;
        cardEntries = new List<CardEntry>();
    }

    // Додавання карти в колекцію з обліком кількості
    private void AddCardToCollection(CardSO card) {
        if (card == null) {
            Debug.LogWarning("Collection generating null card!");
            return;
        }
        // Перевірка чи карта вже є в колекції
        CardEntry existingEntry = cardEntries.Find(entry => entry.cardSO == card);
        if (existingEntry != null) {
            existingEntry.quantity++;
        } else {
            cardEntries.Add(new CardEntry { cardSO = card, quantity = 1 });
        }
    }

    public void GenerateTestCollection(int count) {
        List<CardSO> cardDatas = assetLoader.CardManager.GetAllCards();

        if (cardDatas == null || cardDatas.Count == 0) {
            Debug.LogWarning("Generating null collection");
            return;
        }
        for (int i = 0; i < count; i++) {
            // Get random Card datas for test
            CardSO cardData = RandomUtil.GetRandomFromList(cardDatas);
            AddCardToCollection(cardData);
        }
    }

    public class CardEntry {
        public CardSO cardSO;
        public int quantity;
    }
}
