using System;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions {

    public static bool TryGetRandomElement<T>(this List<T> list, out T value) {
        value = default;
        if (list == null || list.Count == 0) {
            Debug.LogWarning("List is null or empty");
            return false;
        }
        
        int index = UnityEngine.Random.Range(0, list.Count);
        value = list[index];
        return true;
    }

    public static void Shuffle<T>(this List<T> list) {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--) {
            int j = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
