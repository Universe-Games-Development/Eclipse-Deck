using System;
using System.Collections.Generic;

public static class ListExtensions {

    // Returns Random element from the list
    public static T GetRandomElement<T>(this List<T> list) {
        if (list == null || list.Count == 0) {
            throw new InvalidOperationException("Cannot get a random element from an empty or null list.");
        }
        int index = UnityEngine.Random.Range(0, list.Count);
        return list[index];
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
