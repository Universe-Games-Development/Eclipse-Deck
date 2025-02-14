using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Zenject;

public class TurnManager {
    public Opponent ActiveOpponent { get; private set; }
    public OpponentRegistrator registrator;

    [Inject] private GameEventBus eventBus;

    [Inject]
    public void Construct(OpponentRegistrator registrator) {
        this.registrator = registrator;
        registrator.OnOpponentsRegistered += InitTurns;
        registrator.OnOpponentUnregistered += RemoveOpponent;

        // Реєстрація для події CREATURES_ACTIONED
        eventBus.SubscribeTo<EndTurnActionsPerformed>(OnEndTurnACtionsPerformed);
    }

    private void OnEndTurnACtionsPerformed(ref EndTurnActionsPerformed eventData) {
        
    }

    // Імплементація OnEventReceived з IEventListener
    public void OnEventReceived(object data) {

        if (data is CreaturesPerformedTurnsData) {
            // Якщо всі істоти виконали свої дії, розпочинаємо хід наступного опонента
            Debug.Log("All creatures performed their actions. Moving to the next turn.");
            TriggerNextTurn();
        }
    }

    private void InitTurns(List<Opponent> registeredOpponents) {
        foreach (var opponent in registeredOpponents) {
            opponent.OnDefeat += RemoveOpponent;
        }

        if (registeredOpponents.Count > 1) {
            ActiveOpponent = registrator.GetRandomOpponent();
            Opponent NonActiveOpponent = registrator.GetNextOpponent(ActiveOpponent);
            eventBus.Raise(new OnTurnChange(ActiveOpponent, NonActiveOpponent));
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
        eventBus.Raise(new TurnEndEvent(endTurnOpponent));

        TriggerNextTurn();
    }

    private void TriggerNextTurn() {
        // Цей метод буде викликаний після завершення всіх дій істот
        if (ActiveOpponent != null) {
            Debug.Log($"It is now {ActiveOpponent.Name}'s turn.");
            eventBus.Raise(new OnTurnChange(ActiveOpponent, null));
            eventBus.Raise(new OnTurnStart(ActiveOpponent));
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


