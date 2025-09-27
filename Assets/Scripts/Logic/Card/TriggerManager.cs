using System;
using System.Collections.Generic;
using Zenject;
/*Stores all triggers and their data
 * When a trigger is activated, it checks if the conditions are met
 * THen manager notifies all subscribers and gives trigger data
 */
public class TriggerManager {
    private List<AbilityTrigger> _triggers = new();
    public Action<Opponent, UnitModel, IEvent> OnTriggerActivation;

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
    [Inject] protected IEventBus<IEvent> _eventBus;
    public Action<Opponent, UnitModel, IEvent> OnTriggerActivation;
    public string TriggerName { get; protected set; }
    public abstract void ActivateTrigger(UnitModel gameUnit);
    public abstract void DeactivateTrigger(UnitModel gameUnit);
}

public class OnSelfSummonTrigger : AbilityTrigger {
    public OnSelfSummonTrigger() {
        TriggerName = "Summon";
    }

    // Logic for activating the trigger

    public override void ActivateTrigger(UnitModel gameUnit) {
        //gameUnit.OnUnitDeployed += HandleSummon;
    }
    public override void DeactivateTrigger(UnitModel gameUnit) {
        //gameUnit.OnUnitDeployed -= HandleSummon;
    }

    private void HandleSummon(GameEnterEvent eventData) {
        OnTriggerActivation?.Invoke(eventData.Summoned.GetPlayer(), eventData.Summoned, eventData);
    }
}

public class OnAnotherSummonTrigger : AbilityTrigger {
    public OnAnotherSummonTrigger() {
        TriggerName = "When another creature sommons";
    }

    public override void ActivateTrigger(UnitModel gameUnit) {
        _eventBus.SubscribeTo<GameEnterEvent>(HandleSummon);
    }

    public override void DeactivateTrigger(UnitModel gameUnit) {
        _eventBus.UnsubscribeFrom<GameEnterEvent>(HandleSummon);
    }

    private void HandleSummon(ref GameEnterEvent eventData) {
        OnTriggerActivation?.Invoke(eventData.Summoned.GetPlayer(), eventData.Summoned, eventData);
    }
}

