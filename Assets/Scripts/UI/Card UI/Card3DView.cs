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
            _instancedMaterial.SetFloat("_MaskStrength", isEnabled ? 1f : 0f);
        }
    }
}

