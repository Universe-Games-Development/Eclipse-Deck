using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zenject;

public interface IOperationFactory {
    TOperation Create<TOperation>(params object[] args) where TOperation : GameOperation;
    GameOperation Create(OperationData data, UnitModel source);
}

public class OperationFactory : IOperationFactory {
    private readonly DiContainer _container;

    public OperationFactory(DiContainer container) {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }


    public GameOperation Create(OperationData data, UnitModel source) {
        if (data == null) throw new ArgumentNullException(nameof(data));

        return data.CreateOperation(this, source);
    }

    public TOperation Create<TOperation>(params object[] args) where TOperation : GameOperation {
        return _container.Instantiate<TOperation>(args);
    }
}