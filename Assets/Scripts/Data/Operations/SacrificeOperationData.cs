using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Sacrifice", menuName = "Operations/Sacrifice")]
public class SacrificeOperationData : OperationData<SummonCreatureOperation> {

}

public class SacrificeCreatureOperation : GameOperation {
    private const string TargetCreatureKey = "targetCreature";
    public SacrificeCreatureOperation(UnitModel source, Zone zone = null) : base(source) {
        TargetRequirement<Creature> targetRequirement;
        if (zone != null) {
            targetRequirement = new RequirementBuilder<Creature>()
                .WithCondition(new ZoneCondition(zone))
                .Build();
        } else {
            targetRequirement = new RequirementBuilder<Creature>().Build();
        }

        SimpleTargetInstruction simpleTargetInstruction = new SimpleTargetInstruction("Select creature to sacrifice");
        AddTarget(new TargetInfo(TargetCreatureKey, targetRequirement, simpleTargetInstruction));
    }

    public override async UniTask<bool> Execute() {
        if (!TryGetTypedTarget(TargetCreatureKey, out Creature creature)) {
            Debug.LogError($"Valid {TargetCreatureKey} not found");
            return false;
        }

        creature.Die();


        await UniTask.CompletedTask;
        return true;
    }
}