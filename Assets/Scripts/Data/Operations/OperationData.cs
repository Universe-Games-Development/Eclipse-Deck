using System.Collections.Generic;
using UnityEngine;

public abstract class OperationData : ScriptableObject {
    [SerializeReference] public List<TargetRequirementData> requirements = new();
    [SerializeReference] public VisualData visualData;

    public abstract GameOperation CreateOperation(IOperationFactory factory, UnitModel source);
}

// Generic проміжний клас (для типобезпеки)
public abstract class OperationData<TOperation> : OperationData
  where TOperation : GameOperation {
    public sealed override GameOperation CreateOperation(IOperationFactory factory, UnitModel source) {
        return factory.Create<TOperation>(this, source);
    }
}
