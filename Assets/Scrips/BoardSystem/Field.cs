using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Field : ITipProvider {
    [Header("Actions")]
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
            if (type != value) {
                type = value;
                OnChangedType?.Invoke(type);
            }
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
        OnOccupiedByCreature?.Invoke();
        return true;
    }

    public void RemoveCreature() {
        OccupiedCreature = null;
    }

    public void ReceiveAttack(int damage) {
        if (OccupiedCreature != null) {
            OccupiedCreature.Health.ApplyDamage(damage);
        } else {
            if (Owner != null) {
                Owner.health.ApplyDamage(damage);
                Debug.Log($"{Owner.Name} takes {damage} damage, because field {row} / {column} is empty.");
            } else {
                Debug.Log($"Nobody takes {damage} damage");
            }
        }
    }

    public void AssignOwner(Opponent player) {
        if (Owner != null) {
            Debug.LogWarning($"{row} / {column} already occupied by owner");
        }
        Owner = player;
        OnChangedOwner?.Invoke();
    }

    public string GetInfo() {
        return "Field";
    }

    public void NotifyRemoval() {
        // Remove animation
        if (OccupiedCreature != null) {
            OccupiedCreature.OnFieldRemoved(this);
        }
    }

    public async UniTask<bool> SummonCreatureAsync(Creature creature, Opponent summoner) {
        await UniTask.DelayFrame(1);

        if (summoner == null) {
            Debug.LogError("Summoner for field is nobody!");
            return false;
        }

        return PlaceCreature(creature);
    }

    public async UniTask<bool> PlaceCreatureAsync(Creature creature) {
        await UniTask.DelayFrame(1);
        return PlaceCreature(creature);
    }

    private bool PlaceCreature(Creature creature) {
        if (OccupiedCreature == null && creature != null) {
            OccupiedCreature = creature;
            OnOccupiedByCreature?.Invoke();
            return true;
        } else {
            Debug.Log($"Field at ({row}, {column}) is already occupied.");
            return false;
        }
    }
}
