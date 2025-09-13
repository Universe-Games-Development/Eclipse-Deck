using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zenject;

// used for runtime creation of operation
public class SpawnCreatureOperationData : OperationData {
    public CreatureCard creatureCard;
    public Vector3 spawnPosition;

    public override GameOperation CreateOperation() {
        return null;
    }
}

public class SpawnCreatureOperation : GameOperation {
    private const string SpawnZoneKey = "spawnZone";
    private readonly CreatureCard _creatureCard;
    private readonly ICreatureSpawnService _spawnService;
    private readonly Vector3 _spawnPosition;

    public SpawnCreatureOperation(CreatureCard creatureCard, ICreatureSpawnService spawnService) {
        _creatureCard = creatureCard;
        _spawnService = spawnService;

        ZoneRequirement allyZone = TargetRequirements.AllyZone;
        RequestTargets.Add(new Target(SpawnZoneKey, allyZone));
    }

    public override bool Execute() {
        if (!TryGetTarget(SpawnZoneKey, out ZonePresenter zone)) {
            Debug.LogError($"Valid {SpawnZoneKey} not found");
            return false;
        }

        // Делегуємо створення сервісу
        CreaturePresenter creaturePresenter = _spawnService.SpawnCreatureFromCard(_creatureCard);
        return zone.PlaceCreature(creaturePresenter, _spawnPosition);
    }
}

