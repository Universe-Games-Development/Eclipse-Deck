using System;
using System.Collections.Generic;
using Zenject;
/*Stores all triggers and their data
 * When a trigger is activated, it checks if the conditions are met
 * THen manager notifies all subscribers and gives trigger data
 */
public class TriggerManager {
    private List<AbilityTrigger> _triggers = new();
    public Action<Opponent, IGameUnit, IEvent> OnTriggerActivation;

    public void AddTrigger(AbilityTrigger trigger) {
        if (!_triggers.Contains(trigger)) {
            _triggers.Add(trigger);
            trigger.OnTriggerActivation += OnTriggerActivation;
        }
    }
    public void RemoveTrigger(AbilityTrigger trigger) {
        if (_triggers.Contains(trigger)) {
            _triggers.Remove(trigger);
            trigger.OnTriggerActivation -= OnTriggerActivation;
        }
    }
}

public abstract class AbilityTrigger {
    [Inject] protected GameEventBus _eventBus;
    public Action<Opponent, IGameUnit, IEvent> OnTriggerActivation;
    public string TriggerName { get; protected set; }
    public abstract void ActivateTrigger(IGameUnit gameUnit);
    public abstract void DeactivateTrigger(IGameUnit gameUnit);
}

public class OnSelfSummonTrigger : AbilityTrigger {
    public OnSelfSummonTrigger() {
        TriggerName = "Summon";
    }

    // Logic for activating the trigger

    public override void ActivateTrigger(IGameUnit gameUnit) {
        gameUnit.OnUnitDeployed += HandleSummon;
    }
    public override void DeactivateTrigger(IGameUnit gameUnit) {
        gameUnit.OnUnitDeployed -= HandleSummon;
    }

    private void HandleSummon(GameEnterEvent eventData) {
        OnTriggerActivation?.Invoke(eventData.Summoned.ControlOpponent, eventData.Summoned, eventData);
    }
}

public class OnAnotherSummonTrigger : AbilityTrigger {
    public OnAnotherSummonTrigger() {
        TriggerName = "When another creature sommons";
    }

    public override void ActivateTrigger(IGameUnit gameUnit) {
        _eventBus.SubscribeTo<GameEnterEvent>(HandleSummon);
    }

    public override void DeactivateTrigger(IGameUnit gameUnit) {
        _eventBus.UnsubscribeFrom<GameEnterEvent>(HandleSummon);
    }

    private void HandleSummon(ref GameEnterEvent eventData) {
        OnTriggerActivation?.Invoke(eventData.Summoned.ControlOpponent, eventData.Summoned, eventData);
    }
}

