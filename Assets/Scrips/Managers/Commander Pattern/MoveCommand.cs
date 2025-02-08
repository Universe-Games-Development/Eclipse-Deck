using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : ICommand {
    private Creature creature;

    private CreatureStrategyMovement strategyHandler;
    private Stack<Field> previousFields = new Stack<Field>();

    public MoveCommand(Creature creature, CreatureStrategyMovement strategyHandler) {
        this.creature = creature;
        this.strategyHandler = strategyHandler;
    }

    public async UniTask Execute() {
        Field currentField = creature.CurrentField;

        IMoveStrategy moveStrategy = strategyHandler.GetStrategy(currentField);

        List<Path> paths = moveStrategy.CalculatePath(currentField);
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
                if (currentField != null) {
                    previousFields.Push(currentField);
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
                Debug.LogWarning($"Failed to undo move to {lastField.GetTextCoordinates()}. Field may be occupied or invalid.");
                break;
            }

            Debug.Log($"Undo: Moved back to {lastField.GetTextCoordinates()}");
        }
    }

    public async UniTask<bool> TryMoveToField(Field field, Creature creature) {
        creature.CurrentField?.UnAssignCreature();

        bool placeResult = await field.PlaceCreatureAsync(creature);

        if (!placeResult) {
            Debug.LogWarning($"Failed to move to {field.GetTextCoordinates()}. Field may be occupied or invalid.");
            creature.CurrentField?.PlaceCreature(creature);
            return false;
        }

        Debug.Log($"Moved to {field.GetTextCoordinates()}");
        return true;
    }
}
