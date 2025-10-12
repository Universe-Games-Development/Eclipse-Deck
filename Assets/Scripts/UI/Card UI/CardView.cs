using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public abstract class CardView : UnitView {
    public event Action<CardView> OnClicked;
    public event Action<CardView, bool> OnHoverChanged;

    [SerializeField] protected MovementComponent movementComponent;
    [SerializeField] protected CardTiltController tiltController;
    [SerializeField] public Transform innerBody;

    [SerializeField] InteractableBody interactableBody;

    #region Unity Lifecycle

    protected virtual void Awake() {
        ValidateComponents();
        SubscribeToInteractableBody();
    }

    private void SubscribeToInteractableBody() {
        if (interactableBody == null) {
            Debug.LogError($"InteractableBody not assigned on {gameObject.name}", this);
            return;
        }

        interactableBody.OnClicked += HandleBodyClicked;
        interactableBody.OnHoverChanged += HandleBodyHovered;
    }

    private void HandleBodyHovered(bool value) {
        OnHoverChanged?.Invoke(this, value);
    }

    private void HandleBodyClicked() {
        OnClicked?.Invoke(this);
    }

    public void SetInteractable(bool value) {
        interactableBody?.SetInteractable(value);
    }

    protected virtual void OnDestroy() {
        if (interactableBody != null) {
            interactableBody.OnClicked -= HandleBodyClicked;
            interactableBody.OnHoverChanged -= HandleBodyHovered;
        }

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

    public abstract void UpdateDisplay(CardDisplayContext context);

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
    public void DoPhysicsMovement(Vector3 targetPosition) {
        movementComponent?.UpdateContinuousTarget(targetPosition);
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

    #region Render Order Management
    public abstract void SetRenderOrder(int sortingOrder);

    public abstract void ModifyRenderOrder(int modifyValue);

    public abstract void ResetRenderOrder();

    #endregion

    public abstract void SetHoverState(bool isHovered);
}