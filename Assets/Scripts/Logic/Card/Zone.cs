using System;
using System.Collections.Generic;
using UnityEngine;


public class Zone : UnitModel {
    public Action<Creature> OnCreaturePlaced;
    public Action<Creature> OnCreatureRemoved;

    List<Creature> creatures = new();

    public void PlaceCreature(Creature creature) {
        Debug.Log($"Spawning craeture at zone: {this}");

        creatures.Add(creature);
       
        OnCreaturePlaced?.Invoke(creature);
    }

    public void RemoveCreature(Creature creature) {
        Debug.Log($"Spawning craeture at zone: {this}");

        creatures.Remove(creature);

        OnCreatureRemoved?.Invoke(creature);
    }

    public int GetCreaturesCount() {
        return creatures.Count;
    }
}
