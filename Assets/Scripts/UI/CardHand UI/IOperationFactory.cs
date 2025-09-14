using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zenject;

public interface IOperationFactory {
    bool CanCreate(Type dataType);
    TOperation Create<TOperation>(OperationData data) where TOperation : GameOperation;
    GameOperation Create(OperationData data);
}

public class OperationFactory : IOperationFactory {
    private readonly DiContainer _container;
    private readonly Dictionary<Type, Type> _dataToOperationMap = new();

    public OperationFactory(DiContainer container) {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        Initialize();
    }

    private void Initialize() {
        var assembly = Assembly.GetExecutingAssembly();
        var operationTypes = assembly.GetTypes()
            .Where(IsValidOperationType)
            .ToList();

        foreach (var operationType in operationTypes) {
            RegisterOperationType(operationType);
        }

        Console.WriteLine($"OperationFactory: Registered {_dataToOperationMap.Count} operation types");
    }

    private static bool IsValidOperationType(Type type) {
        return type.IsClass &&
               !type.IsAbstract &&
               typeof(GameOperation).IsAssignableFrom(type) &&
               type.GetCustomAttribute<OperationForAttribute>() != null;
    }

    private void RegisterOperationType(Type operationType) {
        var attribute = operationType.GetCustomAttribute<OperationForAttribute>();

        if (!typeof(OperationData).IsAssignableFrom(attribute.DataType)) {
            Console.WriteLine($"Invalid data type {attribute.DataType} for operation {operationType}");
            return;
        }

        if (_dataToOperationMap.ContainsKey(attribute.DataType)) {
            Console.WriteLine($"Duplicate operation registration for {attribute.DataType}");
            return;
        }

        _dataToOperationMap[attribute.DataType] = operationType;
    }

    public bool CanCreate(Type dataType) {
        return _dataToOperationMap.ContainsKey(dataType);
    }

    public GameOperation Create(OperationData data) {
        if (data == null) throw new ArgumentNullException(nameof(data));

        var dataType = data.GetType();
        if (!_dataToOperationMap.TryGetValue(dataType, out var operationType)) {
            throw new InvalidOperationException($"No operation registered for {dataType.Name}");
        }

        return (GameOperation)_container.Instantiate(operationType, new object[] { data });
    }

    public TOperation Create<TOperation>(OperationData data) where TOperation : GameOperation {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return _container.Instantiate<TOperation>(new object[] { data });
    }
}


[System.AttributeUsage(System.AttributeTargets.Class)]
public class OperationForAttribute : System.Attribute {
    public System.Type DataType { get; }
    public OperationForAttribute(System.Type dataType) {
        DataType = dataType;
    }
}