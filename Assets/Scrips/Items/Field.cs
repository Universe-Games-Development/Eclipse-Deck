using System;
using UnityEngine;

public class Field : TipItem {
    public Opponent Owner { get; private set; } // Власник цього поля
    public BattleCreature OccupiedCreature { get; private set; } // Істота на полі (якщо є)

    public int Index = 0;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject battleCreaturePrefab;

    public void InitializeField(Opponent owner) {
        Owner = owner;
        OccupiedCreature = null;
    }

    internal void SummonCreature(Card selectedCard) {
        GameObject battleCreatureObj = Instantiate(battleCreaturePrefab, spawnPoint);
        BattleCreature battleCreature = battleCreatureObj.GetComponent<BattleCreature>();
        battleCreature.Initialize(selectedCard, this, new SingleAttack());
    }

    // Метод для додавання істоти на поле
    public bool PlaceCreature(BattleCreature creature) {
        if (OccupiedCreature == null) {
            OccupiedCreature = creature;
            return true;
        }
        return false; // Поле вже зайняте
    }

    // Метод для видалення істоти з поля
    public void RemoveCreature() {
        OccupiedCreature = null;
    }

    // Метод обробки атаки
    public void ReceiveAttack(int damage) {
        if (OccupiedCreature != null) {
            // Якщо на полі є істота, вона отримує шкоду
            OccupiedCreature.TakeDamage(damage);
        } else {
            // Якщо поле порожнє, шкоду отримує власник поля
            Owner.TakeDamage(damage);
            Debug.Log($"{Owner.Name} отримує {damage} шкоди, тому що поле {Owner} порожнє.");
        }
    }

    public bool IsEmpty() {
        return OccupiedCreature == null;
    }

    public override string GetInfo() {
        string info = $"Поле #{Index}\nВласник: {Owner?.Name}";

        if (OccupiedCreature != null) {
            info += $"\nІстота: {OccupiedCreature.Name}\nЗдоров'я: {OccupiedCreature.health.GetHealth()}";
        } else {
            info += "\nПоле порожнє.";
        }

        return info;
    }

    internal void AssignOwner(Opponent player1) {
        Owner = player1;
    }

    internal void SetFieldOwnerIndicator(Opponent owner) {
        Debug.Log("Field assigned to : " + owner.Name);
    }
}
