using UnityEngine;

// used for runtime creation of operation

public class SpawnCreatureOperationData : OperationData {
    public CreatureCard creatureCard;
    public Vector3 spawnPosition;
    internal CardPresenter presenter;
}

[OperationFor(typeof(SpawnCreatureOperationData))]
public class SpawnCreatureOperation : GameOperation {
    private const string SpawnZoneKey = "spawnZone";
    private readonly SpawnCreatureOperationData _data;
    private readonly ICreatureSpawnService _spawnService;
    private readonly Vector3 _spawnPosition;

    public SpawnCreatureOperation(SpawnCreatureOperationData data, ICreatureSpawnService spawnService) {
        _data = data;
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
        CreaturePresenter creaturePresenter = _spawnService.SpawnCreatureFromCard(_data.creatureCard);
        return zone.PlaceCreature(creaturePresenter, _spawnPosition);
    }
}

