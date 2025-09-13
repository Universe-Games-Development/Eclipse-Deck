using System;
using System.Collections.Generic;
using UnityEngine;


public class Zone : UnitModel {
    public Action OnCreatureSpawned;

    List<Creature> creatures = new();
    public void PlaceCreature(Creature creature) {
        Debug.Log($"Spawning craeture at zone: {this}");

        creatures.Add(creature);
       
        OnCreatureSpawned?.Invoke();
    }

    public int GetCreaturesCount() {
        return creatures.Count;
    }
}
