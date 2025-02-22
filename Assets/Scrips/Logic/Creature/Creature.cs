using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Creature : IHealthEntity, IDamageDealer, IAbilityOwner {
    public Field CurrentField { get; private set; }
    public Action OnInterruptedMove;
    
    private CreatureCard creatureCard;
    private CreatureStrategyMovement movementHandler;

    protected Health _health;
    protected Attack _attack;
    
    public Creature(CreatureCard creatureCard, IMovementStrategyFactory strategyFactory, GameEventBus eventBus) {
        this.creatureCard = creatureCard;
        _health = new Health(this, creatureCard.Health, eventBus);
        _attack = new Attack(this, creatureCard.Attack, eventBus);
        
        // Soon we define how to get the creatureSO
        CreatureCardData creatureData = creatureCard.creatureCardData;

        var movementData = creatureData.movementStrategy;
        // TO DO : abilities initialization

        movementHandler = new CreatureStrategyMovement(strategyFactory, movementData);
    }

    // TODO: return also attack action
    public Command GetEndTurnAction() {
        IMoveStrategy moveStrategy = movementHandler.GetStrategy(CurrentField);
        MoveCommand moveCommand = new MoveCommand(this, moveStrategy);
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

    public Health GetHealth() {
        return _health;
    }

    public Attack GetAttack() {
        return _attack;
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
        await UniTask.CompletedTask;
        Debug.Log("End Turn actions end");
    }

    public async override UniTask Undo() {
        await UniTask.CompletedTask;
    }
}
