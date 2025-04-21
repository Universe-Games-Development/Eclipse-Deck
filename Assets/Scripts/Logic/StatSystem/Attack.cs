using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Attack : Attribute {
    public GameEventBus eventBus;
    private readonly IDamageable _owner;
    private readonly GameEventBus _eventBus;

    public Attack(int initialValue, IDamageable owner, GameEventBus eventBus) : base(initialValue) {
        _owner = owner;
        _eventBus = eventBus;
    }

    internal void DealDamage(IDamageable target) {
        target.Health.TakeDamage(CurrentValue);
    }
}

public class CreatureFightOperation : GameOperation {
    private string ATTAKER_KEY = "attacker";
    private string TARGET_KEY = "target";
    public CreatureFightOperation() {
        // ������ ������� ������ �� ����������
        Requirements.Add(ATTAKER_KEY, new CreatureRequirement(new FriendlyCreatureCondition()));

        // ������ ������ ������ �� ���
        Requirements.Add(TARGET_KEY, new CreatureRequirement(new EnemyCreatureCondition()));
    }

    public override void PerformOperation() {
        if (!AreTargetsFilled()) {
            Debug.LogError("����������� ����� ��� ��������");
            return;
        }

        var attacker = Results["attacker"] as IDamageDealer;
        var target = Results[TARGET_KEY] as IDamageable;

        if (attacker != null && target != null) {
            // ������������� ����� ������
            attacker.Attack.DealDamage(target);
        }
    }
}
