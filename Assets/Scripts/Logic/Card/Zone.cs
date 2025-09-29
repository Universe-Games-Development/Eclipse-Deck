using System;
using System.Collections.Generic;
using Zenject;


public class Zone : AreaModel, IDisposable {
    private const int MIN_CREATURES = 1;
    public int MaxCreatures { get; }
    public event Action<Creature> OnCreaturePlaced;
    public event Action<Creature> OnCreatureRemoved;

    private readonly List<Creature> _creatures = new();
    public IReadOnlyList<Creature> Creatures => _creatures.AsReadOnly();
    private readonly IEventBus<IEvent> eventBus;

    public Zone(IEventBus<IEvent> eventBus, int maxCreatures = 5) {
        
        this.eventBus = eventBus;
        MaxCreatures = Math.Max(MIN_CREATURES, maxCreatures);
        

        eventBus.SubscribeTo<DeathEvent>(OnDeath);
    }

    private void OnDeath(ref DeathEvent eventData) {
        if (eventData.DeadUnit is Creature creature) {
            if (_creatures.Contains(creature)) {
                RemoveCreature(creature);
            }
        }
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

    public bool IsFull() {
        return Creatures.Count == MaxCreatures;
    }

    public void Dispose() {
        eventBus.UnsubscribeFrom<DeathEvent>(OnDeath);
    }
}
