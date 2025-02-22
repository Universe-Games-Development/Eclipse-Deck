using System;
using System.Collections.Generic;

public static class ListExtensions {
    private static readonly Random random = new Random();

    public static T GetRandomElement<T>(this List<T> list) {
        if (list == null || list.Count == 0) {
            throw new InvalidOperationException("Cannot get a random element from an empty or null list.");
        }
        return list[random.Next(list.Count)];
    }
}
