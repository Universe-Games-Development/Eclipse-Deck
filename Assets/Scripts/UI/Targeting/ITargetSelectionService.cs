using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface ITargetSelectionService {
    public event Action<TargetSelectionRequest> OnSelectionStarted;
    public event Action<TargetSelectionRequest, UnitModel> OnSelectionCompleted;
    public event Action<TargetSelectionRequest> OnSelectionCancelled;
    void CancelCurrentSelection();
    UniTask<UnitModel> SelectTargetAsync(TargetSelectionRequest request, CancellationToken cancellationToken);
}

public abstract class BaseTargetSelector : ITargetSelectionService {
    public event Action<TargetSelectionRequest> OnSelectionStarted;
    public event Action<TargetSelectionRequest, UnitModel> OnSelectionCompleted;
    public event Action<TargetSelectionRequest> OnSelectionCancelled;

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

        OnSelectionStarted?.Invoke(selectionRequest);

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
            OnSelectionCompleted?.Invoke(_currentRequest, target);
        }
    }

    public void CancelCurrentSelection() {
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

public class RandomTargetSelector : BaseTargetSelector {
    protected override UniTask StartSelectionAsync(TargetSelectionRequest request, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
}