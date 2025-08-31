using System;
using TMPro;
using UnityEngine;
using Zenject;

public class ArrowTargeting : MonoBehaviour, ITargetingVisualization {
    [Header("Arrow Components")]
    [SerializeField] private LineRenderer arrowLine;
    [SerializeField] private Transform arrowHead;
    [SerializeField] private Material validTargetMaterial;
    [SerializeField] private Material invalidTargetMaterial;
    [SerializeField] private Material noTargetMaterial;

    [Header("Dependencies")]
    [SerializeField] private LayerMask boardMask;
    [SerializeField] private BoardInputManager boardInputManager;

    [Inject] private InputManager inputManager;

    // State
    private bool isActive;
    private Vector3 startPosition;
    private Func<Vector3> targetPositionProvider;

    private TargetSelectionRequest currentRequest;

    private GameObject lastHoveredObject;


    public void StartTargeting(Func<Vector3> targetPositionProvider, TargetSelectionRequest targetSelectionRequest) {
        this.targetPositionProvider = targetPositionProvider;
        currentRequest = targetSelectionRequest;
        isActive = true;

        SetupArrowVisuals(true);
    }

    public void StopTargeting() {
        isActive = false;
        SetupArrowVisuals(false);
        ResetArrowColor();
    }

    public void UpdateTargeting() {
        if (!isActive) return;

        Vector3 targetPosition = targetPositionProvider();
        UpdateArrowPosition(startPosition, targetPosition);
        UpdateArrowColor(targetPosition);
    }

    private void Update() {
        if (isActive) {
            UpdateTargeting();
        }
    }

    private void UpdateArrowPosition(Vector3 start, Vector3 end) {
        // Оновлюємо позицію лінії стрілки
        arrowLine.positionCount = 2;
        arrowLine.SetPosition(0, start);
        arrowLine.SetPosition(1, end);

        // Позиціонуємо голівку стрілки
        arrowHead.position = end;
        arrowHead.LookAt(start);
    }

    private void UpdateArrowColor(Vector3 targetPosition) {
        GameObject hoveredObject = GetObjectUnderPosition(targetPosition);

        if (hoveredObject == lastHoveredObject) return;
        lastHoveredObject = hoveredObject;

        Material materialToUse = DetermineArrowMaterial(hoveredObject);
        ApplyArrowMaterial(materialToUse);
    }

    private GameObject GetObjectUnderPosition(Vector3 position) {
        if (boardInputManager.TryGetCursorObject(boardMask, out GameObject hitObject)) {
            return hitObject;
        }
        return null;
    }

    private Material DetermineArrowMaterial(GameObject hoveredObject) {
        if (hoveredObject == null) {
            return noTargetMaterial; // Синій - немає об'єкта
        }

        UnitModel gameUnit = GetGameUnitFromObject(hoveredObject);
        if (gameUnit == null) {
            return noTargetMaterial; // Синій - не ігровий об'єкт
        }

        ValidationResult validation = currentRequest.Requirement.IsValid(gameUnit, currentRequest.Initiator.GetPlayer());
        return validation.IsValid ? validTargetMaterial : invalidTargetMaterial; // Зелений/Червоний
    }

    private UnitModel GetGameUnitFromObject(GameObject obj) {
        if (obj.TryGetComponent<UnitPresenter>(out var provider)) {
            return provider.GetInfo();
        }
        return null;
    }

    private void ApplyArrowMaterial(Material material) {
        arrowLine.material = material;
        if (arrowHead.TryGetComponent<Renderer>(out var renderer)) {
            renderer.material = material;
        }
    }

    private void SetupArrowVisuals(bool active) {
        arrowLine.enabled = active;
        arrowHead.gameObject.SetActive(active);
    }

    private void ResetArrowColor() {
        ApplyArrowMaterial(noTargetMaterial);
        lastHoveredObject = null;
    }

}

