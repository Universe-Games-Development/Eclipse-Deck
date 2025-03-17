using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CardManager {

    private Dictionary<LocationType, List<CardData>> loadedCards = new Dictionary<LocationType, List<CardData>>();
    public async UniTask LoadCardsForLocation(AssetLabelReference locationLabel, LocationType location) {
        if (!loadedCards.ContainsKey(location)) {
            loadedCards[location] = new List<CardData>();
            string key = location.ToString();
            await Addressables.LoadAssetsAsync<CardData>(locationLabel, cards => {
                loadedCards[location].Add(cards);
            });
        }
    }

    public List<CardData> GetCardsForLocation(LocationType location) {
        if (!HasLocationCardData(location)) return new List<CardData>(); ;

        return loadedCards.TryGetValue(location, out var cards) ? cards : new List<CardData>();
    }

    public void UnloadCards(LocationType location) {
        if (loadedCards.TryGetValue(location, out var cards)) {
            foreach (var card in cards) {
                Addressables.Release(card);
            }
            loadedCards.Remove(location);
        }
    }

    public void UnloadAllCards() {
        foreach (var location in loadedCards.Keys) {
            UnloadCards(location);
        }
        loadedCards.Clear();
    }

    public bool HasLocationCardData(LocationType location) {
        return loadedCards.ContainsKey(location);
    }

    public List<CardData> GetAllCards() {
        List<CardData> allCards = new List<CardData>();

        foreach (var kvp in loadedCards) {
            if (kvp.Value != null && kvp.Value.Count > 0) {
                allCards.AddRange(kvp.Value);
            }
        }

        return allCards;
    }

    internal bool HasCardsLoaded(LocationType sewers) {
        throw new NotImplementedException();
    }
}
