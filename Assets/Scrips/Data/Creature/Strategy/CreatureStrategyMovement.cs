using System.Collections.Generic;
using UnityEngine;

public class CreatureStrategyMovement {
    private readonly IMoveStrategy attackMovementStrategy;
    private readonly IMoveStrategy supportMovementStrategy;

    private readonly Creature currentCreature;
    public CreatureStrategyMovement(CreatureMovementDataSO movementData, Creature creature) {
        // Thinking to make just  GetMoveStrategy method but there will be much enums
        attackMovementStrategy = movementData.attackStrategy.GetInstance();
        supportMovementStrategy = movementData.supportStrategy.GetInstance();
        currentCreature = creature;
    }

    public List<Path> GetPaths(GameContext gameContext) {
        if (gameContext.initialField == null) Debug.LogError("Handler can`t define field to move from");

        if (gameContext.initialField.Type == FieldType.Attack) {
            return attackMovementStrategy.CalculatePath(gameContext);
        } else if (gameContext.initialField.Type == FieldType.Support) {
            return supportMovementStrategy.CalculatePath(gameContext);
        }
        Debug.Log("No movement strategy for this creature");
        return null;
    }
}
