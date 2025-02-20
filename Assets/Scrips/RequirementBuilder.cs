using System.Collections.Generic;
using System.Linq;

public interface IRequirement<in T> {
    string GetInstruction();
    bool IsMet(T entity, out string errorMessage);
}

public class RequirementBuilder<T> {
    private IRequirement<T> _current;

    public RequirementBuilder<T> Add(IRequirement<T> requirement) {
        if (_current == null) {
            _current = requirement;
        } else {
            _current = new AndRequirement<T>(_current, requirement);
        }
        return this;
    }

    public RequirementBuilder<T> And(IRequirement<T> requirement) => Add(requirement);

    public RequirementBuilder<T> Or(IRequirement<T> requirement) {
        _current = new OrRequirement<T>(_current, requirement);
        return this;
    }

    public RequirementBuilder<T> Not() {
        _current = new NotRequirement<T>(_current);
        return this;
    }

    public IRequirement<T> Build() => _current;
}

public abstract class CompositeRequirement<T> : IRequirement<T> {
    protected readonly IRequirement<T>[] Requirements;

    protected CompositeRequirement(params IRequirement<T>[] requirements) {
        Requirements = requirements;
    }

    public abstract bool IsMet(T entity, out string errorMessage);
    public abstract string GetInstruction();
}

public class AndRequirement<T> : CompositeRequirement<T> {
    public AndRequirement(params IRequirement<T>[] requirements) : base(requirements) { }

    public override bool IsMet(T entity, out string errorMessage) {
        foreach (var requirement in Requirements) {
            if (!requirement.IsMet(entity, out errorMessage)) {
                return false;
            }
        }
        errorMessage = string.Empty;
        return true;
    }

    public override string GetInstruction() {
        return string.Join(" AND ", Requirements.Select(r => r.GetInstruction()));
    }
}

public class OrRequirement<T> : CompositeRequirement<T> {
    public OrRequirement(params IRequirement<T>[] requirements) : base(requirements) { }

    public override bool IsMet(T entity, out string errorMessage) {
        var errors = new List<string>();
        foreach (var requirement in Requirements) {
            if (requirement.IsMet(entity, out _)) {
                errorMessage = string.Empty;
                return true;
            }
            requirement.IsMet(entity, out var msg);
            errors.Add(msg);
        }
        errorMessage = $"None of the conditions met: {string.Join(" OR ", errors)}";
        return false;
    }

    public override string GetInstruction() =>
        string.Join(" OR ", Requirements.Select(r => r.GetInstruction()));

}

public class NotRequirement<T> : IRequirement<T> {
    private readonly IRequirement<T> _requirement;

    public NotRequirement(IRequirement<T> requirement) {
        _requirement = requirement;
    }

    public bool IsMet(T entity, out string errorMessage) {
        var result = !_requirement.IsMet(entity, out var innerMessage);
        errorMessage = result ? string.Empty : $"NOT condition failed: {innerMessage}";
        return result;
    }

    public string GetInstruction() => $"NOT ({_requirement.GetInstruction()})";
}

public class EnemyCreatureRequirement : IRequirement<Creature> {
    private Opponent requestingPlayer;

    public EnemyCreatureRequirement(Opponent requestingPlayer) {
        this.requestingPlayer = requestingPlayer;
    }

    public string GetInstruction() => "Choose enemy creature";

    public bool IsMet(Creature creature, out string errorMessage) {
        if (creature == null) {
            errorMessage = "No creature selected";
            return false;
        }

        if (creature.CurrentField.Owner == requestingPlayer) {
            errorMessage = "Cannot target your own creature";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}

public class CreaturePowerRequirement : IRequirement<Creature> {
    private readonly int _requiredPower;
    private readonly bool _isLessThan;

    public CreaturePowerRequirement(int power, bool isLessThan = false) {
        _requiredPower = power;
        _isLessThan = isLessThan;
    }

    public string GetInstruction() =>
        $"Creature with power {(_isLessThan ? "<" : "≥")} {_requiredPower}";

    public bool IsMet(Creature creature, out string errorMessage) {
        if (creature == null) {
            errorMessage = "No creature selected";
            return false;
        }

        int attackValue = creature.GetAttack().CurrentValue;
        var result = _isLessThan ?
            attackValue < _requiredPower :
            attackValue >= _requiredPower;

        errorMessage = result ?
            string.Empty :
            $"Creature power {attackValue} does not meet requirement";

        return result;
    }
}

public class OwnerFieldRequirement : IRequirement<Field> {
    private Opponent requestingPlayer;

    public OwnerFieldRequirement(Opponent requestingPlayer) {
        this.requestingPlayer = requestingPlayer;
    }

    public bool IsMet(Field field, out string errorMessage) {
        var result = field?.Owner == requestingPlayer;
        errorMessage = result ?
            string.Empty :
            "This field does not belong to you";
        return result;
    }

    public string GetInstruction() => "Field must belong to you";
}

public class CardStateRequirement : IRequirement<Card> {
    private readonly CardState state;
    private readonly List<CardState> states;
    private bool isSingle = false;

    public CardStateRequirement(CardState state) {
        this.state = state;
        isSingle = true;
    }

    public CardStateRequirement(List<CardState> states) {
        this.states = states;
        isSingle = false;
    }

    public bool IsMet(Card card, out string errorMessage) {
        errorMessage = string.Empty;
        return isSingle ? card.CurrentState == state : states.Contains(card.CurrentState);
    }

    public string GetInstruction() {
        return isSingle ? $"Choose card {state}" : $"Any card that {string.Join(", ", states)}";
    }
}

public class CreatureAliveRequirement : IRequirement<Creature> {
    public string Instruction = "Creature must be alive";

    public string GetInstruction() => Instruction;

    public bool IsMet(Creature creature, out string errorMessage) {
        errorMessage = string.Empty;
        return creature != null && creature.GetHealth().IsAlive();
    }
}