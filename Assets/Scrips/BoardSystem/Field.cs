using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Field : ITipProvider {
    [Header("Actions")]
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
            Debug.LogWarning($"{row} / {column} already occupied by {Owner.Name} but you trying to assign {player.Name}");
        }
        Owner = player;
        OnChangedOwner?.Invoke();
    }

    public string GetInfo() {
        return $"Field is ({{ {row} }} / {{ {column} }}).";
    }

    public void NotifyFieldRemoval() {
        // Remove animation
        OccupiedCreature?.OnFieldRemoved(this);
    }


    public async UniTask<bool> SummonCreatureAsync(Creature creature, Opponent summoner) {
        await UniTask.DelayFrame(1);

        bool canSummon = isSommonable(creature, summoner);
        if (!canSummon) {
            Debug.LogError("Summoner for field is nobody!");
            return false;
        }

        return PlaceCreature(creature);
    }


    public async UniTask<bool> PlaceCreatureAsync(Creature creature) {
        await UniTask.DelayFrame(1); // Імітація асинхронності (можна замінити на анімацію або затримку)
        return PlaceCreature(creature);
    }

    private bool PlaceCreature(Creature creature) {
        if (OccupiedCreature != null) {
            Debug.LogWarning($"Field at ({row}, {column}) is already occupied by another creature.");
            return false;
        }

        OccupiedCreature = creature;
        OccupiedCreature.AssignField(this);
        Debug.Log($"Creature placed on field ({row}, {column}).");
        return true;
    }

    public bool CanPlaceCreature(Creature creature) {
        return OccupiedCreature == null && creature != null;
    }

    public bool isSommonable(Creature creature, Opponent summoner) {
        return CanPlaceCreature(creature) && summoner == Owner;
    }

    public void RemoveCreature() {
        if (OccupiedCreature != null) {
            OccupiedCreature = null;
        } else {
            Debug.Log("Received remove but nothing to remove!");
        }
    }
}
