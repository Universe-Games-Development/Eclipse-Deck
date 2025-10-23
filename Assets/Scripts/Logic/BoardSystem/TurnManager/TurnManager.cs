using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public interface ITurnManager {
    Opponent ActiveOpponent { get; }
    int TurnCounter { get; }
    int RoundCounter { get; }

    void InitTurns(List<Opponent> opponents);
    bool EndTurnRequest(Opponent requester);
    void AddNextTurn(Opponent opponent, int priority = 0);
    void RemoveTurns(Opponent opponent);
    void Dispose();
}

public class TurnManager : ITurnManager, IDisposable {
    public event Action<Opponent> OnOpponentChanged;
    public event Action<TurnEndEvent> OnTurnEnd;
    public event Action<TurnStartEvent> OnTurnStart;
    public event Action<RoundStartEvent> OnRoundStart;

    public Opponent ActiveOpponent { get; private set; }
    public int TurnCounter { get; private set; }
    public int RoundCounter { get; private set; }

    private readonly IEventBus<IEvent> _eventBus;
    private readonly Queue<TurnSlot> _turnQueue = new();
    private readonly List<Opponent> _registeredOpponents = new();
    private bool _inTransition = false;
    private int _completedTurnsInRound = 0;

    [Inject]
    public TurnManager(IEventBus<IEvent> eventBus) {
        _eventBus = eventBus;
    }

    // ============================================
    // ОСНОВНИЙ ФУНКЦІОНАЛ
    // ============================================

    public void InitTurns(List<Opponent> opponents) {
        if (opponents == null || opponents.Count == 0) {
            Debug.LogError("No opponents provided for turn initialization");
            return;
        }

        ResetState();
        _registeredOpponents.Clear();
        _registeredOpponents.AddRange(opponents);

        // Додаємо початковий набір ходів
        RefillTurnQueue();

        // Стартуємо перший хід
        StartNextTurn();
    }

    public bool EndTurnRequest(Opponent requester) {
        if (!CanEndTurn(requester)) {
            return false;
        }

        _inTransition = true;
        EndCurrentTurn();
        StartNextTurn();
        _inTransition = false;

        return true;
    }

    // ============================================
    // УПРАВЛІННЯ ЧЕРГОЮ ХОДІВ
    // ============================================

    public void AddNextTurn(Opponent opponent, int priority = 0) {
        if (opponent == null || !_registeredOpponents.Contains(opponent)) {
            Debug.LogWarning("Cannot add turn for unregistered opponent");
            return;
        }

        var turnSlot = new TurnSlot(opponent, priority);

        // Додаємо з урахуванням пріоритету
        var tempList = _turnQueue.ToList();
        tempList.Add(turnSlot);
        tempList = tempList.OrderByDescending(t => t.Priority).ToList();

        _turnQueue.Clear();
        foreach (var slot in tempList) {
            _turnQueue.Enqueue(slot);
        }

        Debug.Log($"Added turn for {opponent.InstanceId} with priority {priority}");
    }

    public void RemoveTurns(Opponent opponent) {
        if (opponent == null) return;

        var tempList = _turnQueue.Where(slot => slot.Opponent != opponent).ToList();
        _turnQueue.Clear();
        foreach (var slot in tempList) {
            _turnQueue.Enqueue(slot);
        }

        Debug.Log($"Removed all turns for {opponent.InstanceId}");
    }

    private void RefillTurnQueue() {
        if (_registeredOpponents.Count == 0) return;

        // Додаємо по одному ходу для кожного гравця
        foreach (var opponent in _registeredOpponents) {
            _turnQueue.Enqueue(new TurnSlot(opponent, 0));
        }

        Debug.Log($"Turn queue refilled with {_turnQueue.Count} turns");
    }

    // ============================================
    // ВНУТРІШНЯ ЛОГІКА
    // ============================================

    private void StartNextTurn() {
        if (_turnQueue.Count == 0) {
            RefillTurnQueue();
            StartNewRound();
        }

        var previousOpponent = ActiveOpponent;
        var nextTurn = _turnQueue.Dequeue();
        ActiveOpponent = nextTurn.Opponent;

        TurnCounter++;
        _completedTurnsInRound++;

        Debug.Log($"Turn {TurnCounter} started for {ActiveOpponent.InstanceId}");

        // Сповіщення про початок ходу
        var turnStartEvent = new TurnStartEvent(TurnCounter, ActiveOpponent);
        OnTurnStart?.Invoke(turnStartEvent);
        _eventBus.Raise(turnStartEvent);

        var changeEvent = new OpponentTurnChangedEvent(previousOpponent, ActiveOpponent);
        _eventBus.Raise(changeEvent);
        OnOpponentChanged?.Invoke(ActiveOpponent);
    }

    private void EndCurrentTurn() {
        var turnEndEvent = new TurnEndEvent(ActiveOpponent);
        OnTurnEnd?.Invoke(turnEndEvent);
        _eventBus.Raise(turnEndEvent);

        Debug.Log($"Turn ended for {ActiveOpponent.InstanceId}");
    }

    private void StartNewRound() {
        RoundCounter++;
        _completedTurnsInRound = 0;

        Debug.Log($"Round {RoundCounter} started!");

        var roundStartEvent = new RoundStartEvent(RoundCounter, ActiveOpponent);
        OnRoundStart?.Invoke(roundStartEvent);
        _eventBus.Raise(roundStartEvent);
    }

    private bool CanEndTurn(Opponent requester) {
        if (_inTransition) {
            Debug.LogWarning("Turn transition in progress");
            return false;
        }

        if (requester != ActiveOpponent) {
            Debug.LogWarning($"{requester.InstanceId} is not the active opponent");
            return false;
        }

        return true;
    }

    private void ResetState() {
        _turnQueue.Clear();
        ActiveOpponent = null;
        TurnCounter = 0;
        RoundCounter = 0;
        _completedTurnsInRound = 0;
        _inTransition = false;
    }

    public void Dispose() {
        ResetState();
        _registeredOpponents.Clear();
    }

    // ============================================
    // ДОПОМІЖНИЙ КЛАС
    // ============================================

    private struct TurnSlot {
        public Opponent Opponent { get; }
        public int Priority { get; }

        public TurnSlot(Opponent opponent, int priority) {
            Opponent = opponent;
            Priority = priority;
        }
    }
}

// ============================================
// EVENT STRUCTURES
// ============================================

public struct TurnEndEvent : IEvent {
    public Opponent Opponent { get; }

    public TurnEndEvent(Opponent opponent) {
        Opponent = opponent;
    }
}

public struct TurnStartEvent : IEvent {
    public Opponent Opponent { get; }
    public int TurnNumber { get; }

    public TurnStartEvent(int turnNumber, Opponent opponent) {
        TurnNumber = turnNumber;
        Opponent = opponent;
    }
}

public struct RoundStartEvent : IEvent {
    public Opponent Opponent { get; }
    public int RoundNumber { get; }

    public RoundStartEvent(int roundNumber, Opponent opponent) {
        RoundNumber = roundNumber;
        Opponent = opponent;
    }
}

public struct OpponentTurnChangedEvent : IEvent {
    public Opponent PreviousOpponent { get; }
    public Opponent NextOpponent { get; }

    public OpponentTurnChangedEvent(Opponent previous, Opponent next) {
        PreviousOpponent = previous;
        NextOpponent = next;
    }
}