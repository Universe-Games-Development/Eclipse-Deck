using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Добавим дополнительные методы доступа
public interface IGameDataProvider {
    CreatureSpawner CreatureSpawner { get; }
    CardsHandleSystem CardsHandleSystem { get; }
    BoardSeatSystem BoardSeatSystem { get; }
}

public class GameDataAggregator : IGameDataProvider {
    public BoardPresenter BoardPresenter { get; }

    public CreatureSpawner CreatureSpawner { get; }
    public CardsHandleSystem CardsHandleSystem { get; }
    public BoardSeatSystem BoardSeatSystem { get; }

    public GameDataAggregator(BoardPresenter boardPresenter, BoardSeatSystem seats, CreatureSpawner creatureSpawner, CardsHandleSystem cardsPlaySystem) {
        BoardPresenter = boardPresenter;
        BoardSeatSystem = seats;
        CreatureSpawner = creatureSpawner;
        CardsHandleSystem = cardsPlaySystem;
    }
}

public class RequirementFactory {
    private readonly IGameDataProvider _data;

    public RequirementFactory(IGameDataProvider data) {
        _data = data;
    }

    public FieldRequirement CreateFieldRequirement(params Condition<Field>[] conditions) {
        return new FieldRequirement(_data, conditions);
    }

    public CreatureRequirement CreateCreatureRequirement(params Condition<Creature>[] conditions) {
        return new CreatureRequirement(_data, conditions);
    }
}


public interface IRequirement {
    bool IsCasterFill { get; } // Whether the caster or opponent fills this
    bool IsForcedChoice { get; } // Whether the choice can be canceled or not
    ValidationResult Check(object selected);
    string GetInstruction();
}

public abstract class SelectionRequirement<T> : IRequirement where T : class {
    public IEnumerable<Condition<T>> Conditions { get; private set; }

    public bool IsCasterFill { get; set; }

    public bool IsForcedChoice { get; set; }

    protected readonly IGameDataProvider _data;

    protected SelectionRequirement(IGameDataProvider data, params Condition<T>[] conditions) {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
        IsCasterFill = true;// За замовчуванням вважаємо, що це кастер
        IsForcedChoice = false; // За замовчуванням не примусова
    }

    public ValidationResult Check(object selected) {
        if (!TryConvertToRequired(selected, out T defined)) {
            Debug.Log($"Wrong type selected: {selected}");
            return ValidationResult.Fail();
        }
        return ValidateConditions(defined);
    }

    private ValidationResult ValidateConditions(T defined) {
        if (!Conditions.Any()) {
            Debug.LogWarning("No conditions defined. Defaulting to valid.");
            return ValidationResult.Success;
        }

        foreach (var condition in Conditions) {
            var result = condition.Validate(_data, defined);
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

public class FieldRequirement : SelectionRequirement<Field> {
    public FieldRequirement(IGameDataProvider data, params Condition<Field>[] conditions) : base(data, conditions) {
    }

}

public class CreatureRequirement : SelectionRequirement<Creature> {
    public CreatureRequirement(IGameDataProvider data, params Condition<Creature>[] conditions) : base(data, conditions) {
    }
}

public abstract class GameOperation {
    protected readonly IGameDataProvider _data;

    // Словник поле (як PropertyInfo або інший ідентифікатор) -> вимога
    public Dictionary<string, IRequirement> Requirements { get; } = new Dictionary<string, IRequirement>();

    // Словник для зберігання результатів заповнення
    public Dictionary<string, object> Results { get; } = new Dictionary<string, object>();

    public GameOperation(IGameDataProvider data) {
        _data = data;
    }

    public abstract void PerformOperation();

    public async UniTask<bool> FillRequirements(Opponent requestingPlayer) {
        foreach (var entry in Requirements) {
            string fieldName = entry.Key;
            IRequirement requirement = entry.Value;

            IActionFiller filler = GetInputFiller(requirement, requestingPlayer);
            object result = await filler.ProcessRequirementAsync(requirement);

            if (result == null) {
                return false;
            }

            Results[fieldName] = result;
        }
        return true;
    }

    internal bool IsPossible() {
        throw new NotImplementedException();
    }

    private IActionFiller GetInputFiller(IRequirement requirement, Opponent requestingPlayer) {
        Opponent inputPlayer = requirement.IsCasterFill ? requestingPlayer : _data.BoardSeatSystem.GetAgainstOpponent(requestingPlayer);
        return _data.BoardSeatSystem.GetActionFiller(inputPlayer);
    }

    // Допоміжний метод для отримання результату з правильним типом
    protected T GetResult<T>(string fieldName) where T : class {
        if (Results.TryGetValue(fieldName, out object value)) {
            return value as T;
        }
        return null;
    }
}

public class CreatureSummonOperation : GameOperation {
    private const string FIELD_KEY = "chosenField";
    private CreatureCard _creatureCard;

    public CreatureSummonOperation(CreatureCard creatureCard, RequirementFactory factory, IGameDataProvider data) : base(data) {
        _creatureCard = creatureCard;
        FieldRequirement fieldRequirement = factory.CreateFieldRequirement();
        fieldRequirement.AddCondition(new FieldEmptyCondition());
        fieldRequirement.AddCondition(new FieldOwnerSelectCondition());
        // Додаємо вимогу до словника
        Requirements[FIELD_KEY] = fieldRequirement;
    }

    public override void PerformOperation() {
        Field chosenField = GetResult<Field>(FIELD_KEY);
        CreatureSpawner creatureSpawner = _data.CreatureSpawner;
        creatureSpawner.SpawnCreature(_creatureCard, chosenField).Forget();
    }
}

public struct ValidationResult {
    public bool IsValid;
    public string ErrorMessage;

    public static ValidationResult Success => new ValidationResult { IsValid = true };
    public static ValidationResult Fail(string message = default) => new ValidationResult { IsValid = false, ErrorMessage = message };
}


public abstract class Condition<T> where T : class {
    public ValidationResult Validate(IGameDataProvider data, T model) {
        if (model == null)
            return ValidationResult.Fail("Wrong item selected");

        return CheckCondition(data, model);
    }

    protected abstract ValidationResult CheckCondition(IGameDataProvider data, T model);
}

public class FieldEmptyCondition : Condition<Field> {

    protected override ValidationResult CheckCondition(IGameDataProvider data, Field target) {
        return target.Creature == null ? ValidationResult.Success : ValidationResult.Fail("Field is not empty");
    }
}

public class FieldOwnerSelectCondition : Condition<Field> {
    private const string WRONG_OWNER = "This field doesn't belong to you";
    protected override ValidationResult CheckCondition(IGameDataProvider data, Field target) {
        CardsHandleSystem cardsPlaySystem = data.CardsHandleSystem;
        return target.Owner == cardsPlaySystem.CurrentPlayer ? ValidationResult.Success : ValidationResult.Fail(WRONG_OWNER); ;
    }
}

public class AliveCreatureCOndition : Condition<Creature> {
    public string Instruction = "Creature must be alive";
    public const string NOT_ALIVE = "Not alive";

    protected override ValidationResult CheckCondition(IGameDataProvider data, Creature creature) {
        return creature.Health.IsAlive() ? ValidationResult.Success : ValidationResult.Fail(NOT_ALIVE); ; ;
    }
}