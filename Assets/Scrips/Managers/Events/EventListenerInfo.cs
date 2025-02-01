public class EventListenerInfo {
    public readonly IEventListener Listener;
    public readonly int ExecutionOrder;

    public EventListenerInfo(IEventListener listener, int executionOrder) {
        Listener = listener;
        ExecutionOrder = executionOrder;
    }
}