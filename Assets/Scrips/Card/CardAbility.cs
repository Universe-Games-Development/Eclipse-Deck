using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

public class CardAbility : IEventListener {
    public CardAbilitySO data;
    private Card card;
    private bool isActiveState = false;
    private IEventQueue eventQueue;

    public CardAbility(CardAbilitySO abilitySO, Card card, IEventQueue eventQueue) {
        this.data = abilitySO;
        this.card = card;
        this.eventQueue = eventQueue;

        card.OnStateChanged += OnCardStateChanged;

        CheckAndActivateAbility();

        //Debug.Log($"CardAbility created for card: {card.data.name} in state: {card.CurrentState}");
    }

    ~CardAbility() {
        card.OnStateChanged -= OnCardStateChanged;
        if (isActiveState)
            UnregisterActivation();
    }

    private void OnCardStateChanged(CardState newState) {
        CheckAndActivateAbility();
    }

    private void CheckAndActivateAbility() {
        if (card.CurrentState == data.activationState) {
            if (!isActiveState) {
                RegisterActivation();
            }
        } else {
            if (isActiveState) {
                UnregisterActivation();
            }
        }
    }

    public virtual void RegisterActivation() {
        if (isActiveState) {
            Debug.LogWarning($"Abilities for card {card.data.name} is already registered.");
            return;
        }

        //Debug.Log($"Registering ability for card: {card.data.name}");
        foreach (var abilityTrigger in data.eventTriggers) {
            eventQueue.RegisterListener(this, abilityTrigger);
        }

        isActiveState = true;
    }

    public virtual void UnregisterActivation() {
        //Debug.Log($"Unregistering ability for card: {card.data.name}");
        foreach (var abilityTrigger in data.eventTriggers) {
            eventQueue.UnregisterListener(this, abilityTrigger);
        }

        isActiveState = false;
    }


    // Need to return ability command

    // SUMMON CRETURE ALSO ABILITY
    public object OnEventReceived(object data) {
        ICommand command = null;

        switch (data) {
            case CardHandEventData cardHandEventData:
                Debug.Log($"Gathering future action for drawn card with name: {cardHandEventData.Card.data.name}");
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
        if (isActiveState) {
            UnregisterActivation();
        }

        Debug.Log($"Ability for card {card.data.name} has been reset.");
    }
}
