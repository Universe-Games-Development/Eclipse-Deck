using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : ICommand {
    private List<Path> paths;
    private Creature creature;
    private GameBoard gameBoard;
    private CreatureStrategyMovement strategyHandler;
    private GameContext gameContext;
    private Field fieldTEST;

    private Stack<Field> previousFields = new Stack<Field>(); // Стек для збереження попередніх полів

    public MoveCommand(Creature creature, CreatureStrategyMovement strategyHandler) {
        this.creature = creature;
        this.strategyHandler = strategyHandler;
    }

    public async UniTask Execute() {
        gameContext.initialField = fieldTEST; // Another field because gamecontext somehow forget it 
        paths = strategyHandler.GetPaths(gameContext);
        if (paths.Count == 0) {
            return;
        }

        foreach (Path path in paths) {
            for (int i = 0; i < path.fields.Count; i++) {
                if (path.isInterrupted && i == path.interruptedAt) {
                    creature.InterruptedMove();
                    break;
                }

                // Зберігаємо поточну позицію перед переміщенням
                if (creature.CurrentField != null) {
                    previousFields.Push(creature.CurrentField);
                }

                await TryMoveToField(path.fields[i], creature);
            }
        }
    }

    public async UniTask Undo() {
        // Повертаємося до попереднього стану
        while (previousFields.Count > 0) {
            var lastField = previousFields.Pop();
            bool placeResult = await lastField.PlaceCreatureAsync(creature);

            if (!placeResult) {
                Debug.LogWarning($"Failed to undo move to {lastField.row} / {lastField.column}. Field may be occupied or invalid.");
                break;
            }

            Debug.Log($"Undo: Moved back to {lastField.row} / {lastField.column}");
        }
    }

    public async UniTask<bool> TryMoveToField(Field field, Creature creature) {
        creature.CurrentField?.UnAssignCreature();
        bool placeResult = await field.PlaceCreatureAsync(creature);
        if (!placeResult) {
            Debug.LogWarning($"Failed to move to {field.row} / {field.column}. Field may be occupied or invalid.");
            return false;
        }

        Debug.Log($"Moved to {field.row} / {field.column}");
        return true;
    }

    public void SetGameContext(GameContext gameContext) {
        this.gameContext = gameContext;
        gameBoard = gameContext.gameBoard;
        fieldTEST = gameContext.initialField;
    }
}
