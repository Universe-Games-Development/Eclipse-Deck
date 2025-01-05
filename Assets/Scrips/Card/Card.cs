using System;
using System.Collections.Generic;
using UnityEngine;

public class Card {
    public string Id { get; private set; }  // ��������� ������������� �����
    public string ResourseId { get; private set; }  // ��������� ������������� �����
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Opponent Owner { get; private set; }
    public int Cost { get; private set; }
    public int Attack { get; private set; }
    public int Health { get; private set; }
    public CardState CurrentState { get; private set; }
    public List<string> AbilityDescriptions { get; private set; }
    public IEventManager EventManager { get; private set; }
    public Sprite MainImage { get; private set; }
    public Action<CardState> OnStateChanged { get; internal set; }

    private List<CardAbility> abilities;
    private List<IEffect> activeEffects;

    // ��������� ���������� �������������� � �����������
    public Card(CardSO cardSO, Opponent owner, IEventManager eventManager) {
        // ��������� ���������� �������������� �����
        Id = Guid.NewGuid().ToString();  // ������������� GUID ��� ����������
        ResourseId = cardSO.id;

        EventManager = eventManager;
        Owner = owner;
        Name = cardSO.cardName;
        Description = cardSO.description;
        Cost = cardSO.cost;
        Attack = cardSO.attack;
        Health = cardSO.health;
        MainImage = cardSO.mainImage;

        abilities = new List<CardAbility>();
        AbilityDescriptions = new List<string>();
        activeEffects = new List<IEffect>();

        foreach (var abilitySO in cardSO.abilities) {
            var ability = new CardAbility(abilitySO, this, eventManager);
            abilities.Add(ability);
            AbilityDescriptions.Add(abilitySO.abilityDescription);
        }
    }

    // ����� ��� ��������� �����
    public void TakeDamage(int damage) {
        Health -= damage;
        if (Health < 0) {
            Health = 0;  // ��� ���������� ����������� ������'�
        }
        Debug.Log($"{Name} ������� {damage} �����. ������� ������'�: {Health}");
    }

    public void ChangeState(CardState newState) {
        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }
}


// ��������� ��� ������
public interface IEffect {
    void ApplyEffect(ref int cost, ref int attack, ref int health);  // ������������ ������ �� �����
}

// ��������� ������ ��������� � ���������
public class StrengthEffect : IEffect {
    private int attackIncrease;
    private int healthIncrease;

    public StrengthEffect(int attackIncrease, int healthIncrease) {
        this.attackIncrease = attackIncrease;
        this.healthIncrease = healthIncrease;
    }

    public void ApplyEffect(ref int cost, ref int attack, ref int health) {
        attack += attackIncrease;
        health += healthIncrease;
    }
}

public class CurseEffect : IEffect {
    private int costIncrease;
    private int attackDecrease;
    private int healthDecrease;

    public CurseEffect(int costIncrease, int attackDecrease, int healthDecrease) {
        this.costIncrease = costIncrease;
        this.attackDecrease = attackDecrease;
        this.healthDecrease = healthDecrease;
    }

    public void ApplyEffect(ref int cost, ref int attack, ref int health) {
        cost += costIncrease;
        attack -= attackDecrease;
        health -= healthDecrease;
    }
}
