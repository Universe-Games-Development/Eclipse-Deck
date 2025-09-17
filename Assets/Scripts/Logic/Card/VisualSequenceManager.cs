using Cysharp.Threading.Tasks;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

public class VisualSequenceManager : IVisualManager {

    PriorityQueue<Priority, VisualTask> _visualsQueue = new();
    private readonly ReaderWriterLockSlim _queueLock = new();
    public int QueueCount => _visualsQueue?.Count ?? 0;
    public bool IsRunning => _isRunning;
    private bool _isRunning;
    public VisualTask CurrentTask => _currentTask;
    private VisualTask _currentTask;

    private CancellationTokenSource _globalCancellationSource = new();

    public event Action OnQueueEmpty;

    public void Push(VisualTask task, Priority priority = Priority.Normal) {
        if (task == null) {
            GameLogger.LogError("Cannot push a null task", LogCategory.Visualmanager);
            return;
        }

        using (new WriteLock(_queueLock)) {
            _visualsQueue.Enqueue(priority, task);
        }

        GameLogger.LogDebug($"Operation '{task}' pushed with priority {priority}. Queue size: {QueueCount}",
            LogCategory.Visualmanager);

        TryStartProcessing().Forget();
    }

    private async UniTaskVoid TryStartProcessing() {
        if (_isRunning) {
            //GameLogger.LogDebug("Processing already running, skipping start", LogCategory.Visualmanager);
            return;
        }

        _isRunning = true;
        GameLogger.LogInfo("Starting task processing...", LogCategory.Visualmanager);

        int processedCount = 0;

        try {
            while (HasOperationsInQueue()) {
                if (!TryDequeueTask(out _currentTask))
                    break;

                GameLogger.LogInfo($"Processing task [{processedCount + 1}]: {_currentTask}",
                    LogCategory.OperationManager);

                try {
                    await ProcessOperationAsync(_currentTask, _globalCancellationSource.Token);
                    processedCount++;
                } catch (OperationCanceledException) {
                    GameLogger.LogInfo($"Operation cancelled: {_currentTask}", LogCategory.Visualmanager);
                } finally {
                    _currentTask = null;
                }
            }

            GameLogger.LogInfo($"Queue processing completed. Processed {processedCount} tasks", LogCategory.Visualmanager);
            OnQueueEmpty?.Invoke();
        } finally {
            _isRunning = false;
            _currentTask = null;
            GameLogger.LogDebug("Operation processing finished", LogCategory.Visualmanager);
        }
    }

    private bool HasOperationsInQueue() {
        using (new ReadLock(_queueLock)) {
            return !_visualsQueue.IsEmpty();
        }
    }

    private bool TryDequeueTask(out VisualTask task) {
        using (new WriteLock(_queueLock)) {
            var result = _visualsQueue.TryDequeue(out task);
            if (result) {
                GameLogger.LogDebug($"Dequeued task: {task}. Remaining: {_visualsQueue.Count}",
                    LogCategory.OperationManager);
            }
            return result;
        }
    }
    private async UniTask ProcessOperationAsync(VisualTask task, CancellationToken cancellationToken) {
        OperationStatus status = OperationStatus.Failed;
        var startTime = DateTime.UtcNow;

        try {
            if (!ValidateTask(task)) {
                status = OperationStatus.Failed;
                GameLogger.LogWarning($"Task validation failed: {task}", LogCategory.Visualmanager);
                return;
            }

            GameLogger.LogInfo($"Beginning task: {task}", LogCategory.Visualmanager);

            await task.Execute();

        } catch (OperationCanceledException) {
            status = OperationStatus.Cancelled;
            GameLogger.LogInfo($"Operation {task} was cancelled", LogCategory.Visualmanager);
            throw;
        } finally {
            var duration = DateTime.UtcNow - startTime;
            GameLogger.LogInfo($"{task} finished with status: {status} (Duration: {duration.TotalMilliseconds:F1}ms)",
                LogCategory.Visualmanager);
        }
    }

    private bool ValidateTask(VisualTask task) {
        return task != null;
    }
}