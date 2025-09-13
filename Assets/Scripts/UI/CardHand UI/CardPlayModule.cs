using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Zenject;

public interface IOperationFactory {
    TOperation Create<TOperation>(OperationData data) where TOperation : GameOperation;
    GameOperation Create(OperationData data);
}

[System.AttributeUsage(System.AttributeTargets.Class)]
public class OperationForAttribute : System.Attribute {
    public System.Type DataType { get; }
    public OperationForAttribute(System.Type dataType) {
        DataType = dataType;
    }
}

public class OperationFactory : IOperationFactory {
    private readonly DiContainer _container;
    private readonly Dictionary<Type, Type> _dataToOperationMap = new();

    public OperationFactory(DiContainer container) {
        _container = container;
        Initialize();
    }

    private void Initialize() {
        // Скануємо всі типи в поточній збірці
        var assembly = Assembly.GetExecutingAssembly();
        var operationTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(GameOperation).IsAssignableFrom(t))
            .Where(t => t.GetCustomAttribute<OperationForAttribute>() != null);

        foreach (var operationType in operationTypes) {
            var attribute = operationType.GetCustomAttribute<OperationForAttribute>();
            if (attribute.DataType != null && typeof(OperationData).IsAssignableFrom(attribute.DataType)) {
                _dataToOperationMap[attribute.DataType] = operationType;
            }
        }
    }

    public GameOperation Create(OperationData data) {
        if (data == null) throw new ArgumentNullException(nameof(data));

        if (_dataToOperationMap.TryGetValue(data.GetType(), out var operationType)) {
            return (GameOperation)_container.Instantiate(operationType, new object[] { data });
        }

        throw new InvalidOperationException($"No operation registered for {data.GetType().Name}");
    }

    public TOperation Create<TOperation>(OperationData data) where TOperation : GameOperation {
        return _container.Instantiate<TOperation>(new object[] { data });
    }
}

public class CardPlayModule : MonoBehaviour {
    [Header("Dependencies")]
    [SerializeField] private OperationManager _operationManager;

    [Header("Settings")]
    [SerializeField] private bool _useEarlyFinishMode = false; // Новий режим

    private CardPlayData _playData;
    private CancellationToken _currentCancellationToken;

    public event System.Action<CardPresenter, bool> OnCardPlayCompleted;
    public event System.Action<CardPresenter> OnCardPlayStarted;

    private CancellationTokenSource _internalTokenSource;
    private IOperationFactory _operationFactory;

    public void StartCardPlay(CardPresenter cardPresenter, CancellationToken externalToken = default) {
        if (IsPlaying() || cardPresenter == null) {
            OnCardPlayCompleted?.Invoke(cardPresenter, false);
            return;
        }

        _playData = new CardPlayData(cardPresenter);
        OnCardPlayStarted?.Invoke(cardPresenter);

        _internalTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        _currentCancellationToken = _internalTokenSource.Token;

        // Підписка на івенти в залежності від режиму
        _operationManager.OnOperationStatus += HandleOperationStatus;

        WaitAndSubmitNextOperation(_currentCancellationToken).Forget();
    }


    private async UniTask WaitAndSubmitNextOperation(CancellationToken cancellationToken) {
        try {
            await UniTask.WaitUntil(() => !_operationManager.IsRunning,
                cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested) {
                StopOperationSequence();
                return;
            }

            if (_playData?.HasNextOperation() == true) {
                var nextOperationData = _playData.GetNextOperation();
                GameOperation operation = _operationFactory.Create(nextOperationData);
                operation.Initiator = _playData.CardPresenter;

                if (!cancellationToken.IsCancellationRequested) {
                    _operationManager.Push(operation);
                }
            } else {
                StopOperationSequence();
            }
        } catch (System.OperationCanceledException) {
            StopOperationSequence();
        }
    }

    // Стара логіка (резервна)
    private void HandleOperationStatus(GameOperation operation, OperationStatus status) {
        if (!IsPlaying()) return;

        if (!operation.Initiator == _playData.CardPresenter) return;


        switch (status) {
            case OperationStatus.Start:
                if (_useEarlyFinishMode) {
                    OnCardPlayCompleted?.Invoke(_playData.CardPresenter, true);
                }
                break;
            case OperationStatus.Success:
                _playData.IsStarted = true;
                _playData.CompletedOperations++;
                WaitAndSubmitNextOperation(_currentCancellationToken).Forget();
                break;

            case OperationStatus.Cancelled:
            case OperationStatus.Failed:
                if (_useEarlyFinishMode) {
                    OnCardPlayCompleted?.Invoke(_playData.CardPresenter, false);
                    StopOperationSequence();
                } else {
                    if (!_playData.IsStarted) {
                        // Перша операція не вдалася - карта не зіграна
                        StopOperationSequence();
                    } else {
                        WaitAndSubmitNextOperation(_currentCancellationToken).Forget();
                    }
                }
                

                break;
        }
    }


    private void StopOperationSequence() {
        if (!IsPlaying()) return;

        _operationManager.OnOperationStatus -= HandleOperationStatus;

        _internalTokenSource?.Cancel();
        _internalTokenSource?.Dispose();
        _internalTokenSource = null;

        if (!_useEarlyFinishMode) {
            OnCardPlayCompleted?.Invoke(_playData.CardPresenter, _playData.IsStarted);
        }

        GameLogger.Log($"Card play Completed operations: {_playData.CompletedOperations} / {_playData.Operations.Count}", LogLevel.Debug, LogCategory.CardModule);
        _playData = null;
    }

    public bool IsPlaying() {
        return _playData != null;
    }

    public CardPlayData GetCurrentPlayData() {
        return _playData;
    }

    // Метод для зміни режиму під час виконання
    public void SetEarlyFinishMode(bool enabled) {
        _useEarlyFinishMode = enabled;
    }
}

public class CardPlayData {
    public CardPresenter CardPresenter;
    public bool IsStarted = false;
    public int CurrentOperationIndex = 0;
    public int CompletedOperations = 0;
    public List<OperationData> Operations;

    public CardPlayData(CardPresenter presenter) {
        CardPresenter = presenter;
        Operations = presenter.GetOperationDatas();
    }

    public bool HasNextOperation() => CurrentOperationIndex < Operations.Count;

    public OperationData GetNextOperation() {
        if (HasNextOperation() && CardPresenter != null && Operations != null) {
            return Operations[CurrentOperationIndex++];
        }
        return null;
    }

    public bool IsLastOperation(OperationData operation) {
        return Operations?.LastOrDefault() == operation;
    }
}

public struct CardPlayedEvent : IEvent {
    public Card PlayedCard { get; }
    public BoardPlayer PlayedBy { get; }
    public bool WasSuccessful { get; }

    public CardPlayedEvent(Card playedCard, BoardPlayer playedBy, bool wasSuccessful) {
        PlayedCard = playedCard;
        PlayedBy = playedBy;
        WasSuccessful = wasSuccessful;
    }
}