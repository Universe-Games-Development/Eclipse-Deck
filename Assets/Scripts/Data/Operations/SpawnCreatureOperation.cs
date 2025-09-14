using UnityEngine;

// used for runtime creation of operation
public class SpawnCreatureOperationData : OperationData {
    public CreatureCard creatureCard;
}

[OperationFor(typeof(SpawnCreatureOperationData))]
public class SpawnCreatureOperation : GameOperation {
    private const string SpawnZoneKey = "spawnZone";
    private readonly SpawnCreatureOperationData _data;
    private readonly ICreatureFactory _spawnService;

    public SpawnCreatureOperation(SpawnCreatureOperationData data, ICreatureFactory spawnService) {
        _data = data;
        _spawnService = spawnService;

        ZoneRequirement allyZone = TargetRequirements.AllyZone;
        RequestTargets.Add(new Target(SpawnZoneKey, allyZone));
    }

    public override bool Execute() {
        if (!TryGetTarget(SpawnZoneKey, out Zone zone)) {
            Debug.LogError($"Valid {SpawnZoneKey} not found");
            return false;
        }
        Creature creature = _spawnService.SpawnCreature(_data.creatureCard);
        zone.PlaceCreature(creature);

        return true;
    }
}

