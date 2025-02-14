using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class CardInputRequirements {
    private static readonly Dictionary<Type, object> _requirements = new();

    static CardInputRequirements() {
        RegisterAllRequirements();
    }

    private static void RegisterAllRequirements() {
        var requirementType = typeof(CardInputRequirement<>);

        var derivedTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => new { Type = t, Base = GetBaseGenericType(t, requirementType) })
            .Where(t => t.Base != null)
            .ToList();

        foreach (var typeInfo in derivedTypes) {
            var instance = Activator.CreateInstance(typeInfo.Type);
            if (instance == null) continue;

            _requirements[typeInfo.Base.GenericTypeArguments[0]] = instance;
        }
    }

    private static Type? GetBaseGenericType(Type type, Type genericBaseType) {
        while (type != null && type != typeof(object)) {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericBaseType) {
                return type;
            }
            type = type.BaseType!;
        }
        return null;
    }

    public static CardInputRequirement<T> GetByKey<T>(Type type) where T : class {
        if (_requirements.TryGetValue(type, out var requirement) && requirement is CardInputRequirement<T> typedRequirement) {
            return typedRequirement;
        }

        // Шукаємо найближчий відповідний базовий клас або інтерфейс
        var closestType = _requirements.Keys.FirstOrDefault(t => t.IsAssignableFrom(type));
        if (closestType != null && _requirements.TryGetValue(closestType, out var closestRequirement) && closestRequirement is CardInputRequirement<T> castedRequirement) {
            return castedRequirement;
        }

        throw new KeyNotFoundException($"No valid requirement registered for type {type}");
    }
}



public class AnyCardInputRequirement : CardInputRequirement<Card> {
    public override string Instruction => "Choose any card";

    public override string WrongTypeMessage => "You must select a card";

    protected override bool ValidateCondition(Opponent cardPlayer, Card card, out string callBackMessage) {
        callBackMessage = $"Card {card} selected";
        return true;
    }
}

#region DamageableInputRequirements

public class DamageableInputRequirement : CardInputRequirement<IHasHealth> {
    public override string Instruction => "Choose any target for damage";
    public override string WrongTypeMessage => "You must select a damagable object";
    protected override bool ValidateCondition(Opponent cardPlayer, IHasHealth damagable, out string callBackMessage) {
        callBackMessage = $"Damagable object {damagable} selected";
        return true;
    }
}

public class  EnemyDamagableRequirement : CardInputRequirement<IHasHealth> {
    public override string Instruction => "Choose enemy for damage";
    public override string WrongTypeMessage => "Need enemy damagable";
    protected override bool ValidateCondition(Opponent cardPlayer, IHasHealth damagable, out string callBackMessage) {
        callBackMessage = $"Damagable object {damagable} selected";
        return true;
    }
}

#endregion

public class OpponentInputRequirement : CardInputRequirement<Opponent> {
    public override string Instruction => "Choose any opponent";

    public override string WrongTypeMessage => "Select opponent";

    protected override bool ValidateCondition(Opponent cardPlayer, Opponent targetOpponent, out string callBackMessage) {
        callBackMessage = $"Opponent {targetOpponent.Name} selected";
        return true;
    }
}

#region CreatureInputRequirements
public class CreatureInputRequirement : CardInputRequirement<Creature> {
    public override string Instruction => "Choose any creature";

    public override string WrongTypeMessage => "You must select a creature";

    protected override bool ValidateCondition(Opponent cardPlayer, Creature creature, out string callBackMessage) {
        callBackMessage = $"Creature {creature} selected";
        return true;
    }

    protected bool IsFriendly(Opponent cardPlayer, Creature creature) {
        return creature.CurrentField.Owner == cardPlayer;
    }
}

public class FriendlyCreatureRequirement : CreatureInputRequirement {
    public override string Instruction => "Choose a friendly creature";
    public string WrongCreatureMessage => "You must select a friendly creature";
    protected override bool ValidateCondition(Opponent cardPlayer, Creature creature, out string callBackMessage) {
        if (!IsFriendly(cardPlayer, creature)) {
            callBackMessage = callBackMessage = $"{WrongCreatureMessage}: Enemy {creature} does not belong to {cardPlayer.Name}"; 
            return false;
        }
        callBackMessage = $"Friendly creature {creature} selected";
        return true;
    }
}

public class EnemyCreatureRequirement : CreatureInputRequirement {
    public override string Instruction => "Choose an enemy creature";
    public string WrongCreatureMessage => "You must select an enemy creature";
    protected override bool ValidateCondition(Opponent cardPlayer, Creature creature, out string callBackMessage) {
        if (IsFriendly(cardPlayer, creature)) {
            callBackMessage = WrongCreatureMessage + $"Friendly {creature} is belong to {cardPlayer.Name}";
            return false;
        }
        callBackMessage = $"Enemy creature {creature} selected";
        return true;
    }
}
#endregion

public class FriendlyFieldInputRequirement : FieldInputRequirement {

    protected override bool ValidateCondition(Opponent cardPlayer, Field field, out string callBackMessage) {
        if (!IsFriendly(cardPlayer, field)) {
            callBackMessage = $"Field {field} does not belong to {cardPlayer.Name}";
            return false;
        }
        callBackMessage = $"Field {field} selected";
        return true;
    }

    protected bool IsFriendly(Opponent cardPlayer, Field field) {
        return field.Owner == cardPlayer;
    }
}


public class FieldInputRequirement : CardInputRequirement<Field> {
    public override string Instruction => "Choose any field";

    public override string WrongTypeMessage => "You must select a field";

    protected override bool ValidateCondition(Opponent cardPlayer, Field field, out string callBackMessage) {
        callBackMessage = $"Field {field} selected";
        return true;
    }

    protected bool IsFriendly(Opponent cardPlayer, Field field) {
        return field.Owner == cardPlayer;
    }
}

public abstract class CardInputRequirement<T> {
    public abstract string Instruction { get; }
    public abstract string WrongTypeMessage { get; }

    public bool ValidateInput(Opponent cardPlayer, object input, out string callBackMessage) {
        if (input is not T typedInput) {
            callBackMessage = WrongTypeMessage;
            return false;
        }

        return ValidateCondition(cardPlayer, typedInput, out callBackMessage);
    }
    protected abstract bool ValidateCondition(Opponent cardPlayer, T input, out string callBackMessage);
}