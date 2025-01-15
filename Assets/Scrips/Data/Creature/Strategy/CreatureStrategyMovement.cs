using Cysharp.Threading.Tasks;
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

    public async UniTask<int> ExecuteMovement(GameContext gameContext) {
        if (gameContext.initialField == null) Debug.LogError("Handler can`t define field to move from");

        int moves = 0;
        if (gameContext.initialField.Type == FieldType.Attack) {
            moves += await attackMovementStrategy.Movement(gameContext);
        } else if (gameContext.initialField.Type == FieldType.Support) {
            moves += await supportMovementStrategy.Movement(gameContext);
        } else {
            Debug.Log("No movement strategy for this field");
        }
        return moves;
    }
}
