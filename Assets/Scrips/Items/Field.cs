using System;
using UnityEngine;
using Zenject;

public class Field : TipItem {
    public Opponent Owner { get; private set; } // Власник цього поля
    public BattleCreature OccupiedCreature { get; private set; } // Істота на полі (якщо є)

    public int Index = 0;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject battleCreaturePrefab;

    internal bool SummonCreature(Card selectedCard) {
        if (OccupiedCreature != null) {
            Debug.Log($"{name} вже зайняте");
            return false; // Поле вже зайняте
        }

        GameObject battleCreatureObj = Instantiate(battleCreaturePrefab, spawnPoint);
        BattleCreature battleCreature = battleCreatureObj.GetComponent<BattleCreature>();
        battleCreature.Initialize(selectedCard, this, new SingleAttack());
        OccupiedCreature = battleCreature;
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
            OccupiedCreature.health.ApplyDamage(damage);
        } else {
            // Якщо поле порожнє, шкоду отримує власник поля
            Owner.health.ApplyDamage(damage);
            Debug.Log($"{Owner.Name} отримує {damage} шкоди, тому що поле {Owner} порожнє.");
        }
    }

    public bool IsEmpty() {
        return OccupiedCreature == null;
    }

    public override string GetInfo() {
        string info = $"Поле #{Index}\nВласник: {Owner?.Name}";

        if (OccupiedCreature != null) {
            info += $"\nІстота: {OccupiedCreature.Name}" +
                $"\nHp: {OccupiedCreature.health.GetHealth()} " +
                $" Atk: {OccupiedCreature.GetAttack()}";
        } else {
            info += "\nПоле порожнє.";
        }

        return info;
    }

    internal void AssignOwner(Opponent player1) {
        Owner = player1;
    }

    internal void SetFieldOwnerIndicator(Opponent owner) {
    }
}
