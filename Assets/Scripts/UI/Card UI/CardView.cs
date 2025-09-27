using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public abstract class CardView : UnitView {

    public Action<CardView> OnCardClicked;
    public Action<CardView, bool> OnHoverChanged;

    public string Id { get; set; }
    private bool _isInteractive = true;

    [SerializeField] protected MovementComponent movementComponent;
    [SerializeField] protected CardTiltController tiltController;
    [SerializeField] public Transform innerBody;

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
        DOTween.Kill(this); // Очищаем все анимации, связанные с этим объектом
    }

    #endregion

    #region Mouse Interaction (базовая реализация)

    protected virtual void HandleMouseEnter() {
        if (!_isInteractive) return;
        OnHoverChanged?.Invoke(this, true);
    }

    protected virtual void HandleMouseExit() {
        if (!_isInteractive) return;
        OnHoverChanged?.Invoke(this, false);
    }

    protected virtual void HandleMouseDown() {
        if (!_isInteractive) return;
        OnCardClicked?.Invoke(this);
    }

    #endregion

    #region Movement API - основне для інших модулів

    /// <summary>
    /// Плавний рух до позиції (для руки, реорганізації)
    /// </summary>
    public async UniTask DoTweener(Tweener tweener, CancellationToken token = default) {
        if (movementComponent != null) {
            await movementComponent.ExecuteTween(tweener, token);
        }
    }

    /// <summary>
    /// Плавний рух до позиції (для руки, реорганізації)
    /// </summary>
    public async UniTask DoSequence(Sequence sequence, CancellationToken token = default) {
        if (movementComponent != null) {
            await movementComponent.ExecuteTweenSequence(sequence, token);
        }

    }

    /// <summary>
    /// Почати фізичний рух (для драгу, таргетингу)
    /// </summary>
    public void DoPhysicsMovement(Vector3 initialPosition) {
        movementComponent?.UpdateContinuousTarget(initialPosition);
    }

    /// <summary>
    /// Зупинити всі рухи
    /// </summary>
    public void StopMovement() {
        movementComponent?.StopMovement();
    }

    public void ToggleTiling(bool enable) {
        tiltController.ToggleTiling(enable);
    }
    #endregion

    public abstract void UpdateDisplay(CardDisplayContext context);

    #region Render Order Management
    public abstract void SetRenderOrder(int sortingOrder);

    public abstract void ModifyRenderOrder(int modifyValue);

    public abstract void ResetRenderOrder();

    #endregion

    public abstract void SetHoverState(bool isHovered);

    public void SetInteractable(bool value) {
        _isInteractive = value;
    }
}