using System;
using UnityEngine;

public abstract class InteractableView : UnitView {
    public event Action<UnitView, bool> OnHoverChanged;
    public event Action<UnitView> OnClicked;
    private InteractiveUnitInputProviderBase _inputProvider;

    protected virtual void Awake() {
        if (_inputProvider == null) {
            Initialize();
        }
    }

    public void Initialize(InteractiveUnitInputProviderBase inputProvider = null) {
        _inputProvider = inputProvider ?? FindDefaultInputProvider();

        if (_inputProvider != null) {
            _inputProvider.OnClicked += HandleClicked;
            _inputProvider.OnCursorEnter += HandleCursorEnter;
            _inputProvider.OnCursorExit += HandleCursorExit;
        }
    }
    private InteractiveUnitInputProviderBase FindDefaultInputProvider() {
        var provider = GetComponentInChildren<InteractiveUnitInputProviderBase>();
        if (provider == null) {
            Debug.LogError($"No {nameof(InteractiveUnitInputProviderBase)} found for {nameof(UnitView)} on {gameObject.name}");
        }
        return provider;
    }

    private void HandleClicked() => OnClicked?.Invoke(this);
    private void HandleCursorEnter() => OnHoverChanged?.Invoke(this, true);
    private void HandleCursorExit() => OnHoverChanged?.Invoke(this, false);

    public void SetInteractable(bool interactable) {
        _inputProvider?.SetInteractable(interactable);
    }

    private void OnDestroy() {
        if (_inputProvider != null) {
            _inputProvider.OnClicked -= HandleClicked;
            _inputProvider.OnCursorEnter -= HandleCursorEnter;
            _inputProvider.OnCursorExit -= HandleCursorExit;
        }
    }
}

