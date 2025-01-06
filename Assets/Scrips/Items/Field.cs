using UnityEngine;

public class Field : TipItem {
    public Opponent Owner { get; private set; } // Власник цього поля
    public BattleCreature OccupiedCreature { get; private set; } // Істота на полі (якщо є)

    public int Index = 0;
    [SerializeField] public Transform spawnPoint;
    [SerializeField] public Transform uiPoint;

    internal bool AssignCreature(BattleCreature creature) {
        if (OccupiedCreature != null) {
            Debug.Log($"{name} вже зайняте");
            return false; // Поле вже зайняте
        }
        OccupiedCreature = creature;
        return true;
    }


    // Метод для видалення істоти з поля
    public void RemoveCreature() {
        // TO DO : Death
        OccupiedCreature = null;
    }

    // Метод обробки атаки
    public void ReceiveAttack(int damage) {
        if (OccupiedCreature != null) {
            // Якщо на полі є істота, вона отримує шкоду
            OccupiedCreature.card.Health.ApplyDamage(damage);
        } else {
            // Якщо поле порожнє, шкоду отримує власник поля
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

    public override string GetInfo() {
        string info = $"Field #{Index}" +
            $"\nOwner: {Owner?.Name}";

        if (OccupiedCreature != null) {
            info += $"\n" +
                $"Creature: {OccupiedCreature.Name} + \n" +
                $"Hp: {OccupiedCreature.card.Health.CurrentValue} / " +
                $"Atk: {OccupiedCreature.GetAttack()}";
        } else {
            info += "\nEmpty field.";
        }

        return info;
    }

    internal void AssignOwner(Opponent player1) {
        Owner = player1;
    }

    internal void SetFieldOwnerIndicator(Opponent owner) {
    }
}
