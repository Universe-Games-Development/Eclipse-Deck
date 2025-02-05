using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

public class CardAbility : IEventListener {
    public CardAbilitySO data;
    private Card card;
    private bool InTriggerState = false;
    private IEventQueue eventQueue;

    public CardAbility(CardAbilitySO abilitySO, Card card, IEventQueue eventQueue) {
        this.data = abilitySO;
        this.card = card;
        this.eventQueue = eventQueue;

        card.OnStateChanged += CheckAndRegisterAbility;

        //Debug.Log($"CardAbility created for card: {card.data.name} in state: {card.CurrentState}");
    }

    ~CardAbility() {
        card.OnStateChanged -= CheckAndRegisterAbility;
        if (InTriggerState)
            UnregisterTrigger();
    }

    private void CheckAndRegisterAbility(CardState newState) {
        if (newState == data.activationState && !InTriggerState) {
            RegisterTrigger();
        } else if (InTriggerState) {
            UnregisterTrigger();
        }
    }

    public virtual void RegisterTrigger() {
        if (InTriggerState) {
            Debug.LogWarning($"Abilities for card {card.Data.name} is already registered.");
            return;
        }

        //Debug.Log($"Registering ability for card: {card.data.name}");
        foreach (var abilityTrigger in data.eventTriggers) {
            eventQueue.RegisterListener(this, abilityTrigger);
        }

        InTriggerState = true;
    }

    public virtual void UnregisterTrigger() {
        //Debug.Log($"Unregistering ability for card: {card.data.name}");
        foreach (var abilityTrigger in data.eventTriggers) {
            eventQueue.UnregisterListener(this, abilityTrigger);
        }

        InTriggerState = false;
    }


    // Need to return ability command

    // SUMMON CRETURE ALSO ABILITY
    public object OnEventReceived(object data) {
        ICommand command = null;

        switch (data) {
            case CardHandEventData cardHandEventData:
                Debug.Log($"Gathering future action for drawn card with name: {cardHandEventData.Card.Data.name}");
                // Можете додати додаткову логіку тут
                break;
            // Додаткові випадки для інших типів даних
            default:
                Debug.Log("Unhandled event type");
                break;
        }

        return command;
    }


    public void Reset() {
        if (InTriggerState) {
            UnregisterTrigger();
        }

        Debug.Log($"Ability for card {card.Data.name} has been reset.");
    }
}
