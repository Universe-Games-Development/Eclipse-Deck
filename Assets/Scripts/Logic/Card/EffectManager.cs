using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using UnityEngine;


public class EffectManager {
    private readonly List<BaseEffect> permanents = new();
    private readonly List<TimedEffectWrapper<BaseEffect>> temporary = new();

    public event Action OnCleared;
    public event Action<BaseEffect> OnEffectAdded;
    public event Action<BaseEffect> OnEffectRemoved;
    public EffectManager(TurnManager turnManager) {
        turnManager.OnTurnEnd += OnTurnEnd;
    }

    private void OnTurnEnd(TurnEndEvent @event) {
        List<TimedEffectWrapper<BaseEffect>> timedEffects = temporary.ToList();
        foreach (var timedEffect in timedEffects) {
            timedEffect.DecreaseTurns();
            if (timedEffect.IsExpired) {
                Remove(timedEffect.Modifier);
            }
        }
    }

    public void Add<T>(T effect, int duration = 0) where T : BaseEffect {
        if (duration > 0) {
            var timedEffect = new TimedEffectWrapper<BaseEffect>(effect, duration);
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

public class TimedEffectWrapper<T> where T : class {
    public T Modifier { get; }
    public int TurnsLeft { get; private set; }
    public bool IsExpired => TurnsLeft <= 0;
    public TimedEffectWrapper(T modifier, int turns) {
        Modifier = modifier;
        TurnsLeft = turns;
    }
    public void DecreaseTurns() {
        TurnsLeft--;
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
public class OperationModifier : BaseEffect {
    public virtual void Attach(GameOperation operation) { }
    public virtual void Deatach() { }

    public override bool Equals(BaseEffect other) {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode() {
        throw new System.NotImplementedException();
    }

    public override string ToString() {
        throw new System.NotImplementedException();
    }
}
public class Ability : BaseEffect {
    private TriggerManager _triggerManager = new();
    public List<GameOperation> Operations = new();
    public IGameUnit Owner { get; private set; }
    public Ability(List<AbilityTrigger> triggers, IGameUnit owner) {
        _triggerManager.OnTriggerActivation += Activate;
        triggers.ForEach(trigger => {
            _triggerManager.AddTrigger(trigger);
        });
        Owner = owner;
    }

    // Sonn become command 
    private void Activate(Opponent opponent, IGameUnit unit, IEvent @event) {
        ActivateAsync(opponent).Forget();
    }

    // Активувати здібність
    public async UniTask<bool> ActivateAsync(Opponent invoker) {
        bool atLeastOneOperationSucceeded = false;

        foreach (var operation in Operations) {
            await operation.FillRequirements(invoker);

            // Якщо всі необхідні цілі заповнені, виконуємо операцію
            if (operation.AreTargetsFilled()) {
                List<OperationModifier> operationModifiers = Owner.EffectManager.GetEffectsOfType<OperationModifier>().ToList();

                foreach (var modifier in operationModifiers) {
                    modifier.Attach(operation);
                }

                operation.PerformOperation();

                foreach (var modifier in operationModifiers) {
                    modifier.Deatach();
                }
                atLeastOneOperationSucceeded = true;
            }
        }

        return atLeastOneOperationSucceeded;
    }

    public override bool Equals(BaseEffect other) {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode() {
        throw new System.NotImplementedException();
    }

    public override string ToString() {
        throw new System.NotImplementedException();
    }
}

public abstract class GameOperation {
    [Inject] protected readonly IGameContext _data;

    // Словник поле (як PropertyInfo або інший ідентифікатор) -> вимога
    public Dictionary<string, IRequirement> Requirements { get; } = new Dictionary<string, IRequirement>();

    // Словник для зберігання результатів заповнення
    public Dictionary<string, object> Results { get; } = new Dictionary<string, object>();

    public abstract void PerformOperation();

    public async UniTask<bool> FillRequirements(Opponent requestingPlayer) {
        foreach (var entry in Requirements) {
            string fieldName = entry.Key;
            IRequirement requirement = entry.Value;

            ITargetingService filler = PickTargetingService(requirement.IsCasterFill ? requestingPlayer : _data.BoardSeatSystem.GetAgainstOpponent(requestingPlayer));
            object result = await filler.ProcessRequirementAsync(requestingPlayer, requirement);

            if (result == null) {
                return false;
            }
            SetTarget(fieldName, result);
        }
        return true;
    }

    public virtual void SetTarget(string key, object target) {
        Results[key] = target;
    }
    private ITargetingService PickTargetingService(Opponent inputPlayer) {
        return _data.BoardSeatSystem.GetActionFiller(inputPlayer);
    }

    // Перевірити, чи всі необхідні цілі заповнені
    public virtual bool AreTargetsFilled() {
        foreach (var req in Requirements) {
            if (!Results.ContainsKey(req.Key)) {
                return false;
            }
        }
        return true;
    }

    // Допоміжний метод для отримання результату з правильним типом
    protected T GetResult<T>(string fieldName) where T : class {
        if (Results.TryGetValue(fieldName, out object value)) {
            return value as T;
        }
        return null;
    }
}

// Операція нанесення шкоди
public class DealDamageOperation : GameOperation {
    private string TARGET_KEY = "target";
    private IDamageDealer _dealer;
    public DealDamageOperation(IDamageDealer dealer) {
        _dealer = dealer;

        // Вимога ворожої істоти як цілі
        CreatureRequirement creatureRequirement = new(new AnyCreatureCondition());
        Requirements.Add(TARGET_KEY, creatureRequirement);
    }

    public override void PerformOperation() {
        if (!AreTargetsFilled()) {
            Debug.LogError("Недостатньо цілей для операції");
            return;
        }

        var target = Results["target"] as IDamageable;
        if (target != null) {
            _dealer.Attack.DealDamage(target);
        }
    }
}

public class CreatureFightOperation : GameOperation {
    private string ATTAKER_KEY = "attacker";
    private string TARGET_KEY = "target";
    public CreatureFightOperation() {
        // Вимога дружньої істоти як атакуючого
        Requirements.Add(ATTAKER_KEY, new CreatureRequirement(new FriendlyCreatureCondition()));

        // Вимога ворожої істоти як цілі
        Requirements.Add(TARGET_KEY, new CreatureRequirement(new EnemyCreatureCondition()));
    }

    public override void PerformOperation() {
        if (!AreTargetsFilled()) {
            Debug.LogError("Недостатньо цілей для операції");
            return;
        }

        var attacker = Results["attacker"] as IDamageDealer;
        var target = Results[TARGET_KEY] as IDamageable;

        if (attacker != null && target != null) {
            // Використовуємо атаку істоти
            attacker.Attack.DealDamage(target);
        }
    }
}

