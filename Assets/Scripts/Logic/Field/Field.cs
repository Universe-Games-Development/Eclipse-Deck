using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Field {
    [Header("Grid Position")]
    public int row;
    public int column;

    [Header("Actions")]
    public Action<Opponent> OnChangedOwner;
    public Action<FieldType> OnChangedType;
    public Action<Creature> OnCreatureSummoned;
    public Action<Creature> OnCreaturePlaced;
    public Action<Creature> OnCreatureRemoved;

    public Action<Field> OnRemoval;
    public Action<bool> OnSelectToggled;
    [Header("Board Set-up")]
    public Opponent Owner { get; private set; }

    private FieldType type = FieldType.Support;

    public Field((int row, int column) coordinates) {
        row = coordinates.row;
        column = coordinates.column;
    }

    public FieldType FieldType {
        get { return type; }
        set {
            if (type != value) {
                type = value;
                OnChangedType?.Invoke(type);
            }
        }
    }

    [Header("Game Board Params")]
    public Creature Creature { get; private set; }

    public void ApplyDamage(int damage) {
        if (Creature != null) {
            Creature.Health.TakeDamage(damage);
        } else {
            if (Owner != null) {
                Owner.Health.TakeDamage(damage);
                FieldLogger.Log($"{Owner} takes {damage} damage.");
            } else {
                FieldLogger.Log($"Nobody takes {damage} damage");
            }
        }
    }

    public void ToggleSelection(bool value) {
        OnSelectToggled?.Invoke(value);
    }
    public void RemoveField() {
        OnRemoval?.Invoke(this);
    }

    #region Owner Logic
    public void AssignOwner(Opponent player) {
        if (Owner == player) {
            return;
        }
        Owner = player;
        OnChangedOwner?.Invoke(Owner);
    }

    public void UnassignOwner() {
        Owner = null;
        OnChangedOwner?.Invoke(Owner);
    }

    public bool IsControlled => Owner != null;
    #endregion

    #region Creature Logic

    public bool AssignCreature(Creature creature) {
        if (Creature != null) {
            FieldLogger.Warning($"Field at ({row}, {column}) is already occupied by another creature.");
            return false;
        }

        Creature = creature;
        OnCreaturePlaced?.Invoke(creature); // ¬икликаЇмо под≥ю
        FieldLogger.Log($"Creature placed on field ({row}, {column}).");
        return true;
    }

    public void UnAssignCreature() {
        if (Creature != null) {
            var removedCreature = Creature;
            Creature = null;
            OnCreatureRemoved?.Invoke(removedCreature); // ¬икликаЇмо под≥ю
        } else {
            FieldLogger.Log("Received remove but nothing to remove!");
        }
    }
    // To walk
    public bool CanPlaceCreature(Creature creature) {
        return !HasCreature && creature != null;
    }

    public bool HasCreature => Creature != null;
    #endregion

    public int GetRow() {
        return row;
    }

    public int GetColumn() {
        return column;
    }

    public string GetTextCoordinates() {
        return $"{GetRow()} / {GetColumn()}";
    }

    internal Vector3 GetCoordinates() {
        return new Vector3(row, 0, column);
    }

    internal bool CanSummonCreature() {
        if (Creature != null) {
            Debug.Log("Field is already occupied");
            return false;
        }
        return true;
    }
}
