using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class CardCollection {
    public Dictionary<CardData, int> cardEntries;
    private AssetLoader assetLoader;

    public CardCollection(AssetLoader assetLoader) {
        this.assetLoader = assetLoader;
        cardEntries = new();
    }

    private void AddCardToCollection(CardData cardData) {
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
                cardEntries[cardData] = quantity - 1;  // ✅ Виправлено
            } else {
                cardEntries.Remove(cardData);  // ✅ Видаляємо карту якщо залишилась 1
            }
        } else {
            Debug.LogWarning("Collection removing non-existent card!");
        }
    }

    // Генерація тестової колекції
    public async UniTask GenerateTestCollection(int count) {
        List<CardData> cardDatas;

        if (assetLoader.HasActualData(Location.Sewers)) {
            cardDatas = assetLoader.CardManager.GetAllCards();
        } else {
            await assetLoader.LoadLocationAssets(Location.Sewers);
            cardDatas = assetLoader.CardManager.GetAllCards();
        }

        if (cardDatas == null || cardDatas.Count == 0) {
            Debug.LogWarning("Generating null collection");
            return;
        }

        for (int i = 0; i < count; i++) {
            CardData cardData = RandomUtil.GetRandomFromList(cardDatas);
            AddCardToCollection(cardData);
        }
    }
}
