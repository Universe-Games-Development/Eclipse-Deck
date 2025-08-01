using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

public class Zone : GameUnit {
}

internal class SpawnCreatureOperation : GameOperation {
    private CreatureCardData creatureCardData;

    public SpawnCreatureOperation(CreatureCardData creatureCardData) {
        this.creatureCardData = creatureCardData;
        namedTargets.Add(new NamedTarget("spawnZone", new ZoneRequirement()));
    }
}
