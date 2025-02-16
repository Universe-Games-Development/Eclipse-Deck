using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Creature : IAbilitiesCaster, IDamageDealer, IHealthEntity {
    public Field CurrentField { get; private set; }
    public Action OnInterruptedMove;

    public Health Health;
    public Attack Attack;
    public AbilityManager AbilityManager;
    
    private CreatureCard creatureCard;
    private CreatureStrategyMovement movementHandler;
    private MoveCommand moveCommand;

    public Creature(CreatureCard creatureCard, Opponent owner, GameEventBus eventBus) {
        this.creatureCard = creatureCard;
        Health = new Health(this, creatureCard.HealthStat.MaxValue, creatureCard.HealthStat.CurrentValue, eventBus);
        Attack = new Attack(this, creatureCard.Attack.MaxValue, creatureCard.Attack.CurrentValue, eventBus);

        // Soon we define how to get the creatureSO
        CreatureSO creatuseSO = null;
        throw new NotFiniteNumberException();
        throw new NotFiniteNumberException();

        throw new NotFiniteNumberException();

        throw new NotFiniteNumberException();

        throw new NotFiniteNumberException();
        throw new NotFiniteNumberException();
        AbilityManager = new AbilityManager(this, eventBus);

        var movementData = creatuseSO.movementStrategy;
        // TO DO : abilities initialization

        movementHandler = new CreatureStrategyMovement(movementData, this);
        moveCommand = new MoveCommand(this, movementHandler);
    }

    // TODO: return also attack action
    public Command GetEndTurnAction() {
        return new EndTurnActions(moveCommand);
    }

    public void AssignField(Field field) {
        if (CurrentField != null) {
            CurrentField.UnAssignCreature();
            CurrentField.OnRemoval -= RemoveCreature;
        }
        field.OnRemoval += RemoveCreature;
        CurrentField = field;
    }

    public void RemoveCreature(Field field) {
        // Реакція на видалення поля, наприклад, переміщення на інше поле або помилка.
        Console.WriteLine($"Creature on field ({field.row}, {field.column}) is notified about its removal.");
        // - Вибір нового місця
        // - Знищення істоти
    }

    public AbilityManager GetAbilityManager() {
        return AbilityManager;
    }

    public Attack GetAttack() {
        return Attack;
    }

    public Health GetHealth() {
        return Health;
    }
}


public class EndTurnActions : Command {
    private Creature creature;

    private CreatureStrategyMovement strategyHandler;
    private Stack<Field> previousFields = new Stack<Field>();
    private MoveCommand MoveCommand;

    public EndTurnActions(MoveCommand moveCommand) {
        MoveCommand = moveCommand;
    }

    public async override UniTask Execute() {
        Debug.Log("End Turn actions begin");
        AddChild(MoveCommand);
        Debug.Log("End Turn actions end");
    }

    public async override UniTask Undo() {

    }
}
