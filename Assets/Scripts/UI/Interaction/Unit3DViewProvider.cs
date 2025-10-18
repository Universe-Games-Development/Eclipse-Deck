using System;
using UnityEngine;

public interface IUnitProvider {
    UnitView UnitView { get; }

    void SetInteractable(bool interactable);
}

public abstract class UnitInputProviderBase : MonoBehaviour, IUnitProvider {
    [SerializeField] protected UnitView _unitView;

    public UnitView UnitView => _unitView;

    protected virtual void Awake() {
        EnsureUnitViewReference();
    }

    protected virtual void EnsureUnitViewReference() {
        if (_unitView == null) {
            _unitView = GetComponentInParent<UnitView>();
            if (_unitView == null) {
                Debug.LogError($"{GetType().Name} на {gameObject.name} не знайшов UnitView.");
                enabled = false; // Вимикаємо компонент
            }
        }
    }

    protected virtual void OnValidate() {
        if (_unitView == null) {
            _unitView = GetComponentInParent<UnitView>();
        }
    }

    // В базовому провайдері немає інтерактивності
    public virtual void SetInteractable(bool interactable) {
        // no-op
    }
}

public abstract class InteractiveUnitInputProviderBase : UnitInputProviderBase {
    private event Action _onClicked;
    private event Action _onCursorEnter;
    private event Action _onCursorExit;

    [SerializeField] private bool _isInteractable = true;
    [SerializeField] private bool _enableClick = true;
    [SerializeField] private bool _enableHover = true;
    protected bool _hasCollider;

    public event Action OnClicked {
        add => _onClicked += value;
        remove => _onClicked -= value;
    }

    public event Action OnCursorEnter {
        add => _onCursorEnter += value;
        remove => _onCursorEnter -= value;
    }

    public event Action OnCursorExit {
        add => _onCursorExit += value;
        remove => _onCursorExit -= value;
    }

    protected override void Awake() {
        base.Awake();
        _hasCollider = InitializeCollider();
        if (!_hasCollider) {
            Debug.LogWarning($"No collider found on {gameObject.name}. Input will not work.");
        }
    }

    public override void SetInteractable(bool interactable) {
        if (_isInteractable == interactable) return;

        _isInteractable = interactable;

        if (_hasCollider) {
            UpdateColliderState(interactable);
        }

        OnInteractableStateChanged(interactable);
    }

    protected abstract void UpdateColliderState(bool enabled);
    protected abstract bool InitializeCollider();

    protected virtual void OnInteractableStateChanged(bool isInteractable) {
        // Для похідних класів
    }

    protected void RaiseClicked() {
        if (_isInteractable && _enableClick) {
            _onClicked?.Invoke();
        }
    }

    protected void RaiseCursorEnter() {
        if (_isInteractable && _enableHover) {
            _onCursorEnter?.Invoke();
        }
    }

    protected void RaiseCursorExit() {
        if (_isInteractable && _enableHover) {
            _onCursorExit?.Invoke();
        }
    }

    public void SetClickEnabled(bool enabled) => _enableClick = enabled;
    public void SetHoverEnabled(bool enabled) => _enableHover = enabled;
}

