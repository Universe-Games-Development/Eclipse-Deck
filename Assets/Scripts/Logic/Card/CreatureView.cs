using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

public class CreatureView : UnitView {
    [SerializeField] MovementComponent movementComponent;
    [SerializeField] private Renderer cardRenderer;
    [SerializeField] MovementComponent innerMovementComponent;

    // Автоматично знаходимо компоненти
    private CardDisplayComponent[] displayComponents;
    private Material _instancedMaterial;
    private int _defaultRenderQueue;

    protected void Awake() {
        displayComponents = GetComponentsInChildren<CardDisplayComponent>();
        
        InitializeMaterials();
    }
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
    public void SetRenderOrder(int order) {
        if (_instancedMaterial != null) {
            _instancedMaterial.renderQueue = order;
            //Debug.Log($"{gameObject.name}: Render queue: {order}");
        }
    }

    public void ModifyRenderOrder(int modifyValue) {
        if (TryGetCurrentRenderOrder(out int currentOrder)) {
            SetRenderOrder(currentOrder + modifyValue);
        }
    }

    public void ResetRenderOrder() {
        SetRenderOrder(_defaultRenderQueue);
    }

    private bool TryGetCurrentRenderOrder(out int order) {
        order = _instancedMaterial?.renderQueue ?? 0;
        return _instancedMaterial != null;
    }
    #endregion

    private void InitializeMaterials() {
        if (cardRenderer != null && cardRenderer.sharedMaterial != null) {
            _instancedMaterial = new Material(cardRenderer.sharedMaterial);
            cardRenderer.material = _instancedMaterial;
            _defaultRenderQueue = cardRenderer.sharedMaterial.renderQueue;
        }
    }

    public void UpdateDisplay(CardDisplayContext context) {
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

}
