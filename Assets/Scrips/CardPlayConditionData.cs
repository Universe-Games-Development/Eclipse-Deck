using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Zenject;

public interface IInputRequirementRegistry {
    CardInputRequirement<T> GetRequirement<T>(Type type) where T : Component;
}

public class InputRequirementRegistry : IInputRequirementRegistry {
    private readonly Dictionary<Type, object> _requirements = new();
    private readonly IInstantiator _instantiator; // Залежність від DI-контейнера (наприклад, Zenject)

    public InputRequirementRegistry(IInstantiator instantiator) {
        _instantiator = instantiator;
        RegisterAllRequirements();
    }

    private void RegisterAllRequirements() {
        var requirementType = typeof(CardInputRequirement<>);
        var derivedTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && IsSubclassOfRawGeneric(requirementType, t))
            .ToList();

        foreach (var type in derivedTypes) {
            var instance = _instantiator.Instantiate(type);
            _requirements[type] = instance;
        }
    }

    public CardInputRequirement<T> GetRequirement<T>(Type type) where T : Component {
        // Пошук найбільш конкретного типу в ієрархії
        var targetType = type.GetAllBaseTypesAndInterfaces()
            .FirstOrDefault(t => _requirements.ContainsKey(t));

        if (targetType != null && _requirements.TryGetValue(targetType, out var requirement)) {
            return (CardInputRequirement<T>)requirement;
        }

        throw new KeyNotFoundException($"Requirement for type {type} not found");
    }

    private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck) {
        while (toCheck != null && toCheck != typeof(object)) {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur) return true;
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}

public static class TypeExtensions {
    public static IEnumerable<Type> GetAllBaseTypesAndInterfaces(this Type type) {
        var types = new List<Type>();

        for (var current = type; current != null; current = current.BaseType) {
            types.Add(current);
            types.AddRange(current.GetInterfaces());
        }

        return types.Distinct();
    }
}

public class AnyCardInputRequirement : CardInputRequirement<CardUI> {
    public override string Instruction => "Choose any card";

    public override string WrongTypeMessage => "You must select a card";

    protected override bool ValidateCondition(Opponent cardPlayer, CardUI cardView, out string callBackMessage) {
        callBackMessage = $"Card {cardView} selected";
        return true;
    }
}

#region DamageableInputRequirements

public class GenericDamageableRequirement : CardInputRequirement<MonoBehaviour> {
    public override string Instruction => "Choose a valid damageable target";
    public override string WrongTypeMessage => "You must select a valid damageable object";

    protected override bool ValidateCondition(Opponent cardPlayer, MonoBehaviour input, out string callBackMessage) {
        var logicHolder = input as ILogicHolder<object>; // Отримуємо ILogicHolder
        if (logicHolder == null) {
            callBackMessage = "Selected object does not contain logic";
            return false;
        }

        if (logicHolder.Logic is not IHealthEntity health) {
            callBackMessage = "Selected object is not damageable";
            return false;
        }

        callBackMessage = $"Damageable object {health} selected";
        return true;
    }
}

#endregion

public class OpponentInputRequirement : CardInputRequirement<OpponentController> {
    public override string Instruction => "Choose any opponent";

    public override string WrongTypeMessage => "Select opponent";

    protected override bool ValidateCondition(Opponent cardPlayer, OpponentController targetOpponent, out string callBackMessage) {
        callBackMessage = $"Opponent {targetOpponent.Logic.Name} selected";
        return true;
    }
}

#region CreatureInputRequirements
public class CreatureInputRequirement : CardInputRequirement<CreatureController> {
    public override string Instruction => "Choose any creature";

    public override string WrongTypeMessage => "You must select a creature";

    protected override bool ValidateCondition(Opponent cardPlayer, CreatureController creatureView, out string callBackMessage) {
        callBackMessage = $"Creature {creatureView} selected";
        return true;
    }

    protected bool IsFriendly(Opponent cardPlayer, Creature creature) {
        return creature.CurrentField.Owner == cardPlayer;
    }
}

public class FriendlyCreatureRequirement : CreatureInputRequirement {
    public override string Instruction => "Choose a friendly creature";
    public string WrongCreatureMessage => "You must select a friendly creature";
    protected override bool ValidateCondition(Opponent cardPlayer, CreatureController creatureView, out string callBackMessage) {
        if (!IsFriendly(cardPlayer, creatureView.Logic)) {
            callBackMessage = callBackMessage = $"{WrongCreatureMessage}: Enemy {creatureView} does not belong to {cardPlayer.Name}"; 
            return false;
        }
        callBackMessage = $"Friendly creature {creatureView} selected";
        return true;
    }
}

public class EnemyCreatureRequirement : CreatureInputRequirement {
    public override string Instruction => "Choose an enemy creature";
    public string WrongCreatureMessage => "You must select an enemy creature";
    protected override bool ValidateCondition(Opponent cardPlayer, CreatureController creatureView, out string callBackMessage) {
        if (IsFriendly(cardPlayer, creatureView.Logic)) {
            callBackMessage = WrongCreatureMessage + $"Friendly {creatureView} is belong to {cardPlayer.Name}";
            return false;
        }
        callBackMessage = $"Enemy creature {creatureView} selected";
        return true;
    }
}
#endregion

public class FriendlyFieldInputRequirement : FieldInputRequirement {
    public override string Instruction => "Choose friendly field";
    protected override bool ValidateCondition(Opponent cardPlayer, FieldController fieldView, out string callBackMessage) {
        if (!IsFriendly(cardPlayer, fieldView.Logic)) {
            callBackMessage = $"Field {fieldView} does not belong to {cardPlayer.Name}";
            return false;
        }
        callBackMessage = $"Field {fieldView} selected";
        return true;
    }

    protected bool IsFriendly(Opponent cardPlayer, Field field) {
        return field.Owner == cardPlayer;
    }
}


public class FieldInputRequirement : CardInputRequirement<FieldController> {
    public override string Instruction => "Choose any field";

    public override string WrongTypeMessage => "You must select a field";

    protected override bool ValidateCondition(Opponent cardPlayer, FieldController field, out string callBackMessage) {
        callBackMessage = $"Field {field} selected";
        return true;
    }

    protected bool IsFriendly(Opponent cardPlayer, Field field) {
        return field.Owner == cardPlayer;
    }
}

public abstract class CardInputRequirement<T> where T : Component {
    public abstract string Instruction { get; }
    public abstract string WrongTypeMessage { get; }

    public bool ValidateInput(Opponent cardPlayer, T input, out string callBackMessage) {
        if (input == null) {
            callBackMessage = "No input selected";
            return false;
        }

        input.GetComponent<T>();

        if (input is not T typedInput) {
            callBackMessage = WrongTypeMessage;
            return false;
        }

        return ValidateCondition(cardPlayer, typedInput, out callBackMessage);
    }

    protected abstract bool ValidateCondition(Opponent cardPlayer, T input, out string callBackMessage);
}
