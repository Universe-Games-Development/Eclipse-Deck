using UnityEngine;

public class SpawnCreatureOperation : GameOperation {
    private const string SpawnZoneKey = "spawnZone";
    private readonly CreatureCard _creatureCard;

    public SpawnCreatureOperation(CreatureCard creatureCard) {
        _creatureCard = creatureCard;

        ZoneRequirement allyZone = TargetRequirements.AllyZone;
        RequestTargets.Add(new Target(
            SpawnZoneKey,
            allyZone
        ));
    }

    public override bool Execute() {
        if (!TryGetTarget(SpawnZoneKey, out ZonePresenter zone)) {
            Debug.LogError($"Valid {SpawnZoneKey} not found");
            return false;
        }

        zone.SpawnCreture(_creatureCard);
        return true;
    }
}