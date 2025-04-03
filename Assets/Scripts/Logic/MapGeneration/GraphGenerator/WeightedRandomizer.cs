using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generic interface for objects that have a spawn chance/weight
/// </summary>
public interface IWeightable {
    float SpawnChance { get; }
}

/// <summary>
/// Generic randomizer that can work with any type implementing IWeightable
/// </summary>
/// <typeparam name="T">Any type that implements IWeightable</typeparam>
public class WeightedRandomizer<T> where T : IWeightable {
    private List<T> items;
    private Dictionary<T, float> weightPoints = new Dictionary<T, float>();
    private bool isInitialized = false;

    public WeightedRandomizer() { }

    public WeightedRandomizer(IEnumerable<T> itemsCollection) {
        UpdateItems(itemsCollection);
    }

    /// <summary>
    /// Updates the collection of items and recalculates weights
    /// </summary>
    public void UpdateItems(IEnumerable<T> itemsCollection) {
        if (itemsCollection == null || !itemsCollection.Any()) {
            Debug.LogError($"Empty collection provided to {typeof(T).Name} randomizer");
            isInitialized = false;
            return;
        }

        this.items = new List<T>(itemsCollection);
        ValidateItems();
        GenerateWeightPoints();
        isInitialized = true;
    }

    /// <summary>
    /// Validates the item collection for duplicates
    /// </summary>
    private void ValidateItems() {
        HashSet<T> itemSet = new HashSet<T>();
        foreach (var item in items) {
            if (!itemSet.Add(item)) {
                Debug.LogWarning($"Found duplicate in {typeof(T).Name} collection");
            }
        }
    }

    /// <summary>
    /// Calculates cumulative weight points for binary search selection
    /// </summary>
    private void GenerateWeightPoints() {
        float totalWeight = items.Sum(item => item.SpawnChance);
        if (totalWeight <= 0) {
            Debug.LogError($"Total weight for {typeof(T).Name} collection must be greater than zero!");
            isInitialized = false;
            return;
        }

        weightPoints.Clear();
        float cumulative = 0f;
        foreach (var item in items) {
            float normalizedWeight = item.SpawnChance / totalWeight;
            cumulative += normalizedWeight;
            weightPoints.Add(item, cumulative);
        }
    }

    /// <summary>
    /// Gets a random item based on weight distribution
    /// </summary>
    public T GetRandomItem() {
        if (!isInitialized || weightPoints.Count == 0) {
            Debug.LogError($"{typeof(T).Name} randomizer not properly initialized");
            return default;
        }

        float randomWeight = UnityEngine.Random.value;
        List<KeyValuePair<T, float>> itemList = weightPoints.ToList();

        // Binary search for the item
        int left = 0;
        int right = itemList.Count - 1;

        while (left <= right) {
            int mid = (left + right) / 2;
            if (randomWeight <= itemList[mid].Value) {
                if (mid == 0 || randomWeight > itemList[mid - 1].Value) {
                    return itemList[mid].Key;
                }
                right = mid - 1;
            } else {
                left = mid + 1;
            }
        }

        return itemList.Last().Key;
    }

    internal void UpdateItems(object commonActivities) {
        throw new NotImplementedException();
    }
}