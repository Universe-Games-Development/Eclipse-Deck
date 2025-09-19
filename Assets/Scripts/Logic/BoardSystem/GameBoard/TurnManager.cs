using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


public class TurnManager : IDisposable {
    public Action<BoardPlayerPresenter> OnOpponentChanged;
    public Action<TurnEndEvent> OnTurnEnd;
    public Action<TurnStartEvent> OnTurnStart;

    private List<BoardPlayerPresenter> currentOpponents = new(2);
    public BoardPlayerPresenter ActiveOpponent { get; private set; }
    public int TurnCounter { get; private set; }

    private IEventBus<IEvent> eventBus;
    private bool inTransition = false;
    private bool isDisabled = true;
    private int RoundCounter;
    private int completedTurnsInRound = 0; // Лічильник завершених ходів

    [Inject]
    public void Construct(IEventBus<IEvent> eventBus) {
        this.eventBus = eventBus;
    }

    public void InitTurns(List<BoardPlayerPresenter> registeredOpponents) {
        RoundCounter = 0;
        TurnCounter = 0;
        completedTurnsInRound = 0;
        currentOpponents = new List<BoardPlayerPresenter>(registeredOpponents);
        currentOpponents.TryGetRandomElement(out var player);
        SwitchToNextOpponent(player);
        isDisabled = false;
    }

    public bool EndTurnRequest(BoardPlayerPresenter requester) {
        if (inTransition || isDisabled) {
            Debug.LogWarning($"Turn cannot be ended right now. Transition: {inTransition}, Disabled: {isDisabled}");
            return false;
        }

        if (!(requester == ActiveOpponent)) {
            Debug.LogWarning($"Player is not the active opponent and cannot end the turn.");
            return false;
        }

        inTransition = true;
        TurnEndEvent turnEndEvent = new TurnEndEvent(ActiveOpponent);
        OnTurnEnd?.Invoke(turnEndEvent);
        eventBus.Raise(turnEndEvent);
        return true;
    }

    private void PerformEndTurn() {
        inTransition = true;
        UpdateRoundCounter();
        SwitchToNextOpponent();
        inTransition = false;
    }

    private void UpdateRoundCounter() {
        completedTurnsInRound++;
        bool isRoundFinalTurn = completedTurnsInRound >= currentOpponents.Count;

        if (isRoundFinalTurn) {
            RoundCounter++;
            Debug.Log($"Round {RoundCounter} started!");
            completedTurnsInRound = 0;
            if (isRoundFinalTurn) {
                eventBus.Raise(new RoundStartEvent(RoundCounter, ActiveOpponent));
            }
        }

        TurnStartEvent turnStartEvent = new TurnStartEvent(TurnCounter, ActiveOpponent);
        TurnCounter++;
        OnTurnStart?.Invoke(turnStartEvent);
        eventBus.Raise(turnStartEvent);
    }

    private void SwitchToNextOpponent(BoardPlayerPresenter starterOpponent = null) {
        var previous = ActiveOpponent;
        ActiveOpponent = (starterOpponent != null && currentOpponents.Contains(starterOpponent))
            ? starterOpponent
            : GetNextOpponent();

        Debug.Log($"Turn started for {ActiveOpponent}");
        OnOpponentChanged?.Invoke(ActiveOpponent);
        eventBus.Raise(new OpponentTurnChangedEvent(previous, ActiveOpponent));
    }

    private BoardPlayerPresenter GetNextOpponent() {
        if (currentOpponents.Count == 0) return null;
        int index = (currentOpponents.IndexOf(ActiveOpponent) + 1) % currentOpponents.Count;
        return currentOpponents[index];
    }

    public void ResetTurnManager() {
        currentOpponents.Clear();
        isDisabled = true;
        completedTurnsInRound = 0;
    }

    public void Dispose() {
        ResetTurnManager();
    }
}



public struct TurnEndEvent : IEvent {
    public BoardPlayerPresenter endTurnOpponent;

    public TurnEndEvent(BoardPlayerPresenter endTurnOpponent) {
        this.endTurnOpponent = endTurnOpponent;
    }
}
public struct OpponentTurnChangedEvent : IEvent {
    public BoardPlayerPresenter activeOpponent;
    public BoardPlayerPresenter endTurnOpponent;

    public OpponentTurnChangedEvent(BoardPlayerPresenter previous, BoardPlayerPresenter next) {
        this.activeOpponent = previous;
        this.endTurnOpponent = next;
    }
}
public struct TurnStartEvent : IEvent {
    public BoardPlayerPresenter StartingOpponent { get; private set; }
    public int TurnNumber { get; private set; }
    public TurnStartEvent(int turnCount, BoardPlayerPresenter startTurnOpponent) {
        StartingOpponent = startTurnOpponent;
        TurnNumber = turnCount;
    }
}

public struct RoundStartEvent : IEvent {
    public BoardPlayerPresenter StartingOpponent { get; private set; }
    public int RoundNumber { get; private set; }
    public RoundStartEvent(int roundCount, BoardPlayerPresenter startingOpponent) {
        RoundNumber = roundCount;
        StartingOpponent = startingOpponent;
    }
}