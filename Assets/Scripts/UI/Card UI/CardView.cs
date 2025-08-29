using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

public abstract class CardView : MonoBehaviour {
    
    public Action<CardView> OnCardClicked;
    public Action<CardView, bool> OnHoverChanged;

    public CardUIInfo uiInfo;
    public string Id { get; set; }
    private bool _isInteractive = true;

    [SerializeField] CardMovementComponent movementComponent;

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

    #region State Management

    public void SetInteractivity(bool enabled) {
        _isInteractive = enabled;
    }

    public virtual void Reset() {
        // Сбрасываем визуальные состояния
        ResetVisualState();
    }

    protected virtual void ResetVisualState() {
        // Переопределяется в наследниках для сброса визуального состояния
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
    public void MoveTo(Vector3 position, Quaternion rotation, Vector3 scale, float duration, System.Action onComplete = null) {
        movementComponent?.MoveTo(position, rotation, scale, duration, onComplete);
    }

    /// <summary>
    /// Плавний рух до позиції (для руки, реорганізації)
    /// </summary>
    public void MoveTo(Vector3 position, Quaternion rotation, float duration) {
        movementComponent?.MoveTo(position, rotation, transform.localScale, duration);
    }

    /// <summary>
    /// Миттєве переміщення
    /// </summary>
    public void SetPosition(Vector3 position, Quaternion rotation, Vector3 scale) {
        movementComponent?.SetPosition(position, rotation, scale);
    }

    /// <summary>
    /// Почати фізичний рух (для драгу, таргетингу)
    /// </summary>
    public void StartPhysicsMovement(Vector3 initialPosition) {
        movementComponent?.StartPhysicsMovement(initialPosition);
    }

    /// <summary>
    /// Оновлення цільової позиції в real-time
    /// </summary>
    public void UpdateTargetPosition(Vector3 position) {
        movementComponent?.UpdateTargetPosition(position);
    }

    /// <summary>
    /// Зупинити всі рухи
    /// </summary>
    public void StopMovement() {
        movementComponent?.StopMovement();
    }
    #endregion

    #region UI Info Update
    public void UpdateCost(int cost) {
        uiInfo.UpdateCost(cost);
    }

    public void UpdateName(string name) {
        uiInfo.UpdateName(name);
    }

    public void UpdateAttack(int attack) {
        uiInfo.UpdateAttack(attack);
    }

    public void UpdateHealth(int health) {
        uiInfo.UpdateHealth(health);
    }

    public void ToggleCreatureStats(bool isEnabled) {
        uiInfo.ToggleAttackText(isEnabled);
        uiInfo.TogglHealthText(isEnabled);
    }

    public void UpdatePortait(Sprite portait) {
        uiInfo.UpdatePortait(portait);
    }

    internal void UpdateBackground(Sprite bgImage) {
        uiInfo.UpdateBackground(bgImage);
    }

    public void UpdateRarity(Color rarity) {
        uiInfo.UpdateRarity(rarity);
    }
    #endregion

    #region Render Order Management
    public abstract void SetRenderOrder(int sortingOrder);

    public abstract void ModifyRenderOrder(int modifyValue);

    public abstract void ResetRenderOrder();

    #endregion

    public abstract void SetHoverState(bool isHovered);
}