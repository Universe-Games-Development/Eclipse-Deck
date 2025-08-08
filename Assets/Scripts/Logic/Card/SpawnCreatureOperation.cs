using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class Zone : GameUnit {
}

public class SpawnCreatureOperation : GameOperation {
    private CreatureCardData creatureCardData;
    private string spawnZone = "spawnZone";

    public SpawnCreatureOperation(CreatureCardData creatureCardData) {
        this.creatureCardData = creatureCardData;
        NamedTargets.Add(new NamedTarget(spawnZone, new ZoneRequirement(new OwnershipCondition<Zone>(OwnershipType.Friendly))));
    }


    public async override Task<bool> ExecuteAsync(CancellationToken cancellationToken) {
        await UniTask.Yield(); // Simulate async operation
        GameUnit gameUnit = targets[spawnZone];
        Debug.Log($"spawning craeture at zone: {gameUnit}");
        return true;
    }
}