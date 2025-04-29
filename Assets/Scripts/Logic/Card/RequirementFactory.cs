using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

// Добавим дополнительные методы доступа
public interface IGameContext {
    BoardGame BoardGame { get; }
    CreatureSpawner CreatureSpawner { get; }
    CardsHandleSystem CardsHandleSystem { get; }
}

public class GameDataAggregator : IGameContext {
    public BoardSystem BoardPresenter { get; }
    public BoardGame BoardGame { get; }
    public CreatureSpawner CreatureSpawner { get; }
    public CardsHandleSystem CardsHandleSystem { get; }

    public GameDataAggregator(BoardSystem boardPresenter, CreatureSpawner creatureSpawner, CardsHandleSystem cardsPlaySystem) {
        BoardPresenter = boardPresenter;
        CreatureSpawner = creatureSpawner;
        CardsHandleSystem = cardsPlaySystem;
    }
}

public class RequirementFactory {
    private readonly IGameContext _data;

    public RequirementFactory(IGameContext data) {
        _data = data;
    }

    public FieldRequirement CreateFieldRequirement(params Condition<Field>[] conditions) {
        return new FieldRequirement(conditions);
    }

    public CreatureRequirement CreateCreatureRequirement(params Condition<Creature>[] conditions) {
        return new CreatureRequirement(conditions);
    }
}


public interface IRequirement {
    bool IsCasterFill { get; } // Whether the caster or opponent fills this
    bool IsForcedChoice { get; } // Whether the choice can be canceled or not
    ValidationResult Check(BoardPlayer initiator, object selected);
    string GetInstruction();
}

public abstract class Requirement<T> : IRequirement where T : class {
    public IEnumerable<Condition<T>> Conditions { get; private set; }

    public bool IsCasterFill { get; set; }

    public bool IsForcedChoice { get; set; }

    protected Requirement(params Condition<T>[] conditions) {
        Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
        IsCasterFill = true;// За замовчуванням вважаємо, що це кастер
        IsForcedChoice = false; // За замовчуванням не примусова
    }

    public ValidationResult Check(BoardPlayer initiator, object selected) {
        if (!TryConvertToRequired(selected, out T defined)) {
            Debug.Log($"Wrong type selected: {selected}");
            return ValidationResult.Fail();
        }
        foreach (var item in Conditions) {
            item.SetInitiator(initiator);
        }
        return ValidateConditions(defined);
    }

    private ValidationResult ValidateConditions(T defined) {
        if (!Conditions.Any()) {
            Debug.LogWarning("No conditions defined. Defaulting to valid.");
            return ValidationResult.Success;
        }

        foreach (var condition in Conditions) {
            var result = condition.Validate(defined);
            if (!result.IsValid) {
                return result;
            }
        }

        return ValidationResult.Success;
    }

    protected bool TryConvertToRequired(object something, out T defined) {
        if (something is T tryDefine) {
            defined = tryDefine;
            return true;
        }
        defined = null;
        return false;
    }

    public void AddCondition(Condition<T> condition) {
        if (condition == null) {
            throw new ArgumentNullException(nameof(condition));
        }
        // Додаємо нову умову до списку
        Conditions = Conditions.Append(condition);
    }
    public string GetInstruction() {
        throw new NotImplementedException();
    }
}

public class FieldRequirement : Requirement<Field> {
    public FieldRequirement(params Condition<Field>[] conditions) : base(conditions) {
    }

}

public class CreatureRequirement : Requirement<Creature> {
    public CreatureRequirement(params Condition<Creature>[] conditions) : base(conditions) {
    }
}



public struct ValidationResult {
    public bool IsValid;
    public string ErrorMessage;

    public static ValidationResult Success => new ValidationResult { IsValid = true };
    public static ValidationResult Fail(string message = default) => new ValidationResult { IsValid = false, ErrorMessage = message };
}

public abstract class Condition<T> where T : class {
    protected BoardPlayer Initiator;
    [Inject] private IGameContext _gameData;

    public void SetInitiator(BoardPlayer opponent) {
        Initiator = opponent;
    }

    public ValidationResult Validate(T model) {
        if (model == null)
            return ValidationResult.Fail("Wrong item selected");

        return CheckCondition(model);
    }

    protected abstract ValidationResult CheckCondition(T model);
}

public class FieldEmptyCondition : Condition<Field> {

    protected override ValidationResult CheckCondition(Field target) {
        return target.OccupyingCreature == null ? ValidationResult.Success : ValidationResult.Fail("Field is not empty");
    }
}

public class FieldOwnerSelectCondition : Condition<Field> {
    private const string WRONG_OWNER = "This field doesn't belong to you";
    protected override ValidationResult CheckCondition(Field target) {
        return target.Owner == Initiator ? ValidationResult.Success : ValidationResult.Fail(WRONG_OWNER); ;
    }
}

public class AliveCreatureCOndition : Condition<Creature> {
    public string Instruction = "Creature must be alive";
    public const string NOT_ALIVE = "Not alive";

    protected override ValidationResult CheckCondition(Creature creature) {
        return creature.Health.IsDead ? ValidationResult.Success : ValidationResult.Fail(NOT_ALIVE); ; ;
    }
}

public class EnemyCreatureCondition : Condition<Creature> {
    protected override ValidationResult CheckCondition(Creature creature) {
        return creature.ControlledBy != Initiator
            ? ValidationResult.Success
            : ValidationResult.Fail("Not enemy creature");
    }
}

public class FriendlyCreatureCondition: Condition<Creature> {
    protected override ValidationResult CheckCondition(Creature creature) {
        return creature.ControlledBy == Initiator
            ? ValidationResult.Success
            : ValidationResult.Fail("Not friendly creature");
    }
}

public class AnyCreatureCondition : Condition<Creature> {
    protected override ValidationResult CheckCondition(Creature creature) {
        return ValidationResult.Success;
    }
}
