using System;
using System.Collections.Generic;

public static class RandomUtil {
    private static readonly Random rnd = new();
    public static T GetRandomFromList<T>(List<T> list) where T : class {
        if (list == null || list.Count == 0) {
            return null;
        }

        int r = rnd.Next(list.Count);
        return list[r];
    }

}
