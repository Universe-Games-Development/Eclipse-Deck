using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TargetHighlightingService : MonoBehaviour {
    [Header("Highlight Settings")]

    [Inject] private IUnitRegistry unitRegistry;
    [Inject] ITargetValidator targetValidator;

    private HashSet<UnitView> highlightedUnits = new();
    private float lastUpdateTime;

    private void OnEnable() {
    }

    private void OnDisable() {
        ClearAllHighlights();
    }

    private void OnTargetSelectionStarted(TargetSelectionRequest request) {
        ITargetRequirement targetRequirement = request.RequirementData.BuildRuntime();

        List<UnitModel> unitModels = targetValidator.GetValidTargetsFor(targetRequirement, request.ValidationContext.InitiatorId);

        foreach (var model in unitModels) {
            HighlightUnit(model, true);
        }
    }

    public void HighlightUnit(UnitModel unit, bool isEnabled) {
        if (unit == null) return;

        UnitView view = unitRegistry.GetViewByModel(unit);
        highlightedUnits.Add(view);

        // Викликаємо метод Highlight у юніта (якщо потрібно)
        view.Highlight(isEnabled);
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