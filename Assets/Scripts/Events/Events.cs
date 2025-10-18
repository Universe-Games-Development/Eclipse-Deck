
using System;

public struct OnDamageTaken : IEvent {
    public IAttacker Source { get; }
    public IHealthable Target { get; }
    public int Amount { get; }

    public OnDamageTaken(IHealthable target, IAttacker source, int amount) {
        Source = source;
        Target = target;
        Amount = amount;
    }
}

public struct DeathEvent : IEvent {
    public UnitModel DeadUnit { get; }

    public DeathEvent(UnitModel deadUnit) {
        DeadUnit = deadUnit;
    }
}


public readonly struct HoverUnitEvent : IEvent {
    public UnitPresenter UnitPresenter { get; }
    public bool IsHovered { get; }

    public HoverUnitEvent(UnitPresenter unitPresenter, bool isHovered) {
        UnitPresenter = unitPresenter;
        IsHovered = isHovered;
    }
}

public readonly struct ClickUnitEvent : IEvent {
    public UnitPresenter UnitPresenter { get; }

    public ClickUnitEvent(UnitPresenter unitPresenter) {
        UnitPresenter = unitPresenter;
    }
}

#region Card Play
public readonly struct CardActivatedEvent : IEvent {
    public Card Card { get; }
    public DateTime ActivationTime { get; }

    public CardActivatedEvent(Card card) {
        Card = card;
        ActivationTime = DateTime.UtcNow;
    }
}

public readonly struct CardPlayResult {
    public bool IsSuccess { get; }
    public bool IsCancelled { get; }
    public string ErrorMessage { get; }
    public int CompletedOperations { get; }

    private CardPlayResult(bool isSuccess, bool isCancelled, string errorMessage, int completedOperations) {
        IsSuccess = isSuccess;
        IsCancelled = isCancelled;
        ErrorMessage = errorMessage;
        CompletedOperations = completedOperations;
    }

    public static CardPlayResult Success(int completedOperations) =>
        new(true, false, null, completedOperations);

    public static CardPlayResult Failed(string errorMessage) =>
        new(false, false, errorMessage, 0);

    public static CardPlayResult Cancelled() =>
        new(false, true, "Cancelled", 0);

    public bool IsFailed => !IsSuccess && !IsCancelled;
}

public readonly struct CardOperationResultEvent : IEvent {
    public Card Card { get; }
    public int OperationIndex { get; }
    public int TotalOperations { get; }
    public OperationResult Result { get; }
    public DateTime CompletedTime { get; }

    public CardOperationResultEvent(Card card, int operationIndex, int totalOperations, OperationResult result) {
        Card = card;
        OperationIndex = operationIndex;
        TotalOperations = totalOperations;
        Result = result;
        CompletedTime = DateTime.UtcNow;
    }

    public bool IsLastOperation => OperationIndex == TotalOperations - 1;
    public float Progress => TotalOperations > 0 ? (float)(OperationIndex + 1) / TotalOperations : 0f;
}

public readonly struct CardPlaySessionStartedEvent : IEvent {
    public Card Card { get; }
    public int TotalOperations { get; }
    public DateTime StartTime { get; }

    public CardPlaySessionStartedEvent(Card card, int totalOperations) {
        Card = card;
        TotalOperations = totalOperations;
        StartTime = DateTime.UtcNow;
    }
}

public readonly struct CardPlaySessionEndedEvent : IEvent {
    public Card Card { get; }
    public CardPlayResult FinalResult { get; }
    public DateTime EndTime { get; }

    public CardPlaySessionEndedEvent(Card card, CardPlayResult finalResult) {
        Card = card;
        FinalResult = finalResult;
        EndTime = DateTime.UtcNow;
    }

    public bool WasSuccessful => FinalResult.IsSuccess;
    public bool WasCancelled => FinalResult.IsCancelled;
    public bool WasFailed => FinalResult.IsFailed;
}
#endregion