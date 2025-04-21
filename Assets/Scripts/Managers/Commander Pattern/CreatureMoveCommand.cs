using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CreatureMoveCommand : Command {
    private Creature creature;
    private IMoveStrategy moveStrategy;
    private Stack<Field> previousFields = new Stack<Field>();

    public CreatureMoveCommand(Creature creature, IMoveStrategy moveStrategy) {
        this.creature = creature;
        this.moveStrategy = moveStrategy;
    }

    public override async UniTask Execute() {
        Field lastField = creature.CurrentField;
        List<FieldsPath> paths = moveStrategy.CalculatePath();

        if (paths.Count == 0) {
            return;
        }

        foreach (FieldsPath path in paths) {
            if (path.fields == null) {
                return;
            }

            bool hasMoved = false;
            for (int i = 0; i < path.fields.Count; i++) {
                hasMoved = await TryMoveToField(path.fields[i], creature);
                if (!hasMoved) {
                    break;
                }
                previousFields.Push(lastField);
            }
            if (!hasMoved) {
                break;
            }
        }
    }

    public async override UniTask Undo() {
        while (previousFields.Count > 0) {
            var lastField = previousFields.Pop();
            if (lastField.OccupyingCreature != null) continue; // Пропускаємо зайняті поля

            bool placeResult = await TryMoveToField(lastField, creature);
            if (placeResult) {
                Debug.Log($"Undo: Moved back to {lastField.GetCoordinatesText()}");
                return;
            }
        }

        await UniTask.CompletedTask;
    }

    public async UniTask<bool> TryMoveToField(Field field, Creature creature) {
        bool hasMoved = false;
        if (field == creature.CurrentField) return true;

        hasMoved = field.PlaceCreature(creature);

        Debug.Log($"{(hasMoved ? "Failed" : "Made")} move to {field.GetCoordinatesText()}");
        if (hasMoved) {
            if (creature.OnMoved != null) {
                await creature.OnMoved.Invoke(field);
            }
            creature.AssignField(field);
        } else {
            if (creature.OnInterruptedMove != null) {
                await creature.OnInterruptedMove.Invoke(field);
            }
        }

        return hasMoved;
    }
}
