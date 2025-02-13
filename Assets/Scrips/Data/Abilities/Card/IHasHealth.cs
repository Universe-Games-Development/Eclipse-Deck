using System;

public interface IHasHealth {
    Health GetHealth();
}

public interface IDamageDealer {
    Attack GetAttack();
}

public interface IAbilityOwner {
    AbilityManager GetAbilityManager();
}