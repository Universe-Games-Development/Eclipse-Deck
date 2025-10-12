using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TargetHighlightingService : MonoBehaviour {
    [Header("Highlight Settings")]

    [Inject] private IUnitRegistry unitRegistry;

    private HashSet<UnitView> highlightedUnits = new();
    private float lastUpdateTime;

    private void OnEnable() {
    }

    private void OnDisable() {
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
        var player = unit.OwnerId;
        if (player == null) {
            //Debug.LogWarning($"player is null for {unit}");
        }


        return request.Target.IsValid(unit, new ValidationContext(request.Source.OwnerId));
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