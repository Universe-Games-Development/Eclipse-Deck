using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public abstract class CardView : MonoBehaviour {
    // События
    public Action<CardView> OnCardClicked;
    public Action<CardView, bool> OnHoverChanged;

    public CardUIInfo CardInfo;
    [SerializeField] protected bool isInteractable = true;
    public string Id { get; set; }

    // Флаги состояния
    protected bool _isInitialized = false;
    protected bool _isSelected = false;
    protected bool _isHovered = false;

    #region Unity Lifecycle

    protected virtual void Awake() {
        ValidateComponents();
    }

    protected virtual void OnDestroy() {
        CleanupResources();
    }

    #endregion

    #region Initialization

    protected virtual void ValidateComponents() {
        // Переопределяется в наследниках для проверки специфичных компонентов
    }

    protected virtual void CleanupResources() {
        // Переопределяется в наследниках для очистки ресурсов
    }

    #endregion

    #region State Management

    public virtual void Reset() {
        isInteractable = false;
        _isSelected = false;
        _isHovered = false;

        // Сбрасываем визуальные состояния
        ResetVisualState();
    }

    protected virtual void ResetVisualState() {
        // Переопределяется в наследниках для сброса визуального состояния
    }

    public virtual void Select() {
        if (!_isInitialized || !isInteractable) return;

        _isSelected = true;
        OnSelectInternal();
    }

    public virtual void Deselect() {
        if (!_isInitialized) return;

        _isSelected = false;
        OnDeselectInternal();
    }

    protected virtual void OnSelectInternal() {
        // Переопределяется в наследниках для специфичной логики выделения
    }

    protected virtual void OnDeselectInternal() {
        // Переопределяется в наследниках для специфичной логики снятия выделения
    }

    public virtual void SetInteractable(bool value) {
        bool wasInteractable = isInteractable;
        isInteractable = value;

        // Если карта становится неинтерактивной и на ней было наведение - сбрасываем
        if (!value && _isHovered) {
            _isHovered = false;
            OnHoverChanged?.Invoke(this, false);
            OnHoverEndInternal();
        }

        OnInteractableChanged(wasInteractable, value);
    }

    protected virtual void OnInteractableChanged(bool oldValue, bool newValue) {
        // Переопределяется в наследниках для реакции на изменение интерактивности
    }

    #endregion

    #region Mouse Interaction (базовая реализация)

    protected virtual void HandleMouseEnter() {
        if (!isInteractable || _isHovered) return;

        _isHovered = true;
        OnHoverChanged?.Invoke(this, true);
        OnHoverStartInternal();
    }

    protected virtual void HandleMouseExit() {
        if (!isInteractable || !_isHovered) return;

        _isHovered = false;
        OnHoverChanged?.Invoke(this, false);
        OnHoverEndInternal();
    }

    protected virtual void HandleMouseDown() {
        if (!isInteractable) return;

        RaiseCardClickedEvent();
    }

    protected virtual void OnHoverStartInternal() {
        // Переопределяется в наследниках для специфичной логики начала наведения
    }

    protected virtual void OnHoverEndInternal() {
        // Переопределяется в наследниках для специфичной логики окончания наведения
    }

    protected virtual void RaiseCardClickedEvent() {
        if (isInteractable && _isInitialized) {
            OnCardClicked?.Invoke(this);
        }
    }

    #endregion

    #region Card Removal

    public virtual async UniTask RemoveCardView() {
        // Делаем карту неинтерактивной при удалении
        SetInteractable(false);

        // Проигрываем анимацию удаления
        await PlayRemovalAnimation();

        // Уничтожаем объект
        Destroy(gameObject);
    }

    protected virtual async UniTask PlayRemovalAnimation() {
        // Переопределяется в наследниках для анимации удаления
        await UniTask.CompletedTask;
    }

    #endregion

    #region Properties

    public bool IsInitialized => _isInitialized;
    public bool IsSelected => _isSelected;
    public bool IsHovered => _isHovered;
    public bool IsInteractable => isInteractable;

    #endregion
}