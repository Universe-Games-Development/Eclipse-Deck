using System;

public class Subscription {
    public readonly Action<object> action;
    public readonly int order;
    public readonly object token;
    public readonly object extraToken;

    public Subscription(Action<object> action, object token, object extraToken, int order = int.MaxValue) {
        this.action = action;
        this.order = order;
        this.token = token;
        this.extraToken = extraToken;
    }
}