using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerSelectorService : ITargetSelectionService, IDisposable {
    public event Action<TargetSelectionRequest> OnSelectionStarted;
    public event Action<TargetSelectionRequest, UnitModel> OnSelectionCompleted;
    public event Action<TargetSelectionRequest> OnSelectionCancelled;

    private TargetSelectionRequest _currentRequest;
    private TaskCompletionSource<UnitModel> _currentTask;
    private CancellationTokenRegistration _cancellationRegistration;

    private readonly IEventBus<IEvent> eventBus;
    private readonly IUnitRegistry _unitRegistry;
    private readonly SelectorView _selectionView;

    public PlayerSelectorService(SelectorView selectionView, IUnitRegistry unitRegistry, IEventBus<IEvent> eventBus) {
        _selectionView = selectionView;
        _unitRegistry = unitRegistry;
        this.eventBus = eventBus;

        _selectionView.OnTargetsSelected += OnTargetSelected;
        eventBus.SubscribeTo<HoverUnitEvent>(HandleUnitHover);
    }

    private void HandleUnitHover(ref HoverUnitEvent eventData) {
        if (_currentRequest == null) return;

        if (eventData.IsHovered) {
            ValidationContext validationContext = new(_currentRequest.Source.OwnerId);
            bool isValidToRequest = _currentRequest.Target.Requirement.IsValid(eventData.UnitPresenter.Model, validationContext);
            TargetValidationState state = isValidToRequest ? TargetValidationState.Valid : TargetValidationState.WrongTarget;
            _selectionView.UpdateHoverStatus(state);
        } else {
            _selectionView.UpdateHoverStatus(TargetValidationState.None);
        }
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

    private void OnTargetSelected(GameObject[] objects) {
        List<UnitView> views = new();

        foreach (var hitObj in objects) {
            if (hitObj.TryGetComponent(out IUnitProvider provider)) {
                var view = provider.UnitView;
                if (view != null) {
                    views.Add(view);
                }
            } else if (hitObj.TryGetComponent(out UnitView directView)) {
                views.Add(directView);
            }
        }

        List<UnitModel> models = new();
        for (int i = 0; i < views.Count; i++) {
            UnitModel unitModel = _unitRegistry.GetModelByView(views[i]);
            if (unitModel != null)
            models.Add(unitModel);


        }

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
        _currentRequest = null;
    }

    public void Dispose() {
        eventBus.UnsubscribeFrom<HoverUnitEvent>(HandleUnitHover);
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


public enum TargetValidationState {
    None,
    Valid,
    WrongTarget,
}