using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public class Card3DView : CardView {
    [SerializeField] private Renderer cardRenderer;
    [SerializeField] private Card3DAnimator animator;
    [SerializeField] MovementComponent innerMovementComponent;

    // Автоматично знаходимо компоненти
    private CardDisplayComponent[] displayComponents;
    private Material _instancedMaterial;
    private int _defaultRenderQueue;

    protected override void Awake() {
        base.Awake();
        displayComponents = GetComponentsInChildren<CardDisplayComponent>();
        InitializeMaterials();
    }

    #region Movement API - основне для інших модулів

    /// <summary>
    /// Плавний рух до позиції (для руки, реорганізації)
    /// </summary>
    public async UniTask DoTweenerInner(Tweener tweener, CancellationToken token = default) {
        if (innerMovementComponent != null) {
            await innerMovementComponent.ExecuteTween(tweener, token);
        }
    }

    /// <summary>
    /// Плавний рух до позиції (для руки, реорганізації)
    /// </summary>
    public async UniTask DoSequenceInner(Sequence sequence, CancellationToken token = default) {
        if (innerMovementComponent != null) {
            await innerMovementComponent.ExecuteTweenSequence(sequence, token);
        }

    }
    #endregion

    #region Render Order Management
    public override void SetRenderOrder(int order) {
        if (_instancedMaterial != null) {
            _instancedMaterial.renderQueue = order;
            //Debug.Log($"{gameObject.name}: Render queue: {order}");
        }
    }

    public override void ModifyRenderOrder(int modifyValue) {
        if (TryGetCurrentRenderOrder(out int currentOrder)) {
            SetRenderOrder(currentOrder + modifyValue);
        }
    }

    public override void ResetRenderOrder() {
        SetRenderOrder(_defaultRenderQueue);
    }

    private bool TryGetCurrentRenderOrder(out int order) {
        order = _instancedMaterial?.renderQueue ?? 0;
        return _instancedMaterial != null;
    }
    #endregion

    public override void SetHoverState(bool isHovered) {
        animator?.Hover(isHovered);

    }

    private void InitializeMaterials() {
        if (cardRenderer?.sharedMaterial != null) {
            _instancedMaterial = new Material(cardRenderer.sharedMaterial);
            cardRenderer.material = _instancedMaterial;
            _defaultRenderQueue = cardRenderer.sharedMaterial.renderQueue;
        }
    }

    public override void UpdateDisplay(CardDisplayContext context) {
        foreach (var component in displayComponents) {
            component.UpdateDisplay(context);
        }

        UpdatePortait(context.Data.portrait);
        ToggleFrame(context.Config.showFrame);
    }

    public void UpdatePortait(Sprite portrait) {
        if (_instancedMaterial != null && portrait != null) {
            _instancedMaterial.SetTexture("_Portait", portrait.texture);
        }
    }

    public void ToggleFrame(bool isEnabled) {
        if (_instancedMaterial != null) {
            _instancedMaterial.SetFloat("_FrameMask", isEnabled ? 0f : -1f);
        }
    }
    #region Mouse Interaction

    private void OnMouseEnter() {
        HandleMouseEnter();
    }

    private void OnMouseExit() {
        HandleMouseExit();
    }

    private void OnMouseDown() {
        HandleMouseDown();
    }

    

    #endregion
}

