using System;
using UnityEngine;

public class Field : ITipProvider {
    [Header ("Actiopns")]
    public Action OnOccupiedByCreature;
    public Action OnChangedOwner;
    public Action<FieldType> OnChangedType;

    [Header("Board Set-up")]
    public Opponent Owner { get; private set; }
    public int row;
    public int column;
    private FieldType type = FieldType.Support;
    public FieldType Type {
        get { return type; }
        set {
            type = value;
            OnChangedType?.Invoke(type);
        }
    }
    [Header("Game Board Params")]
    public Creature OccupiedCreature { get; private set; }

    public Field(int row, int column) {
        this.row = row;
        this.column = column;
    }

    public bool AssignCreature(Creature creature) {
        if (OccupiedCreature != null) {
            Debug.Log($"{row} / {column} already occupied by creature");
            return false;
        }
        OccupiedCreature = creature;
        return true;
    }

    public void RemoveCreature() {
        OccupiedCreature = null;
    }

    public void ReceiveAttack(int damage) {
        if (OccupiedCreature != null) {
            OccupiedCreature.Health.ApplyDamage(damage);
        } else {
            if (Owner) {
                Owner.health.ApplyDamage(damage);
                Debug.Log($"{Owner.Name} takes {damage} damage, because field {Owner} empty.");
            } else {
                Debug.Log($"Nobody takes {damage} damage");
            }
        }
    }

    public bool IsEmpty() {
        return OccupiedCreature == null;
    }

    public void AssignOwner(Opponent player1) {
        if (Owner != null) {
            Debug.LogWarning($"{row} / {column} already occupied by owner");
        }
        Owner = player1;
    }

    public string GetInfo() {
        return "Field";
    }

    internal void OnRemoveFromGrid() {
        throw new NotImplementedException();
    }
}
