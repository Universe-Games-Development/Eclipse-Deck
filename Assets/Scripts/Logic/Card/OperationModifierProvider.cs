using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public interface IOperationModifier {
    void Attach(GameOperation operation);
    void Deatach();
}

public interface IModifierProvider {
    List<IOperationModifier> GetAll();
}

public class OperationModifierProvider : IModifierProvider {
    private List<IOperationModifier> permanents = new();
    private List<TimedModifier<IOperationModifier>> temporary = new();
    private GameEventBus _gameEventBus;

    public OperationModifierProvider(GameEventBus gameEventBus) {
        _gameEventBus = gameEventBus;
        _gameEventBus.SubscribeTo<TurnEndEvent>(OnTurnEnd);
    }

    private void OnTurnEnd(ref TurnEndEvent eventData) {
        foreach (var modifier in temporary) {
            modifier.DecreaseTurns();
        }
        temporary.RemoveAll(m => m.IsExpired);
    }

    public List<IOperationModifier> GetAll() {
        var active = temporary.Where(m => !m.IsExpired).Cast<IOperationModifier>();
        return permanents.Concat(active).ToList();
    }

    public void Add(IOperationModifier modifier, int duration = 0) {
        if (duration > 0) {
            temporary.Add(new TimedModifier<IOperationModifier>(modifier, duration));
        } else {
            permanents.Add(modifier);
        }
    }

    public void AddTemporary(IOperationModifier modifier, int turns) {
        temporary.Add(new TimedModifier<IOperationModifier>(modifier, turns));
    }
}

public abstract class BaseEffect {
    public string Name { get; }

    protected BaseEffect(string name = default) {
        Name = name;
    }

    public abstract bool Equals(BaseEffect other);

    public override bool Equals(object obj) {
        return Equals(obj as BaseEffect);
    }

    public abstract override int GetHashCode();
    public abstract override string ToString();
}


public class EffectManager {
    private readonly List<BaseEffect> permanents = new();
    private readonly List<TimedModifier<BaseEffect>> temporary = new();

    public event Action OnCleared;
    public event Action<BaseEffect> OnEffectAdded;
    public event Action<BaseEffect> OnEffectRemoved;
    public EffectManager(TurnManager turnManager) {
        turnManager.OnTurnEnd += OnTurnEnd;
    }

    private void OnTurnEnd(TurnEndEvent @event) {
        List<TimedModifier<BaseEffect>> timedEffects = temporary.ToList();
        foreach (var timedEffect in timedEffects) {
            timedEffect.DecreaseTurns();
            if (timedEffect.IsExpired) {
                Remove(timedEffect.Modifier);
            }
        }
    }

    public void Add<T>(T effect, int duration = 0) where T : BaseEffect {
        if (duration > 0) {
            var timedEffect = new TimedModifier<BaseEffect>(effect, duration);
            temporary.Add(timedEffect);
        } else {
            permanents.Add(effect);
        }

        OnEffectAdded?.Invoke(effect);
    }

    public void Remove(BaseEffect effect) {
        if (permanents.Contains(effect)) {
            permanents.Remove(effect);
            OnEffectRemoved?.Invoke(effect);
        }

        var toRemove = temporary.FirstOrDefault(m => m.Modifier == effect);
        if (toRemove != null) {
            temporary.Remove(toRemove);
            OnEffectRemoved?.Invoke(effect);
        }
    }

    public void ClearAll() {
        // Створюємо копії для уникнення помилки модифікації колекції
        var permCopy = permanents.ToList();
        var tempCopy = temporary.ToList();

        foreach (var effect in permCopy) {
            Remove(effect);
        }

        foreach (var effect in tempCopy) {
            Remove(effect.Modifier);
        }

        OnCleared?.Invoke();
    }

    public IEnumerable<BaseEffect> GetAll() =>
        permanents.Concat(temporary.Where(m => !m.IsExpired).Select(m => m.Modifier));

    public IEnumerable<T> GetEffectsOfType<T>() where T : BaseEffect =>
        GetAll().OfType<T>();
}


public class StatEffect : BaseEffect {
    public int AttackModifier { get; }
    public int HealthModifier { get; }

    public StatEffect(int attackModifier, int healthModifier, string name = "") : base(name) {
        AttackModifier = attackModifier;
        HealthModifier = healthModifier;
    }

    public bool Equals(StatEffect other) {
        if (other == null) return false;
        return AttackModifier == other.AttackModifier &&
               HealthModifier == other.HealthModifier &&
               Name == other.Name;
    }

    public override bool Equals(BaseEffect other) {
        return Equals(other as StatEffect);
    }

    public override int GetHashCode() {
        return HashCode.Combine(AttackModifier, HealthModifier, Name);
    }

    public override string ToString() {
        string attack = AttackModifier > 0 ? $"+{AttackModifier}" : AttackModifier.ToString();
        string health = HealthModifier > 0 ? $"+{HealthModifier}" : HealthModifier.ToString();
        return $"{Name} ({attack}, {health})";
    }
}


public class TimedModifier<T> where T : class {
    public T Modifier { get; }
    public int TurnsLeft { get; private set; }
    public bool IsExpired => TurnsLeft <= 0;
    public TimedModifier(T modifier, int turns) {
        Modifier = modifier;
        TurnsLeft = turns;
    }
    public void DecreaseTurns() {
        TurnsLeft--;
    }
}

