using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Field {
    // Properties with proper access modifiers
    public int Row { get; }
    public int Column { get; }
    public BoardPlayer Owner { get; private set; }
    public FieldType FieldType { get; private set; } = FieldType.Support;
    public Creature OccupyingCreature { get; private set; }

    // Events with proper naming conventions
    public event Action<BoardPlayer> OwnerChanged;
    public event Action<FieldType> TypeChanged;
    public event Action<Creature> CreaturePlaced;
    public event Action<Creature> CreatureRemoved;
    public event Action<Field> FieldRemoved;
    public event Action<bool> SelectionToggled;

    public Field(int row, int column, FieldType fieldType = FieldType.Empty) {
        Row = row;
        Column = column;
        FieldType = fieldType;
    }

    public void ApplyDamage(int damage) {
        if (HasCreature) {
            OccupyingCreature.Health.TakeDamage(damage);
        } else if (Owner != null) {
            Owner.Health.TakeDamage(damage);
            FieldLogger.Log($"{Owner} takes {damage} damage.");
        } else {
            FieldLogger.Log($"Nobody takes {damage} damage");
        }
    }

    public void SetFieldType(FieldType newType) {
        if (FieldType != newType) {
            FieldType = newType;
            OnTypeChanged(newType);
        }
    }

    public void SetOwner(BoardPlayer player) {
        if (Owner == player) return;

        Owner = player;
        OnOwnerChanged(player);
    }

    public void ClearOwner() {
        if (Owner == null) return;

        Owner = null;
        OnOwnerChanged(null);
    }

    public bool PlaceCreature(Creature creature) {
        if (!CanPlaceCreature(creature)) return false;

        OccupyingCreature = creature;
        OnCreaturePlaced(creature);
        return true;
    }

    public void RemoveCreature() {
        if (OccupyingCreature == null) return;

        var creature = OccupyingCreature;
        OccupyingCreature = null;
        OnCreatureRemoved(creature);
    }

    public bool CanPlaceCreature(Creature creature) {
        return OccupyingCreature == null && creature != null;
    }

    public bool HasCreature => OccupyingCreature != null;
    public bool IsControlled => Owner != null;

    public void ToggleSelection(bool isSelected) {
        OnSelectionToggled(isSelected);
    }

    public void MarkForRemoval() {
        OnFieldRemoved(this);
    }

    public Vector3 GetWorldPosition() {
        return new Vector3(Row, 0, Column);
    }

    public string GetCoordinatesText() {
        return $"{Row} / {Column}";
    }

    // Protected event invokers
    protected virtual void OnOwnerChanged(BoardPlayer newOwner) {
        OwnerChanged?.Invoke(newOwner);
    }

    protected virtual void OnTypeChanged(FieldType newType) {
        TypeChanged?.Invoke(newType);
    }

    protected virtual void OnCreaturePlaced(Creature creature) {
        CreaturePlaced?.Invoke(creature);
    }

    protected virtual void OnCreatureRemoved(Creature creature) {
        CreatureRemoved?.Invoke(creature);
    }

    protected virtual void OnFieldRemoved(Field field) {
        FieldRemoved?.Invoke(field);
    }

    protected virtual void OnSelectionToggled(bool isSelected) {
        SelectionToggled?.Invoke(isSelected);
    }
}
