using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class Zone : GameUnit {
    public Action OnCreatureSpawned;

    List<CreatureCard> creatures = new();
    public void AddCreature(CreatureCard creatureCard) {
        Debug.Log($"Spawning craeture at zone: {this}");

        creatures.Add(creatureCard);
       
        OnCreatureSpawned?.Invoke();
    }

    public int GetCreaturesCount() {
        return creatures.Count;
    }
}

public class SpawnCreatureOperation : GameOperation {
    private const string SpawnZoneKey = "spawnZone";
    private readonly CreatureCard _creatureCard;

    public SpawnCreatureOperation(CreatureCard creatureCard) {
        _creatureCard = creatureCard;
        OperationName = $"Spawn Creature";
        NamedTargets.Add(new NamedTarget(
            SpawnZoneKey,
            new ZoneRequirement(new OwnershipCondition<Zone>(OwnershipType.Friendly))
        ));
    }

    public override async UniTask<bool> ExecuteAsync(CancellationToken cancellationToken) {
        if (!TryGetTarget(SpawnZoneKey, out Zone zone)) {
            Debug.LogError($"Valid {SpawnZoneKey} not found");
            return false;
        }


        await UniTask.Yield(); // For async simulation
        zone.AddCreature(_creatureCard);
        return true;
    }
}