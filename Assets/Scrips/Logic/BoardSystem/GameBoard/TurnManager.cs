using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Zenject;

public class TurnManager : IEventListener {
    public Opponent ActiveOpponent { get; private set; }
    public OpponentRegistrator registrator;

    [Inject] private IEventQueue eventQueue;

    [Inject]
    public void Construct(OpponentRegistrator registrator) {
        this.registrator = registrator;
        registrator.OnOpponentsRegistered += InitTurns;
        registrator.OnOpponentUnregistered += RemoveOpponent;

        // Реєстрація для події CREATURES_ACTIONED
        eventQueue.RegisterListener(this, EventType.CREATURES_ACTIONED);
    }

    // Імплементація OnEventReceived з IEventListener
    public object OnEventReceived(object data) {
        if (data is CreaturesPerformedTurnsData) {
            // Якщо всі істоти виконали свої дії, розпочинаємо хід наступного опонента
            Debug.Log("All creatures performed their actions. Moving to the next turn.");
            TriggerNextTurn();
        }

        return new EmptyCommand();
    }

    private void InitTurns(List<Opponent> registeredOpponents) {
        foreach (var opponent in registeredOpponents) {
            opponent.OnDefeat += RemoveOpponent;
        }

        if (registeredOpponents.Count > 1) {
            ActiveOpponent = registrator.GetRandomOpponent();
            Opponent NonActiveOpponent = registrator.GetNextOpponent(ActiveOpponent);
            eventQueue.TriggerEvent(EventType.ON_TURN_START, new TurnChangeEventData(ActiveOpponent, NonActiveOpponent));
        }
    }

    public async void EndTurn(Opponent endTurnOpponent) {
        if (endTurnOpponent == null || endTurnOpponent != ActiveOpponent) {
            Debug.LogWarning($"{endTurnOpponent.Name} cannot perform a turn right now!");
            return;
        }

        if (!registrator.IsAllRegistered()) {
            Debug.LogWarning("Not all registered opponents left. Game should end?");
            return;
        }

        await UniTask.Yield(); // Асинхронне завершення ходу

        ActiveOpponent = SetNextOpponent();
        eventQueue.TriggerEvent(EventType.ON_TURN_END, new TurnEndEventData(endTurnOpponent));

        TriggerNextTurn();
    }

    private void TriggerNextTurn() {
        // Цей метод буде викликаний після завершення всіх дій істот
        if (ActiveOpponent != null) {
            Debug.Log($"It is now {ActiveOpponent.Name}'s turn.");
            eventQueue.TriggerEvent(EventType.ON_TURN_START, new TurnChangeEventData(ActiveOpponent, null));
        }
    }

    public void RemoveOpponent(Opponent opponent) {
        if (opponent == null) return;

        opponent.OnDefeat -= RemoveOpponent;
        Debug.Log($"Opponent {opponent.Name} unregistered. Maybe end game?");

        if (ActiveOpponent == opponent) {
            ActiveOpponent = SetNextOpponent();
        }

        // Перевірка кількості опонентів після видалення
        if (registrator.GetActiveOpponents().Count <= 1) {
            Debug.Log("Game Over. Only one or no opponents left.");
            // Тут можна викликати подію завершення гри
        }
    }

    public Opponent GetActivePlayer() => ActiveOpponent;

    public Opponent SetNextOpponent() {
        ActiveOpponent = registrator.GetNextOpponent(ActiveOpponent);
        return ActiveOpponent;
    }
}
