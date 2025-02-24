using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BattleManager {
    private static readonly int OPPONENTS_TO_PLAY = 2;
    [Inject] private GameEventBus eventBus;
    public void StartBattle(List<Opponent> registeredOpponents) {
        ValidateOpponents(registeredOpponents);
        eventBus.Raise(new BattleStartedEvent(registeredOpponents));
    }

    public void EndBattle(Opponent testLooser) {
        eventBus.Raise(new BattleEndEventData(testLooser));
    }

    private void ValidateOpponents(List<Opponent> registeredOpponents) {
        if (registeredOpponents == null || registeredOpponents.Count < OPPONENTS_TO_PLAY) {
            throw new ArgumentException($"Requires at least {OPPONENTS_TO_PLAY} opponents");
        }
    }
}

public class TurnManager : IDisposable {
    public Action<Opponent> OnOpponentChanged;
    
    private List<Opponent> currentOpponents = new(2);
    public Opponent ActiveOpponent { get; private set; }
    private GameEventBus eventBus;
    private bool inTransition = false;
    private bool isDisabled = true;

    [Inject]
    public void Construct(GameEventBus eventBus) {
        this.eventBus = eventBus;
        eventBus.SubscribeTo<EndActionsExecutedEvent>(OnEndTurnActionsPerformed);
    }

    public void InitTurns(List<Opponent> registeredOpponents) {
        isDisabled = false;
        currentOpponents = new List<Opponent>(registeredOpponents);
        SwitchToNextOpponent(currentOpponents.GetRandomElement());
    }


    public bool EndTurnRequest(Opponent endTurnOpponent) {
        if (endTurnOpponent is Player)
        endTurnOpponent.Health.TakeDamage(2);
        if (inTransition || isDisabled) {
            Debug.LogWarning($"Turn cannot be ended right now. Transition: {inTransition}, Disabled: {isDisabled}");
            return false;
        }

        if (endTurnOpponent != ActiveOpponent) {
            Debug.LogWarning($"{endTurnOpponent?.Name} is not the active opponent and cannot end the turn.");
            return false;
        }

        inTransition = true;
        eventBus.Raise(new TurnEndStartedEvent(ActiveOpponent));
        return true;
    }


    private void OnEndTurnActionsPerformed(ref EndActionsExecutedEvent eventData) {
        SwitchToNextOpponent();
        inTransition = false;
    }

    private void SwitchToNextOpponent(Opponent starterOpponent = null) {
        var previous = ActiveOpponent;
        ActiveOpponent = (starterOpponent != null && currentOpponents.Contains(starterOpponent))
            ? starterOpponent
            : GetNextOpponent();


        Debug.Log($"Turn started for {ActiveOpponent.Name}");
        OnOpponentChanged?.Invoke(ActiveOpponent);
        eventBus.Raise(new OnTurnStart(ActiveOpponent));
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
    public Opponent startTurnOpponent;

    public OnTurnStart(Opponent startTurnOpponent) {
        this.startTurnOpponent = startTurnOpponent;
    }
}
