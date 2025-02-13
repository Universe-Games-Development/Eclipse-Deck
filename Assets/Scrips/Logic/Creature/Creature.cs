using System;
using UnityEngine;

public class Creature : IAbilityOwner, IDamageDealer, IHasHealth {
    public Field CurrentField { get; private set; }

    public Health Health;
    public Attack Attack;

    private Card card;
    private CreatureStrategyMovement movementHandler;
    private MoveCommand moveCommand;

    public Creature(Card myCard, CreatureSO creatuseSO) {
        card = myCard;
        var movementData = creatuseSO.movementStrategy;
        // TO DO : abilities initialization

        movementHandler = new CreatureStrategyMovement(movementData, this);
        moveCommand = new MoveCommand(this, movementHandler);
    }

    // TODO: return also attack action
    public ICommand GetEndTurnMove() {
        return moveCommand;
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

    public void InterruptedMove() {
        Debug.Log("INTERRUPTED to MOVE! ANIMATION NEEDED");
    }

    internal bool IsAlive() {
        throw new NotImplementedException();
    }

    public AbilityManager GetAbilityManager() {
        throw new NotImplementedException();
    }

    public Attack GetAttack() {
        throw new NotImplementedException();
    }

    public Health GetHealth() {
        throw new NotImplementedException();
    }
}