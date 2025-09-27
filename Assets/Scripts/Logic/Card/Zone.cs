using System;
using System.Collections.Generic;


public class Zone : UnitModel {
    public int MaxCreatures { get; }
    public event Action<Creature> OnCreaturePlaced;
    public event Action<Creature> OnCreatureRemoved;

    private readonly List<Creature> _creatures = new();
    public IReadOnlyList<Creature> Creatures => _creatures.AsReadOnly();

    public Zone(int maxCreatures = 5) {
        MaxCreatures = maxCreatures;
        Id = $"Zone_{Guid.NewGuid()}";
    }

    public bool TryPlaceCreature(Creature creature) {
        if (_creatures.Count >= MaxCreatures) return false;

        _creatures.Add(creature);
        OnCreaturePlaced?.Invoke(creature);
        return true;
    }

    public bool RemoveCreature(Creature creature) {
        if (!_creatures.Remove(creature)) return false;

        OnCreatureRemoved?.Invoke(creature);
        return true;
    }
}
