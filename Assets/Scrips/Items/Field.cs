using System;
using UnityEngine;
using Zenject;

public class Field : TipItem {
    public Opponent Owner { get; private set; } // ������� ����� ����
    public BattleCreature OccupiedCreature { get; private set; } // ������ �� ��� (���� �)

    public int Index = 0;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject battleCreaturePrefab;

    internal bool SummonCreature(Card selectedCard) {
        if (OccupiedCreature != null) {
            Debug.Log($"{name} ��� �������");
            return false; // ���� ��� �������
        }

        GameObject battleCreatureObj = Instantiate(battleCreaturePrefab, spawnPoint);
        BattleCreature battleCreature = battleCreatureObj.GetComponent<BattleCreature>();
        battleCreature.Initialize(selectedCard, this, new SingleAttack());
        OccupiedCreature = battleCreature;
        return true;
    }


    // ����� ��� ��������� ������ � ����
    public void RemoveCreature() {
        // TO DO : Death
        OccupiedCreature = null;
    }

    // ����� ������� �����
    public void ReceiveAttack(int damage) {
        if (OccupiedCreature != null) {
            // ���� �� ��� � ������, ���� ������ �����
            OccupiedCreature.health.ApplyDamage(damage);
        } else {
            // ���� ���� ������, ����� ������ ������� ����
            Owner.health.ApplyDamage(damage);
            Debug.Log($"{Owner.Name} ������ {damage} �����, ���� �� ���� {Owner} ������.");
        }
    }

    public bool IsEmpty() {
        return OccupiedCreature == null;
    }

    public override string GetInfo() {
        string info = $"���� #{Index}\n�������: {Owner?.Name}";

        if (OccupiedCreature != null) {
            info += $"\n������: {OccupiedCreature.Name}" +
                $"\nHp: {OccupiedCreature.health.GetHealth()} " +
                $" Atk: {OccupiedCreature.GetAttack()}";
        } else {
            info += "\n���� ������.";
        }

        return info;
    }

    internal void AssignOwner(Opponent player1) {
        Owner = player1;
    }

    internal void SetFieldOwnerIndicator(Opponent owner) {
    }
}
