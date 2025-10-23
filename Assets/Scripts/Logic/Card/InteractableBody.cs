using System;
using UnityEngine;

public class InteractableBody : MonoBehaviour {
    public event Action<bool> OnHoverChanged;
    public event Action OnClicked;

    private InteractiveUnitInputProviderBase _inputProvider;
    private bool _isInitialized;

    protected virtual void Awake() {
        Initialize();
    }

    public void Initialize(InteractiveUnitInputProviderBase inputProvider = null) {
        if (_isInitialized) return;

        _inputProvider = inputProvider ?? GetComponentInChildren<InteractiveUnitInputProviderBase>();

        if (_inputProvider == null) {
            Debug.LogError($"No input provider found on {gameObject.name}", this);
            return;
        }

        SubscribeToInputProvider();
        _isInitialized = true;
    }

    private void SubscribeToInputProvider() {
        _inputProvider.OnClicked += () => OnClicked?.Invoke();
        _inputProvider.OnCursorEnter += () => OnHoverChanged?.Invoke(true);
        _inputProvider.OnCursorExit += () => OnHoverChanged?.Invoke(false);
    }

    public void SetInteractable(bool interactable) {
        _inputProvider?.SetInteractable(interactable);
    }

    protected virtual void OnDestroy() {
        if (_inputProvider != null) {
            _inputProvider.OnClicked -= () => OnClicked?.Invoke();
            _inputProvider.OnCursorEnter -= () => OnHoverChanged?.Invoke(true);
            _inputProvider.OnCursorExit -= () => OnHoverChanged?.Invoke(false);
        }
    }
}