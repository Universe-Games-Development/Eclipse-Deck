using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

public abstract class CardView : MonoBehaviour {
    // События
    public Action<CardView> OnCardClicked;
    public Action<CardView, bool> OnHoverChanged;

    public CardUIInfo CardInfo;
    public string Id { get; set; }

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
        OnHoverChanged?.Invoke(this, true);
    }

    protected virtual void HandleMouseExit() {
        OnHoverChanged?.Invoke(this, false);
    }

    protected virtual void HandleMouseDown() {
        OnCardClicked?.Invoke(this);
    }

    #endregion

    #region Card Removal

    public virtual async UniTask RemoveCardView() {
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
}