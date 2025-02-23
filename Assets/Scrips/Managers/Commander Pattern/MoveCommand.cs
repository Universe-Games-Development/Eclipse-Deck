using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class MoveCommand : Command {
    private Creature creature;
    private Func<Field, UniTask> onMoved;
    private Func<UniTask> onInterrupted;
    private IMoveStrategy moveStrategy;
    private Stack<Field> previousFields = new Stack<Field>();

    public MoveCommand(Creature creature, IMoveStrategy moveStrategy, Func<Field, UniTask> onMoved, Func<UniTask> onInterrupted) {
        this.creature = creature;
        this.moveStrategy = moveStrategy;
        this.onMoved = onMoved;
        this.onInterrupted = onInterrupted;
    }

    public async override UniTask Execute() {
        Field currentField = creature.CurrentField;

        List<Path> paths = moveStrategy.CalculatePath(currentField);
        if (paths.Count == 0) {
            return;
        }

        foreach (Path path in paths) {
            for (int i = 0; i < path.fields.Count; i++) {
                if (path.isInterrupted && i == path.interruptedAt) {
                    if (onInterrupted != null) {
                        await onInterrupted.Invoke();
                    }
                    
                    Debug.Log("INTERRUPTED to MOVE! ANIMATION NEEDED");
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

    public async override UniTask Undo() {
        // Повертаємося до попереднього стану
        while (previousFields.Count > 0) {
            var lastField = previousFields.Pop();
            bool placeResult = lastField.AssignCreature(creature);

            if (!placeResult) {
                Debug.LogWarning($"Failed to undo move to {lastField.GetTextCoordinates()}. Field may be occupied or invalid.");
                break;
            }

            Debug.Log($"Undo: Moved back to {lastField.GetTextCoordinates()}");
        }
    }

    public async UniTask<bool> TryMoveToField(Field field, Creature creature) {
        if (field == creature.CurrentField) {
            return true;
        }
        bool placeResult = field.AssignCreature(creature);

        if (!placeResult) {
            Debug.LogWarning($"Failed to move to {field.GetTextCoordinates()}. Field may be occupied or invalid.");
            creature.CurrentField?.AssignCreature(creature);
            return false;
        }

        creature.AssignField(field);

        if (onMoved != null) {
            await onMoved.Invoke(field);
        } else {
            Debug.LogWarning("onMoved has not subscribers!");
        }
        

        Debug.Log($"Moved to {field.GetTextCoordinates()}");
        return true;
    }
}
