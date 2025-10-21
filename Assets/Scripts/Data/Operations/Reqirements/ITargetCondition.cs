using System;
using System.Collections.Generic;

public static class ConditionExtensions {
    public static ITargetCondition<T> And<T>(this ITargetCondition<T> first, ITargetCondition<T> second)
        => new AndTargetCondition<T>(first, second);

    public static ITargetCondition<T> Or<T>(this ITargetCondition<T> first, ITargetCondition<T> second)
        => new OrTargetCondition<T>(first, second);
}
