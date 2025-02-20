public interface IPassiveAbility {
    bool IsActive { get; }
    void ToggleAbilityTriggering(bool enable);
    void RegisterTrigger();
    void DeregisterTrigger();
    bool ActivationCondition();
}

