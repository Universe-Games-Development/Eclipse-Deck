using System;

public interface IManaSystem {
}

public interface IHealthable {
    bool IsDead { get; }
    int CurrentHealth{ get; }
    float BaseValue { get; }

    void TakeDamage(int damage);
}

public interface IAttacker {
    int CurrentAttack { get; }
}
