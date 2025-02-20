using System;

public interface IHealthEntity {
    Health GetHealth();
}

public interface IDamageDealer {
    Attack GetAttack();
}

public interface IAbilitiesCaster {
    CardAbilityManager GetAbilityManager();
}