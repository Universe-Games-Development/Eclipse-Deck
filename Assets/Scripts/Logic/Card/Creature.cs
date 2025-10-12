using System;
using Zenject;

public class Creature : UnitModel, IHealthable, IAttacker {
    public Action OnDeath;
    public CreatureCardData Data { get; private set; }
    public Health Health { get; private set; }
    public Attack Attack { get; private set; }
    public Cost Cost { get; private set; }
    public string SourceCardId { get; private set; }

    [Inject] IEventBus<IEvent> eventBus;
    
    public Creature(CreatureCardData data, Health health, Attack attack, Cost cost, string cardID) {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Health = health;
        Attack = attack;
        Cost = cost;
        SourceCardId = cardID;

        UnitName = Data.Name;

        InstanceId = System.Guid.NewGuid().ToString();
    }

    public bool CanAttack() {
        return Health.Current > 0 && Attack.Current > 0;
    }

    public void Die() {
        eventBus.Raise(new DeathEvent(this));

        OnDeath?.Invoke();
    }

    #region IAttacker
    public int CurrentAttack => Attack.Current;

    #endregion

    #region IHealthable
    public bool IsDead => Health.IsDead;

    public int CurrentHealth => Health.Current;

    public void TakeDamage(int damage) {
        Health.TakeDamage(damage);
    }
    #endregion

    public override string ToString() {
        return $"{UnitName} ({Attack.Current}/{Health.Current})";
    }
}
