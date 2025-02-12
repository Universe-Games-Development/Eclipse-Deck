using System;

public interface ISignalBus : ISignalRegistrar, ISignalSender {
    public void AddChildren(ISignalBus signalBus);
    public void RemoveChildren(ISignalBus signalBus);
    public void ClearSubscriptions();
}

public interface ISignalSender {
    public void Send<TSignal>(TSignal signal) where TSignal : class;
    public void Send<TSignal>() where TSignal : class;
    public void Send(Type signalType, object signal);
    public void Broadcast<TSignal>(TSignal signal) where TSignal : class;
    public void Broadcast<TSignal>() where TSignal : class;
    public void Broadcast(Type signalType, object signal);
}
public interface ISignalRegistrar {
    public object Register<TSignal>(Action<TSignal> action, int order = int.MaxValue)
        where TSignal : class;
    public object Register<TSignal>(Action action, int order = int.MaxValue)
        where TSignal : class;
    public void Register<TSignal>(Action<TSignal> action, object token, int order = int.MaxValue)
        where TSignal : class;
    public void Register<TSignal>(Action action, object token, int order = int.MaxValue)
        where TSignal : class;
    public void Deregister<TSignal>(Action action) where TSignal : class;
    public void Deregister<TSignal>(Action<TSignal> action) where TSignal : class;
    public void Deregister(object token);
}