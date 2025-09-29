using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class PlayerSelectorService : ITargetSelectionService, IDisposable {
    public event Action<TargetSelectionRequest> OnSelectionStarted;
    public event Action<TargetSelectionRequest, UnitModel> OnSelectionCompleted;
    public event Action<TargetSelectionRequest> OnSelectionCancelled;

    private readonly IUnitRegistry _unitRegistry;
    private readonly SelectorView _selectionView;
    private TargetSelectionRequest _currentRequest;
    private TaskCompletionSource<UnitModel> _currentTask;
    private CancellationTokenRegistration _cancellationRegistration;

    public PlayerSelectorService(SelectorView selectionView, IUnitRegistry unitRegistry) {
        _selectionView = selectionView;
        _unitRegistry = unitRegistry;
        _selectionView.OnTargetsSelected += OnTargetSelected;
    }

    public async UniTask<UnitModel> SelectTargetAsync(TargetSelectionRequest request, CancellationToken cancellationToken) {
        // Завершуємо попередній запит якщо активний
        CancelCurrentSelection();

        _currentTask = new TaskCompletionSource<UnitModel>();
        _cancellationRegistration = cancellationToken.Register(CancelCurrentSelection);

        try {
            StartTargeting(request);
            OnSelectionStarted?.Invoke(request);

            return await _currentTask.Task;
        } catch (OperationCanceledException) {
            OnSelectionCancelled?.Invoke(request);
            throw;
        } finally {
            StopTargeting();
            _cancellationRegistration.Dispose();
        }
    }

    private void StartTargeting(TargetSelectionRequest request) {
        _currentRequest = request;

        var visualization = ChooseVisualization(request);
        _selectionView.StartTargeting(visualization);
        _selectionView.HideErrorMessage();
        _selectionView.ShowMessage(request.Target.GetInstruction());
    }

    private ITargetingVisualization ChooseVisualization(TargetSelectionRequest request) {
        if (request.Source is Card card && IsZoneTarget(request.Target)) {
            var cardPresenter = _unitRegistry.GetPresenter<CardPresenter>(card);
            if (cardPresenter != null) {
                return _selectionView.CreateCardMovementTargeting(cardPresenter);
            }
        }

        return _selectionView.CreateArrowTargeting(request);
    }

    private bool IsZoneTarget(TargetInfo target) {
        ITargetRequirement requirement = target.Requirement;
        bool isZoneReq = requirement is TargetRequirement<Zone>;
        return isZoneReq;
    }

    private void OnTargetSelected(List<UnitView> views) {
        var models = views
            .Select(view => _unitRegistry.GetPresenterByView(view)?.Model)
            .Where(model => model != null)
            .ToList();

        var context = new ValidationContext(_currentRequest.Source.OwnerId);

        var optionalTarget = models.FirstOrDefault(model =>
            _currentRequest.Target.IsValid(model, context));

        if (optionalTarget == null) {
            var firstInvalid = models.FirstOrDefault();
            var result = _currentRequest.Target.IsValid(firstInvalid, context);
            _selectionView.ShowTemporaryError(result.ErrorMessage ?? "No valid targets found").Forget();
        }

        CompleteSelection(optionalTarget);
    }


    private void CompleteSelection(UnitModel target) {
        if (_currentTask?.Task.IsCompleted == false) {
            _currentTask.TrySetResult(target);
            OnSelectionCompleted?.Invoke(_currentRequest, target);
        }
    }

    public void CancelCurrentSelection() {
        if (_currentTask?.Task.IsCompleted == false) {
            _currentTask.TrySetCanceled();
        }
    }

    private void StopTargeting() {
        _selectionView.StopTargeting();
    }

    public void Dispose() {
        _selectionView.OnTargetsSelected -= OnTargetSelected;
        _cancellationRegistration.Dispose();
        StopTargeting();
    }

    
}

public class TargetSelectionRequest {
    public TargetInfo Target { get; }
    public UnitModel Source { get; } // Карта, істота, гравець, ніхто

    public TargetSelectionRequest(TargetInfo target, UnitModel initiator = null) {
        Target = target;
        Source = initiator;
    }
}
