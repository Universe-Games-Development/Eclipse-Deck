using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CardManager {

    private Dictionary<Location, List<CardSO>> loadedCards = new Dictionary<Location, List<CardSO>>();
    public async UniTask LoadCardsForLocation(AssetLabelReference locationLabel, Location location) {
        if (!loadedCards.ContainsKey(location)) {
            loadedCards[location] = new List<CardSO>();
            string key = location.ToString();
            await Addressables.LoadAssetsAsync<CardSO>(locationLabel, cards => {
                loadedCards[location].Add(cards);
            });
        }
    }

    public List<CardSO> GetCardsForLocation(Location location) {
        if (!HasLocationCardData(location)) return new List<CardSO>(); ;

        return loadedCards.TryGetValue(location, out var cards) ? cards : new List<CardSO>();
    }

    public void UnloadCards(Location location) {
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

    public bool HasLocationCardData(Location location) {
        return loadedCards.ContainsKey(location);
    }

    public List<CardSO> GetAllCards() {
        List<CardSO> allCards = new List<CardSO>();

        foreach (var kvp in loadedCards) {
            if (kvp.Value != null && kvp.Value.Count > 0) {
                allCards.AddRange(kvp.Value);
            }
        }

        return allCards;
    }

}
