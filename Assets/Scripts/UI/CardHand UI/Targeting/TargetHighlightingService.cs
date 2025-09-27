using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TargetHighlightingService : MonoBehaviour {
    [Header("Highlight Settings")]

    [Inject] private IUnitRegistry unitRegistry;
    [SerializeField] private OldClumsySelector targetSelector;

    private HashSet<UnitPresenter> highlightedUnits = new HashSet<UnitPresenter>();
    private float lastUpdateTime;

    private void OnEnable() {
        targetSelector.OnSelectionStarted += OnTargetSelectionStarted;
        targetSelector.OnSelectionEnded += ClearAllHighlights;
    }

    private void OnDisable() {
        targetSelector.OnSelectionStarted -= OnTargetSelectionStarted;
        ClearAllHighlights();
    }

    private void OnTargetSelectionStarted(TargetSelectionRequest request) {
        foreach (var model in unitRegistry.GetAllModels<UnitModel>()) {
            if (IsValidTarget(model, request)) {
                HighlightUnit(model, true);
            }
        }
    }

    private bool IsValidTarget(UnitModel unit, TargetSelectionRequest request) {
        var player = unit.GetPlayer();
        if (player == null) {
            //Debug.LogWarning($"player is null for {unit}");
        }


        return request.Target.IsValid(unit, new ValidationContext(request.Source.GetPlayer()));
    }


    public void HighlightUnit(UnitModel unit, bool isEnabled) {
        if (unit == null) return;

        UnitPresenter presenter = unitRegistry.GetPresenterByModel(unit);
        highlightedUnits.Add(presenter);

        // Викликаємо метод Highlight у юніта (якщо потрібно)
        presenter.Highlight(isEnabled);
    }

    private void ClearAllHighlights() {
        foreach (var unit in highlightedUnits) {
            if (unit != null) {
                unit.Highlight(false);
            }
        }

        highlightedUnits.Clear();
    }

    public void ForceClearHighlights() {
        ClearAllHighlights();
    }
}