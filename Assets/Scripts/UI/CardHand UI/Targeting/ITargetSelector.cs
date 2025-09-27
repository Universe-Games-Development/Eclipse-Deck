using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface ITargetSelector {
    Action<TargetSelectionRequest> OnSelectionRequested { get; set; }
    Action<TargetSelectionRequest, UnitModel> OnSelectionFinished { get; set; }
    Action<TargetSelectionRequest> OnSelectionCancelled { get; set; } // Новий івент

    void CancelSelection();
    void ConfirmSelection(UnitModel target);
    UniTask<UnitModel> SelectTargetAsync(TargetSelectionRequest selectionRequst, CancellationToken cancellationToken);
}

public abstract class BaseTargetSelector : ITargetSelector {
    public Action<TargetSelectionRequest> OnSelectionRequested { get; set; }
    public Action<TargetSelectionRequest, UnitModel> OnSelectionFinished { get; set; }
    public Action<TargetSelectionRequest> OnSelectionCancelled { get; set; } // Нова реалізація

    private TaskCompletionSource<UnitModel> _currentSelectionTask;
    private CancellationTokenSource _currentCancellation;
    private TargetSelectionRequest _currentRequest;
    private bool _isCancelledBySelector;

    public async UniTask<UnitModel> SelectTargetAsync(
        TargetSelectionRequest selectionRequest,
        CancellationToken cancellationToken = default) {

        // Скасовуємо попередній запит, якщо він активний
        if (_currentSelectionTask != null && !_currentSelectionTask.Task.IsCompleted) {
            _isCancelledBySelector = true;
            _currentCancellation?.Cancel();
            await UniTask.Yield(); // Даємо час на завершення
        }

        _currentCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _currentRequest = selectionRequest;
        _currentSelectionTask = new TaskCompletionSource<UnitModel>();
        _isCancelledBySelector = false;

        OnSelectionRequested?.Invoke(selectionRequest);

        // Обробка скасування
        _currentCancellation.Token.Register(() => {
            _currentSelectionTask?.TrySetCanceled(_currentCancellation.Token);

            // Викликаємо івент тільки якщо скасування не було ініційовано самим селектором
            if (!_isCancelledBySelector) {
                OnSelectionCancelled?.Invoke(_currentRequest);
            }

            Cleanup();
        });

        try {
            // Запускаємо конкретну логіку вибору
            await StartSelectionAsync(selectionRequest, _currentCancellation.Token);
            return await _currentSelectionTask.Task;
        } finally {
            Cleanup();
        }
    }

    protected abstract UniTask StartSelectionAsync(TargetSelectionRequest request, CancellationToken cancellationToken);

    public void ConfirmSelection(UnitModel target) {
        if (_currentSelectionTask != null && !_currentSelectionTask.Task.IsCompleted) {
            _currentSelectionTask.TrySetResult(target);
            OnSelectionFinished?.Invoke(_currentRequest, target);
        }
    }

    public void CancelSelection() {
        _isCancelledBySelector = true;
        _currentCancellation?.Cancel();
        OnSelectionCancelled?.Invoke(_currentRequest); // Сповіщаємо про скасування
    }

    private void Cleanup() {
        _currentSelectionTask = null;
        _currentRequest = null;
        _currentCancellation?.Dispose();
        _currentCancellation = null;
        _isCancelledBySelector = false;
    }
}

public class HumanTargetSelector : BaseTargetSelector {

    protected override async UniTask StartSelectionAsync(TargetSelectionRequest request, CancellationToken cancellationToken) {
        await UniTask.CompletedTask;
    }
}