using System;
using UnityEngine;

public class Field : TipItem {
    public Opponent Owner { get; private set; } // ������� ����� ����
    public BattleCreature OccupiedCreature { get; private set; } // ������ �� ��� (���� �)

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

    // ����� ��� ��������� ������ �� ����
    public bool PlaceCreature(BattleCreature creature) {
        if (OccupiedCreature == null) {
            OccupiedCreature = creature;
            return true;
        }
        return false; // ���� ��� �������
    }

    // ����� ��� ��������� ������ � ����
    public void RemoveCreature() {
        OccupiedCreature = null;
    }

    // ����� ������� �����
    public void ReceiveAttack(int damage) {
        if (OccupiedCreature != null) {
            // ���� �� ��� � ������, ���� ������ �����
            OccupiedCreature.TakeDamage(damage);
        } else {
            // ���� ���� ������, ����� ������ ������� ����
            Owner.TakeDamage(damage);
            Debug.Log($"{Owner.Name} ������ {damage} �����, ���� �� ���� {Owner} ������.");
        }
    }

    public bool IsEmpty() {
        return OccupiedCreature == null;
    }

    public override string GetInfo() {
        string info = $"���� #{Index}\n�������: {Owner?.Name}";

        if (OccupiedCreature != null) {
            info += $"\n������: {OccupiedCreature.Name}\n������'�: {OccupiedCreature.health.GetHealth()}";
        } else {
            info += "\n���� ������.";
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
