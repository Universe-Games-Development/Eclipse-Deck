using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class CardAbility : IEventListener {
    public CardAbilitySO data;
    private Card card;
    private bool isRegistered = false;
    private IEventManager eventManager;

    public CardAbility(CardAbilitySO abilitySO, Card card, IEventManager eventManager) {
        this.data = abilitySO;
        this.card = card;
        this.eventManager = eventManager;

        card.OnStateChanged += OnCardStateChanged;

        CheckAndRegisterAbility();

        Debug.Log($"CardAbility created for card: {card.data.name} in state: {card.CurrentState}");
    }

    ~CardAbility() {
        card.OnStateChanged -= OnCardStateChanged;
        UnregisterActivation();
    }

    private void OnCardStateChanged(CardState newState) {
        CheckAndRegisterAbility();
    }

    private void CheckAndRegisterAbility() {
        if (card.CurrentState == data.activationState) {
            if (!isRegistered) {
                RegisterActivation();
            }
        } else {
            if (isRegistered) {
                UnregisterActivation();
            }
        }
    }

    public virtual void RegisterActivation() {
        if (isRegistered) {
            Debug.LogWarning($"Ability for card {card.data.name} is already registered.");
            return;
        }

        Debug.Log($"Registering ability for card: {card.data.name}");
        eventManager.RegisterListener(this, data.eventTrigger, ExecutionType.Parallel);
        isRegistered = true;
    }

    public virtual void UnregisterActivation() {
        Debug.Log($"Unregistering ability for card: {card.data.name}");
        eventManager.UnregisterListener(this, data.eventTrigger);
        isRegistered = false;
    }

    // Обробка події
    public async UniTask OnEventAsync(EventType eventType, GameContext gameContext, CancellationToken cancellationToken = default) {
        if (!isRegistered) return; // Перевірка перед виконанням

        Debug.Log($"Event received: {eventType} for card: {card.data.name}");

        try {
            data.ActivateAbility(gameContext);

            // wait for effects
            await UniTask.Delay(1000, cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested) {
                Debug.Log("Event handling was cancelled.");
                return;
            }

            Debug.Log("Ability executed.");
        } catch (Exception e) {
            Debug.LogError($"Error during ability execution for card {card.data.name}: {e.Message}\n{e.StackTrace}");
        }
    }

    public void Reset() {
        if (isRegistered) {
            UnregisterActivation();
        }

        Debug.Log($"Ability for card {card.data.name} has been reset.");
    }
}
