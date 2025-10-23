using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sacrifice", menuName = "Operations/Sacrifice")]
public class SacrificeOperationData : OperationData {
    public override GameOperation CreateOperation(IOperationFactory factory, TargetRegistry targetRegistry) {
        Creature creature = targetRegistry.Get<Creature>(TargetKeys.MainTarget);
        return factory.Create<SacrificeCreatureOperation>(this, creature);
    }

    protected override void BuildDefaultRequirements() {
        CreatureTargetRequirementData creatureTargetRequirementData = new CreatureTargetRequirementData {
            targetKey = TargetKeys.MainTarget,
            selector = TargetSelector.Initiator,
            conditions = new List<ISerializableTargetCondition<Creature>> {
                        new AliveConditionData(),
                        new OwnershipConditionData { ownershipType = OwnershipType.Ally }
                    }
        };

        AddRequirement(creatureTargetRequirementData);
    }
}

public class SacrificeCreatureOperation : GameOperation {
    private Creature _creature;
    public SacrificeCreatureOperation(SacrificeOperationData data, Creature creature) {
        _creature = creature;
    }

    public override bool Execute() {
        _creature.Die();

        return true;
    }
}