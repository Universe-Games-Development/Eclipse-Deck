using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Creature : IDamageable, IDamageDealer, IGameUnit {
    public Field CurrentField { get; private set; }
    public Func<Field, UniTask> OnInterruptedMove;
    public Func<Field, UniTask> OnMoved;
    public Func<Field, UniTask> OnSpawned;
    public event Action<GameEnterEvent> OnUnitDeployed;
    public Opponent ControlOpponent { get; private set; }
    public Health Health => creatureCard.Stats.Health;
    public Attack Attack => creatureCard.Stats.Attack;
    public Ability AttackAbility { get; private set; }

    public EffectManager EffectManager => creatureCard.EffectManager;

    public CreatureCard creatureCard;
    private CreatureBehaviour craetureBehaviour;
    
    public Creature(CreatureCard creatureCard, CreatureBehaviour craetureBehaviour, GameEventBus eventBus) {
        this.creatureCard = creatureCard;

        // Soon we define how to get the creatureSO
        CreatureCardData creatureData = creatureCard.CreatureCardData;
        var movementData = creatureData.movementData;
        if (movementData == null) throw new ArgumentNullException("Movement Data not set in " + GetType().Name);
        // TO DO : abilities initialization
        this.craetureBehaviour = craetureBehaviour;
        craetureBehaviour.InitStrategies(this, creatureData);

    }

    private void InitializeAttackAbility() {
        // ������� ������� ��� ����������� �����
        var attackTrigger = new List<AbilityTrigger>(); // ��� ����� ������ ������������ ����������� ������� �� UI

        // ������� �������� ��������� ����� � ��������� this ��� ��������� �����
        var dealDamageOperation = new DealDamageOperation(this);

        // ������� �����������
        AttackAbility = new Ability(attackTrigger, creatureCard);
        AttackAbility.Operations.Add(dealDamageOperation);
    }

    public void Spawn(Field fieldToSpawn) {
        fieldToSpawn.PlaceCreature(this);
        AssignField(fieldToSpawn);
        OnSpawned?.Invoke(fieldToSpawn);
        OnUnitDeployed?.Invoke(new GameEnterEvent(this));
    }

    public void AssignField(Field field) {
        if (CurrentField != null) {
            CurrentField.RemoveCreature();
            CurrentField.FieldRemoved -= RemoveCreature;
        }
        field.FieldRemoved += RemoveCreature;
        CurrentField = field;
    }

    public void RemoveCreature(Field field) {
        // ������� �� ��������� ����, ���������, ���������� �� ���� ���� ��� �������.
        Console.WriteLine($"Creature on field ({field.Row}, {field.Column}) is notified about its removal.");
        // - ���� ������ ����
        // - �������� ������
    }

    internal Command GetEndTurnAction() {
        throw new NotImplementedException();
    }
}


public class EndTurnActions : Command {
    public async override UniTask Execute() {
        Debug.Log("End Turn actions begin");
        await UniTask.CompletedTask;
        Debug.Log("End Turn actions end");
    }

    public async override UniTask Undo() {
        await UniTask.CompletedTask;
    }
}

public class CreatureAttackCommand : Command {
    private Creature creature;
    private IAttackStrategy attackStrategy;

    public CreatureAttackCommand(Creature creature, IAttackStrategy attackStrategy) {
        this.creature = creature;
        this.attackStrategy = attackStrategy;
    }

    public override async UniTask Execute() {
        AttackData attackData = attackStrategy.CalculateAttackData();
        if (attackData.fieldDamageData == null) {
            Debug.LogWarning("Empty attack data in " + GetType().Name);
            return;
        }
        foreach (var fieldDAmage in attackData.fieldDamageData) {
            Field field = fieldDAmage.Key;
            field.ApplyDamage(fieldDAmage.Value);
        }
        await UniTask.CompletedTask;
    }

    public override UniTask Undo() {
        throw new NotImplementedException();
    }
}

public struct GameEnterEvent : IEvent {
    public IGameUnit Summoned;
    public GameEnterEvent(IGameUnit summoned) {
        Summoned = summoned;
    }
}