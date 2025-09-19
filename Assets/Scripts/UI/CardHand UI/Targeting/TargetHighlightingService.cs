using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TargetHighlightingService : MonoBehaviour {
    [Header("Highlight Settings")]

    [Inject] private IUnitPresenterRegistry unitRegistry;
    [SerializeField] private HumanTargetSelector targetSelector;

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
        foreach (var presenter in unitRegistry.GetAllPresenters()) {
            if (IsValidTarget(presenter.GetModel(), request)) {
                HighlightUnit(presenter, true);
            }
        }
    }

    private bool IsValidTarget(UnitModel unit, TargetSelectionRequest request) {
        var player = unit.GetPlayer();
        if (player == null) {
            //Debug.LogWarning($"player is null for {unit}");
        }

        return request.Target.IsValid(unit, player);
    }


    public void HighlightUnit(UnitPresenter unit, bool isEnabled) {
        if (unit == null) return;

        highlightedUnits.Add(unit);

        // Викликаємо метод Highlight у юніта (якщо потрібно)
        unit.Highlight(isEnabled);
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