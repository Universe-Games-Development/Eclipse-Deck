using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class CardAbility : IEventListener {
    private CardAbilitySO abilitySO;
    private Card card;
    private bool isRegistered = false;
    private IEventManager eventManager;

    public CardAbility(CardAbilitySO abilitySO, Card card, IEventManager eventManager) {
        this.abilitySO = abilitySO;
        this.card = card;
        this.eventManager = eventManager;

        // Підписка на подію зміни стану карти
        card.OnStateChanged += OnCardStateChanged;

        // Перевірка стану при ініціалізації
        CheckAndRegisterAbility();

        Debug.Log($"CardAbility created for card: {card.Name} in state: {card.CurrentState}");
    }

    // Деструктор для відписки від події при знищенні об'єкта
    ~CardAbility() {
        card.OnStateChanged -= OnCardStateChanged;
        UnregisterActivation();
    }

    // Обробка зміни стану картки
    private void OnCardStateChanged(CardState newState) {
        Debug.Log($"Card state changed from {card.CurrentState} to {newState} for card: {card.Name}");
        CheckAndRegisterAbility();
    }

    // Перевірка і реєстрація здібності, якщо стан підходить
    private void CheckAndRegisterAbility() {
        if (card.CurrentState == abilitySO.activationState) {
            if (!isRegistered) {
                RegisterActivation();
            }
        } else {
            if (isRegistered) {
                UnregisterActivation();
            }
        }
    }

    // Реєстрація здібності
    public virtual void RegisterActivation() {
        if (isRegistered) {
            Debug.LogWarning($"Ability for card {card.Name} is already registered.");
            return;
        }

        Debug.Log($"Registering ability for card: {card.Name}");
        eventManager.RegisterListener(this, abilitySO.eventTrigger, ExecutionType.Parallel);
        isRegistered = true;
    }

    // Відписка від здібності
    public virtual void UnregisterActivation() {
        Debug.Log($"Unregistering ability for card: {card.Name}");
        eventManager.UnregisterListener(this, abilitySO.eventTrigger);
        isRegistered = false;
    }

    // Обробка події
    public async UniTask OnEventAsync(EventType eventType, GameContext gameContext, CancellationToken cancellationToken = default) {
        if (!isRegistered) return; // Перевірка перед виконанням

        Debug.Log($"Event received: {eventType} for card: {card.Name}");

        try {
            // Викликаємо ActivateAbility з CardAbilitySO
            abilitySO.ActivateAbility(gameContext);  // Виклик метода ActivateAbility

            await UniTask.Delay(1000, cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested) {
                Debug.Log("Event handling was cancelled.");
                return;
            }

            Debug.Log("Ability executed.");
        } catch (Exception e) {
            Debug.LogError($"Error during ability execution for card {card.Name}: {e.Message}\n{e.StackTrace}");
        }
    }

}
