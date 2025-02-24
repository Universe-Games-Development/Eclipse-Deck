using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Threading;

public class CreatureMoveCommand : Command {
    private Creature creature;
    private Func<Field, UniTask> onMoved;
    private Func<UniTask> onInterrupted;
    private IMoveStrategy moveStrategy;
    private Stack<Field> previousFields = new Stack<Field>();

    public CreatureMoveCommand(Creature creature, IMoveStrategy moveStrategy, Func<Field, UniTask> onMoved, Func<UniTask> onInterrupted) {
        this.creature = creature;
        this.moveStrategy = moveStrategy;
        this.onMoved = onMoved;
        this.onInterrupted = onInterrupted;
    }

    public override async UniTask Execute() {
        Field currentField = creature.CurrentField;
        List<Path> paths = moveStrategy.CalculatePath();

        if (paths.Count == 0) {
            return;
        }

        foreach (Path path in paths) {
            if (path.fields == null) {
                return;
            }

            int amountToMove = path.fields.Count;
            if (path.isInterrupted) {
                amountToMove = path.interruptedAt;
            }
            for (int i = 0; i < amountToMove; i++) {
                previousFields.Push(currentField);
                await TryMoveToField(path.fields[i], creature);
            }

            if (path.isInterrupted) {
                if (onInterrupted != null) {
                    await onInterrupted.Invoke();
                }
                break;
            }
        }
    }


    public async override UniTask Undo() {
        while (previousFields.Count > 0) {
            var lastField = previousFields.Pop();
            if (lastField.OccupiedCreature != null) continue; // ���������� ������ ����

            bool placeResult = lastField.AssignCreature(creature);
            if (placeResult) {
                Debug.Log($"Undo: Moved back to {lastField.GetTextCoordinates()}");
                return;
            }
        }

        Debug.LogWarning("Undo failed: No valid field to return!");
    }


    public async UniTask<bool> TryMoveToField(Field field, Creature creature) {
        if (field == creature.CurrentField) return true;
        bool placeResult = field.AssignCreature(creature);

        if (!placeResult) {
            Debug.LogWarning($"Failed to move to {field.GetTextCoordinates()}. Field may be occupied or invalid.");
            return false;
        }

        creature.AssignField(field);

        if (onMoved != null) {
            await onMoved.Invoke(field);
        }

        Debug.Log($"Moved to {field.GetTextCoordinates()}");
        return true;
    }
}
