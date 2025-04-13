using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Creature : IHealthEntity, IDamageDealer, IAbilityOwner {
    public Field CurrentField { get; private set; }
    public Func<Field, UniTask> OnInterruptedMove;
    public Func<Field, UniTask> OnMoved;
    public Func<Field, UniTask> OnSpawned;

    public IHealth Health { get; private set; }

    public Attack Attack { get; private set; }

    public CreatureCard creatureCard;
    private CreatureBehaviour craetureBehaviour;
    
    public Creature(CreatureCard creatureCard, CreatureBehaviour craetureBehaviour, GameEventBus eventBus) {
        this.creatureCard = creatureCard;
        Health = new Health(this, creatureCard.Health, eventBus);
        Attack = new Attack(this, creatureCard.Attack, eventBus);
        
        // Soon we define how to get the creatureSO
        CreatureCardData creatureData = creatureCard.creatureCardData;

        var movementData = creatureData.movementData;
        if (movementData == null) throw new ArgumentNullException("Movement Data not set in " + GetType().Name);
        // TO DO : abilities initialization

        this.craetureBehaviour = craetureBehaviour;
        craetureBehaviour.InitStrategies(this, creatureData);
    }

    // TODO: return also attack action
    public Command GetEndTurnAction() {
        IMoveStrategy moveStrategy = craetureBehaviour.GetMovementStrategy(CurrentField);
        CreatureMoveCommand moveCommand = new CreatureMoveCommand(this, moveStrategy);
        IAttackStrategy attackStrategy = craetureBehaviour.GetAttackStrategy(CurrentField);
        CreatureAttackCommand attackCommand = new CreatureAttackCommand(this, attackStrategy);
        EndTurnActions endTurnCreatureCommands = new EndTurnActions();
        endTurnCreatureCommands.AddChild(moveCommand);
        endTurnCreatureCommands.AddChild(attackCommand);
        return endTurnCreatureCommands;
    }

    public void Spawn(Field fieldToSpawn) {
        fieldToSpawn.AssignCreature(this);
        AssignField(fieldToSpawn);
        OnSpawned?.Invoke(fieldToSpawn);
    }

    public void AssignField(Field field) {
        if (CurrentField != null) {
            CurrentField.UnAssignCreature();
            CurrentField.OnRemoval -= RemoveCreature;
        }
        field.OnRemoval += RemoveCreature;
        CurrentField = field;
    }

    public void RemoveCreature(Field field) {
        // ������� �� ��������� ����, ���������, ���������� �� ���� ���� ��� �������.
        Console.WriteLine($"Creature on field ({field.row}, {field.column}) is notified about its removal.");
        // - ���� ������ ����
        // - �������� ������
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