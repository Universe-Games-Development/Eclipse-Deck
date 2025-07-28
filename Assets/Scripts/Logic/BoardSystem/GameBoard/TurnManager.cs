using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


public class TurnManager : IDisposable {
    public Action<BoardPlayer> OnOpponentChanged;
    public Action<TurnEndEvent> OnTurnEnd;
    public Action<TurnStartEvent> OnTurnStart;

    private List<BoardPlayer> currentOpponents = new(2);
    public BoardPlayer ActiveOpponent { get; private set; }
    public int TurnCounter { get; private set; }

    private GameEventBus eventBus;
    private bool inTransition = false;
    private bool isDisabled = true;
    private int RoundCounter;
    private int completedTurnsInRound = 0; // Лічильник завершених ходів

    [Inject]
    public void Construct(GameEventBus eventBus) {
        this.eventBus = eventBus;
        eventBus.SubscribeTo<EndActionsExecutedEvent>(OnEndTurnActionsPerformed);
    }

    public void InitTurns(List<BoardPlayer> registeredOpponents) {
        RoundCounter = 0;
        TurnCounter = 0;
        completedTurnsInRound = 0;
        currentOpponents = new List<BoardPlayer>(registeredOpponents);
        currentOpponents.TryGetRandomElement(out var player);
        SwitchToNextOpponent(player);
        isDisabled = false;
    }

    public bool EndTurnRequest(BoardPlayer requester) {
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

    private void OnEndTurnActionsPerformed(ref EndActionsExecutedEvent eventData) {
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

    private void SwitchToNextOpponent(BoardPlayer starterOpponent = null) {
        var previous = ActiveOpponent;
        ActiveOpponent = (starterOpponent != null && currentOpponents.Contains(starterOpponent))
            ? starterOpponent
            : GetNextOpponent();

        Debug.Log($"Turn started for {ActiveOpponent}");
        OnOpponentChanged?.Invoke(ActiveOpponent);
        eventBus.Raise(new OpponentTurnChangedEvent(previous, ActiveOpponent));
    }

    private BoardPlayer GetNextOpponent() {
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
        eventBus.UnsubscribeFrom<EndActionsExecutedEvent>(OnEndTurnActionsPerformed);
    }
}



public struct TurnEndEvent : IEvent {
    public BoardPlayer endTurnOpponent;

    public TurnEndEvent(BoardPlayer endTurnOpponent) {
        this.endTurnOpponent = endTurnOpponent;
    }
}
public struct OpponentTurnChangedEvent : IEvent {
    public BoardPlayer activeOpponent;
    public BoardPlayer endTurnOpponent;

    public OpponentTurnChangedEvent(BoardPlayer previous, BoardPlayer next) {
        this.activeOpponent = previous;
        this.endTurnOpponent = next;
    }
}
public struct TurnStartEvent : IEvent {
    public BoardPlayer StartingOpponent { get; private set; }
    public int TurnNumber { get; private set; }
    public TurnStartEvent(int turnCount, BoardPlayer startTurnOpponent) {
        StartingOpponent = startTurnOpponent;
        TurnNumber = turnCount;
    }
}

public struct RoundStartEvent : IEvent {
    public BoardPlayer StartingOpponent { get; private set; }
    public int RoundNumber { get; private set; }
    public RoundStartEvent(int roundCount, BoardPlayer startingOpponent) {
        RoundNumber = roundCount;
        StartingOpponent = startingOpponent;
    }
}