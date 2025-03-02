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
        List<Path> paths = moveStrategy.CalculatePath();

        if (paths.Count == 0) {
            return;
        }

        foreach (Path path in paths) {
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
            if (lastField.OccupiedCreature != null) continue; // Пропускаємо зайняті поля

            bool placeResult = await TryMoveToField(lastField, creature);
            if (placeResult) {
                Debug.Log($"Undo: Moved back to {lastField.GetTextCoordinates()}");
                return;
            }
        }

        await UniTask.CompletedTask;
    }

    public async UniTask<bool> TryMoveToField(Field field, Creature creature) {
        bool hasMoved = false;
        if (field == creature.CurrentField) return true;

        hasMoved = field.AssignCreature(creature);

        Debug.Log($"{(hasMoved ? "Failed" : "Made")} move to {field.GetTextCoordinates()}");
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
