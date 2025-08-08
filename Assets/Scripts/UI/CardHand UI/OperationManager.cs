using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class OperationManager : MonoBehaviour {
    [SerializeField] private OperationTargetsFiller operationFiller;

    // Изменено на Queue для более логичного FIFO порядка
    // Но оставлена возможность приоритетных операций через отдельный стек
    private Queue<GameOperation> _operationQueue = new();
    private Stack<GameOperation> _priorityStack = new();

    private GameOperation _currentOperation;
    private bool _isRunning;
    private CancellationTokenSource _globalCancellationSource;

    public Action<GameOperation, OperationStatus> OnOperationStart;
    public Action<GameOperation, OperationStatus> OnOperationEnd;
    public Action OnQueueEmpty;

    // Свойства для мониторинга состояния
    public bool IsRunning => _isRunning;
    public GameOperation CurrentOperation => _currentOperation;
    public int QueueCount => _operationQueue.Count + _priorityStack.Count;

    private void Awake() {
        _globalCancellationSource = new CancellationTokenSource();
    }

    private void OnDestroy() {
        _globalCancellationSource?.Cancel();
        _globalCancellationSource?.Dispose();
    }

    /// <summary>
    /// Добавляет операцию в обычную очередь (FIFO)
    /// </summary>
    public void Push(GameOperation operation) {
        if (operation == null) {
            Debug.LogError("Cannot push a null operation.");
            return;
        }

        _operationQueue.Enqueue(operation);
        TryStartProcessing().Forget();
    }

    /// <summary>
    /// Добавляет операцию с приоритетом
    /// </summary>
    public void Push(GameOperation operation, Priority priority) {
        if (operation == null) {
            Debug.LogError("Cannot push a null operation.");
            return;
        }

        if (priority >= Priority.High) {
            _priorityStack.Push(operation);
        } else {
            _operationQueue.Enqueue(operation);
        }

        TryStartProcessing().Forget();
    }

    /// <summary>
    /// Добавляет операцию в начало очереди (высокий приоритет)
    /// </summary>
    public void PushImmediate(GameOperation operation) {
        if (operation == null) {
            Debug.LogError("Cannot push a null operation.");
            return;
        }

        _priorityStack.Push(operation);
        TryStartProcessing().Forget();
    }

    public void PushRange(List<GameOperation> operations) {
        if (operations == null || operations.Count == 0) return;

        foreach (var operation in operations) {
            if (operation != null) {
                _operationQueue.Enqueue(operation);
            }
        }

        TryStartProcessing().Forget();
    }

    /// <summary>
    /// Прерывает текущую операцию и очищает очередь
    /// </summary>
    public void CancelAll() {
        _globalCancellationSource?.Cancel();
        _globalCancellationSource = new CancellationTokenSource();

        _operationQueue.Clear();
        _priorityStack.Clear();

        if (_currentOperation != null) {
            OnOperationEnd?.Invoke(_currentOperation, OperationStatus.Canceled);
            _currentOperation = null;
        }

        _isRunning = false;
    }

    /// <summary>
    /// Прерывает только текущую операцию, продолжает обработку очереди
    /// </summary>
    public void CancelCurrent() {
        if (_currentOperation != null) {
            _globalCancellationSource?.Cancel();
            _globalCancellationSource = new CancellationTokenSource();
        }
    }

    /// <summary>
    /// Очищает очередь, но не прерывает текущую операцию
    /// </summary>
    public void ClearQueue() {
        _operationQueue.Clear();
        _priorityStack.Clear();
    }

    private async UniTaskVoid TryStartProcessing() {
        if (_isRunning) return;

        _isRunning = true;

        try {
            while (HasPendingOperations()) {
                _currentOperation = GetNextOperation();
                if (_currentOperation == null) break;

                OnOperationStart?.Invoke(_currentOperation, OperationStatus.Success);

                await ProcessOperationAsync(_currentOperation, _globalCancellationSource.Token);

                _currentOperation = null;
            }

            OnQueueEmpty?.Invoke();
        } finally {
            _isRunning = false;
            _currentOperation = null;
        }
    }

    private bool HasPendingOperations() {
        return _priorityStack.Count > 0 || _operationQueue.Count > 0;
    }

    private GameOperation GetNextOperation() {
        // Сначала проверяем приоритетные операции
        if (_priorityStack.Count > 0) {
            return _priorityStack.Pop();
        }

        // Затем обычную очередь
        if (_operationQueue.Count > 0) {
            return _operationQueue.Dequeue();
        }

        return null;
    }

    private async UniTask ProcessOperationAsync(GameOperation operation, CancellationToken cancellationToken) {
        OperationStatus status = OperationStatus.Failed;
        Debug.Log($"Beginning fill operation: {operation.ActionName}");
        try {
            // Проверяем, можно ли заполнить операцию
            if (!operationFiller.CanBeFilled(operation.NamedTargets)) {
                status = OperationStatus.Failed;
                Debug.LogWarning($"Operation {operation.ActionName} cannot be executed at this time");
                return;
            }

            // 1. Заполняем цели с поддержкой отмены
            Dictionary<string, GameUnit> targets = null;
           
            if (operation.NamedTargets.Count > 0) {
                targets = await operationFiller.FillTargetsAsync(operation.NamedTargets, cancellationToken);

                if (targets == null || cancellationToken.IsCancellationRequested) {
                    status = OperationStatus.Canceled;
                    return;
                }
            }

            operation.SetTargets(targets);

            // 2. Выполняем операцию
            bool success = await operation.ExecuteAsync(cancellationToken);

            status = success ? OperationStatus.Success : OperationStatus.Failed;
        } catch (OperationCanceledException) {
            status = OperationStatus.Canceled;
            
        } catch (Exception ex) {
            status = OperationStatus.Failed;
            Debug.LogError($"Operation {operation.ActionName} failed with exception: {ex}");
        } finally {
            Debug.Log($"Operation {operation.ActionName} finished with status: {status}");
            OnOperationEnd?.Invoke(operation, status);
        }
    }

    // Методы для отладки и мониторинга
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void LogQueueState() {
        Debug.Log($"Queue State - Running: {_isRunning}, Current: {_currentOperation?.ActionName ?? "None"}, " +
                  $"Priority Queue: {_priorityStack.Count}, Normal Queue: {_operationQueue.Count}");
    }

    public List<string> GetQueuedOperationNames() {
        var names = new List<string>();

        foreach (var op in _priorityStack)
            names.Add($"[PRIORITY] {op.ActionName}");

        foreach (var op in _operationQueue)
            names.Add(op.ActionName);

        return names;
    }

    public List<GameOperation> PopDefinedRange(List<GameOperation> operationsToRemove) {
        if (operationsToRemove == null || operationsToRemove.Count == 0)
            return new List<GameOperation>();

        var lookup = new HashSet<GameOperation>(operationsToRemove);
        var removedOperations = new List<GameOperation>();

        // Оптимізоване видалення зі стеку
        _priorityStack.RemoveAll(op =>
        {
            if (lookup.Contains(op)) {
                removedOperations.Add(op);
                return true;
            }
            return false;
        });

        // Оптимізоване видалення з черги
        _operationQueue = new Queue<GameOperation>(
            _operationQueue.Where(op => !lookup.Contains(op))
        );

        return removedOperations;
    }
}

public enum OperationStatus {
    Success,
    Canceled,
    Failed
}
public enum Priority {
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}


public abstract class GameOperation {
    public List<NamedTarget> NamedTargets = new();
    protected Dictionary<string, GameUnit> targets;

    public string ActionName = "defaultOperationName";

    public bool CanBeCancelled { get; internal set; }

    public abstract Task<bool> ExecuteAsync(CancellationToken cancellationToken);
    public void SetTargets(Dictionary<string, GameUnit> filledTargets) {
        targets = filledTargets;
    }
}

public class NamedTarget {
    public string Name;
    public ITargetRequirement Requirement;
    public GameUnit Unit;

    public NamedTarget(string name, ITargetRequirement zoneRequirement) {
        Name = name;
        Requirement = zoneRequirement;
    }
}