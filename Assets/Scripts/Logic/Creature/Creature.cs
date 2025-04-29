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
    public BoardPlayer ControlledBy { get; private set; }
    public Health Health => creatureCard.Stats.Health;
    public Attack Attack => creatureCard.Stats.Attack;
    public Ability AttackAbility { get; private set; }

    public EffectManager Effects => creatureCard.Effects;

    public CreatureCard creatureCard;
    
    public Creature(CreatureCard creatureCard, GameEventBus eventBus) {
        this.creatureCard = creatureCard;

        // Soon we define how to get the creatureSO
        CreatureCardData creatureData = creatureCard.CreatureCardData;

    }

    private void InitializeAttackAbility() {
        // Создаем триггер для способности атаки
        var attackTrigger = new List<AbilityTrigger>(); // Для атаки обычно используется специальный триггер из UI

        // Создаем операцию нанесения урона с указанием this как источника урона
        var dealDamageOperation = new DealDamageOperation(this);

        // Создаем способность
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
        // Реакція на видалення поля, наприклад, переміщення на інше поле або помилка.
        Console.WriteLine($"Creature on field ({field.Row}, {field.Column}) is notified about its removal.");
        // - Вибір нового місця
        // - Знищення істоти
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
