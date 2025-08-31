using System;
using System.Collections.Generic;
using UnityEngine;


public class Zone : UnitModel {
    public Action OnCreatureSpawned;

    List<CreatureCard> creatures = new();
    public void SpawnCreture(CreatureCard creatureCard) {
        Debug.Log($"Spawning craeture at zone: {this}");

        creatures.Add(creatureCard);
       
        OnCreatureSpawned?.Invoke();
    }

    public int GetCreaturesCount() {
        return creatures.Count;
    }
}
