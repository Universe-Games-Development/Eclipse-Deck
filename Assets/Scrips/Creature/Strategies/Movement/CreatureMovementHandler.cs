using Cysharp.Threading.Tasks;
using UnityEngine;
public class CreatureMovementHandler {
    private readonly MovementStrategy attackMovementStrategy;
    private readonly MovementStrategy supportMovementStrategy;

    private readonly Creature currentCreature;
    public CreatureMovementHandler(CreatureMovementDataSO movementData, Creature creature) {
        // Thinking to make just  GetMoveStrategy method but there will be much enums
        attackMovementStrategy = MovementStrategyFactory.GetAttackStrategy(movementData.attackStrategyType, movementData);
        supportMovementStrategy = MovementStrategyFactory.GetSupportStrategy(movementData.supportStrategyType, movementData);
        currentCreature = creature;
    }

    public async UniTask ExecuteMovement(GameContext gameContext) {
        if (gameContext.initialField == null) Debug.LogError("Handler can`t define field to move from");

        int moves = 0;
        if (gameContext.initialField.Type == FieldType.Attack) {
            moves += await attackMovementStrategy.Move(gameContext);
        } else if (gameContext.initialField.Type == FieldType.Support) {
            moves += await supportMovementStrategy.Move(gameContext);
        } else {
            Debug.Log("No movement strategy for this field");
        }
        Debug.Log(currentCreature + " moved by " + moves + " times");
    }
}
