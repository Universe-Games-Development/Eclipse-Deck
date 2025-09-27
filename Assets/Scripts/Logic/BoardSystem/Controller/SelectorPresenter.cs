using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectorPresenter : IDisposable {
    public event Action<TargetSelectionRequest> OnSelectionStarted;
    public event Action<TargetSelectionRequest, UnitModel> OnSelectionCompleted;
    public event Action<TargetSelectionRequest> OnSelectionCancelled;

    private readonly ITargetSelector _selector;
    private readonly IUnitRegistry _unitRegistry;
    private readonly SelectorView _selectionView;

    private TargetSelectionRequest _currentRequest;

    public SelectorPresenter(ITargetSelector selector, IUnitRegistry unitRegistry, SelectorView selectionView) {
        _selector = selector;
        _unitRegistry = unitRegistry;
        _selectionView = selectionView;

        _selector.OnSelectionRequested += HandleSelectionRequested;
        _selector.OnSelectionFinished += HandleSelectionFinished;
        _selector.OnSelectionCancelled += HandleSelectionCancelled;
    }
    
    private void HandleSelectionRequested(TargetSelectionRequest request) {
        _currentRequest = request;

        ITargetingVisualization vizualization = CreateVizualization(_currentRequest);
        _selectionView.StartTargeting(vizualization);

        _selectionView.ShowTargetingMessage(request.Target.GetInstruction());
        _selectionView.OnTargetSelected += ProceedTargets;
        OnSelectionStarted?.Invoke(request);
    }

    // Візуалізація залежно від джерела
    private ITargetingVisualization CreateVizualization(TargetSelectionRequest request) {
        if (request.Source is Card card) {
            CardPresenter cardPresenter = _unitRegistry.GetPresenter<CardPresenter>(card);

            if (cardPresenter != null && IsZoneRequirement(request.Target)) {
                return CreateCardMovementTargeting(cardPresenter);
            }
            
        }

        return CreateArrowTargeting(request);
    }

    private ITargetingVisualization CreateCardMovementTargeting(CardPresenter presenter) {
        CardMovementTargeting cardMovementTargeting = _selectionView.cardTargeting;
        cardMovementTargeting.Initialize(presenter);
        return cardMovementTargeting;
    }

    private ITargetingVisualization CreateArrowTargeting(TargetSelectionRequest request) {
        ArrowTargeting arrowTargeting = _selectionView.arrowTargeting;
        arrowTargeting.Initialize(request);
        return arrowTargeting;
    }

    private bool IsZoneRequirement(TypedTargetBase request) {
        return request.TargetType == typeof(Zone);
    }


    private void ProceedTargets(List<UnitView> views) {
        List<UnitModel> models = views.Select(view => _unitRegistry.GetPresenterByView(view))
            .Where(presenter => presenter != null)
            .Select(presenter => presenter.Model)
            .ToList();

        TypedTargetBase target = _currentRequest.Target;
        Opponent opponent = _currentRequest.Source.GetPlayer();

        UnitModel satisfyModel = models.Where(model => target.IsValid(model, new ValidationContext(opponent))).FirstOrDefault();

        _selector.ConfirmSelection(satisfyModel);
        OnSelectionCompleted?.Invoke(_currentRequest, satisfyModel); // satisfyModel could be null its okay means failed choice
    }

    private void HandleSelectionFinished(TargetSelectionRequest request, UnitModel target) {
        StopTargeting();
        _selectionView.HideTargetingMessage();
        _selectionView.OnTargetSelected -= ProceedTargets;
        OnSelectionCompleted?.Invoke(request, target);
    }

    private void HandleSelectionCancelled(TargetSelectionRequest request) {
        StopTargeting();
        _selectionView.HideTargetingMessage();
        _selector.CancelSelection();
        OnSelectionCancelled?.Invoke(request);
    }

    private void StopTargeting() {
        _selectionView.StopTargeting();
        _currentRequest = null;
    }

    public void Dispose() {
        _selector.OnSelectionRequested -= HandleSelectionRequested;
        _selector.OnSelectionFinished -= HandleSelectionFinished;
        _selector.OnSelectionCancelled -= HandleSelectionCancelled;
        StopTargeting();
    }
}

public interface ITargetingVisualization {
    void StartTargeting();
    void UpdateTargeting(Vector3 cursorPosition);
    void StopTargeting();
}

public class TargetSelectionRequest {
    public UnitModel Source { get; } // Карта, істота, гравець
    public TypedTargetBase Target { get; }

    public TargetSelectionRequest(UnitModel initiator, TypedTargetBase target) {
        Source = initiator;
        Target = target;
    }
}
