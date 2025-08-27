using System;
using System.Collections.Generic;
using System.Linq;

public static class StackExtensions {
    public static List<T> RemoveAll<T>(this Stack<T> stack, Predicate<T> match) {
        List<T> removedItems = new List<T>();
        var tempList = stack.ToList();
        stack.Clear();
        foreach (var item in tempList) {
            if (!match(item)) {
                stack.Push(item);
            } else {
                removedItems.Add(item);
            }
        }
        return removedItems;
    }
}