using System;

public interface IManaSystem {
}

public interface IHealthable {
    bool IsDead { get; }
    int CurrentHealth{ get; }

    void TakeDamage(int damage);
}

public interface IAttacker {
    int CurrentAttack { get; }
}
