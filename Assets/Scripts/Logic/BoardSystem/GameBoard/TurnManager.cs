using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


public class TurnManager : IDisposable {
    public Action<Opponent> OnOpponentChanged;
    public Action<Opponent> OnTurnEnd;
    public Action<Opponent> OnTurnStart;

    private List<Opponent> currentOpponents = new(2);
    public Opponent ActiveOpponent { get; private set; }
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

    public void InitTurns(List<Opponent> registeredOpponents) {
        RoundCounter = 0;
        TurnCounter = 0;
        completedTurnsInRound = 0;
        currentOpponents = new List<Opponent>(registeredOpponents);
        SwitchToNextOpponent(currentOpponents.GetRandomElement());
        isDisabled = false;
    }

    public bool EndTurnRequest(bool isPlayer = false) {
        if (inTransition || isDisabled) {
            Debug.LogWarning($"Turn cannot be ended right now. Transition: {inTransition}, Disabled: {isDisabled}");
            return false;
        }

        if (!(isPlayer && ActiveOpponent is Player)) {
            Debug.LogWarning($"Player is not the active opponent and cannot end the turn.");
            return false;
        }

        inTransition = true;
        OnTurnEnd?.Invoke(ActiveOpponent);
        eventBus.Raise(new TurnEndStartedEvent(ActiveOpponent));
        return true;
    }

    private void OnEndTurnActionsPerformed(ref EndActionsExecutedEvent eventData) {
        bool isRoundFinalTurn = UpdateRoundCounter();
        if (isRoundFinalTurn) {
            eventBus.Raise(new OnRoundStart(RoundCounter, ActiveOpponent));
        }
        SwitchToNextOpponent();
        inTransition = false;
    }

    private bool UpdateRoundCounter() {
        completedTurnsInRound++;
        bool isRoundFinalTurn = completedTurnsInRound >= currentOpponents.Count;

        if (isRoundFinalTurn) {
            RoundCounter++;
            Debug.Log($"Round {RoundCounter} started!");
            completedTurnsInRound = 0;
            return true;
        }
        return isRoundFinalTurn;
    }

    private void SwitchToNextOpponent(Opponent starterOpponent = null) {
        var previous = ActiveOpponent;
        ActiveOpponent = (starterOpponent != null && currentOpponents.Contains(starterOpponent))
            ? starterOpponent
            : GetNextOpponent();

        Debug.Log($"Turn started for {ActiveOpponent.Name}");
        OnOpponentChanged?.Invoke(ActiveOpponent);
        OnTurnStart?.Invoke(ActiveOpponent);
        TurnCounter++;
        eventBus.Raise(new OnTurnStart(TurnCounter, ActiveOpponent));
        eventBus.Raise(new TurnChangedEvent(previous, ActiveOpponent));
    }

    private Opponent GetNextOpponent() {
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



public struct TurnEndStartedEvent : IEvent {
    public Opponent endTurnOpponent;

    public TurnEndStartedEvent(Opponent endTurnOpponent) {
        this.endTurnOpponent = endTurnOpponent;
    }
}
public struct TurnChangedEvent : IEvent {
    public Opponent activeOpponent;
    public Opponent endTurnOpponent;

    public TurnChangedEvent(Opponent previous, Opponent next) {
        this.activeOpponent = previous;
        this.endTurnOpponent = next;
    }
}
public struct OnTurnStart : IEvent {
    public Opponent StartingOpponent { get; private set; }
    public int TurnNumber { get; private set; }
    public OnTurnStart(int turnCount, Opponent startTurnOpponent) {
        StartingOpponent = startTurnOpponent;
        TurnNumber = turnCount;
    }
}

public struct OnRoundStart : IEvent {
    public Opponent StartingOpponent { get; private set; }
    public int RoundNumber { get; private set; }
    public OnRoundStart(int roundCount, Opponent startingOpponent) {
        RoundNumber = roundCount;
        StartingOpponent = startingOpponent;
    }
}
