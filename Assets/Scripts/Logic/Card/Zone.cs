using System;
using System.Collections.Generic;
using System.Linq;

// Припущення: IEvent, DeathEvent, AreaModel і ValidationResult існують.
// Припущення: Creature має властивість, що дозволяє порівняти "силу".

public class Zone : AreaModel, IDisposable {
    private const int MIN_CREATURES = 1;
    public int MaxCreatures { get; private set; }

    // Events
    public event Action<Creature> OnCreatureSummoned;
    public event Action<Creature> OnCreatureRemoved;
    public event Action<int> ONMaxCreaturesChanged;

    private readonly List<Creature> _creatures = new();
    public IReadOnlyList<Creature> Creatures => _creatures.AsReadOnly();
    private readonly IEventBus<IEvent> eventBus;

    public Zone(IEventBus<IEvent> eventBus, int maxCreatures = 5) {
        this.eventBus = eventBus;
        MaxCreatures = Math.Max(MIN_CREATURES, maxCreatures);

        // Підписка на події
        eventBus.SubscribeTo<DeathEvent>(OnDeath);
    }

    private void OnDeath(ref DeathEvent eventData) {
        if (eventData.DeadUnit is Creature creature) {
            if (_creatures.Contains(creature)) {
                RemoveCreature(creature);
            }
        }
    }

    //---------------------------------------------------------
    // PUBLIC METHODS
    //---------------------------------------------------------
    /// <summary>
    /// Змінює максимальну кількість істот у зоні та видаляє надлишок, якщо новий розмір менший.
    /// </summary>
    public void ChangeSize(int newSize) {
        if (newSize < 0) return;

        int oldMaxCreatures = MaxCreatures;
        MaxCreatures = Math.Max(MIN_CREATURES, newSize);

        if (MaxCreatures < oldMaxCreatures) {
            RemoveExceesCreatures();
        }

        if (MaxCreatures != oldMaxCreatures) {
            ONMaxCreaturesChanged?.Invoke(MaxCreatures);
        }
    }

    /// <summary>
    /// Видаляє надлишкові істоти, якщо їх кількість перевищує MaxCreatures.
    /// </summary>
    private void RemoveExceesCreatures() {
        if (_creatures.Count <= MaxCreatures) return;

        int excessCount = _creatures.Count - MaxCreatures;

        // 1. Сортуємо істот за силою (або іншим критерієм)
        // Видаляємо найслабших (з найменшою Power)
        var creaturesToRemove = _creatures
            .OrderBy(c => c is IAttacker pc ? pc.CurrentAttack : 0) // Сортування за силою (слабкіші перші)
            .Take(excessCount)
            .ToList();

        // 2. Видаляємо обрані істоти
        foreach (var creature in creaturesToRemove) {
            RemoveCreature(creature);
            // Додаткова логіка: ініціювати видалення з гри, а не тільки з зони.
            // eventBus.Publish(new ZoneForcedRemovalEvent(creature));
        }
    }

    public bool TrySummonCreature(Creature creature) {
        if (_creatures.Count >= MaxCreatures) return false;

        _creatures.Add(creature);
        OnCreatureSummoned?.Invoke(creature);
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

    public ValidationResult Contains(Creature creature) {
        // Припускаю, що цей метод має повертати ValidationResult
        // Для простоти:
        return _creatures.Contains(creature) ? ValidationResult.Success : ValidationResult.Error("Nothing");
    }

    public void Dispose() {
        eventBus.UnsubscribeFrom<DeathEvent>(OnDeath);
    }

    public bool CanSummonCreature() {
        return !IsFull();
    }
}